namespace Robots;

using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.RapidDomain;
using ABB.Robotics.Controllers.Discovery;

public class RemoteAbb : IRemote
{
    Controller? _controller;

    public string? IP { get; set; }
    public List<string> Log { get; } = [];
    public bool Connected => _controller?.Connected == true;

    internal RemoteAbb()
    { }

    public void Play() => Command(StartCommand);
    public void Pause() => Command(StopCommand);
    public void Upload(IProgram program) => Command(() => UploadCommand(program));

    void AddLog(string text)
    {
        Log.Insert(0, $"{DateTime.Now.ToLongTimeString()} - {text}");
    }

    public void Connect()
    {
        Disconnect();
        var scanner = new NetworkScanner();
        scanner.Scan();

        var controllerInfo = (IP is not null)
            ? scanner.Controllers.FirstOrDefault(c => c.IPAddress.ToString() == IP)
            : scanner.Controllers.FirstOrDefault();

        if (controllerInfo is null)
        {
            AddLog("Controller not found.");
            return;
        }

        if (controllerInfo.Availability != Availability.Available)
        {
            AddLog("Controller not available.");
            return;
        }

        _controller = Controller.Connect(controllerInfo, ConnectionType.Standalone);
        _controller.Logon(UserInfo.DefaultUser);

        if (!Connected)
        {
            AddLog("Could not connect to controller.");
            return;
        }
    }

    public void Disconnect()
    {
        if (_controller is null)
            return;

        _controller.Logoff();
        _controller.Dispose();
        _controller = null;
    }

    void Command(Func<string> command)
    {
        Connect();

        if (Connected)
            AddLog(command());

        Disconnect();
    }

    string StartCommand()
    {
        if (_controller is null)
            return "Controller is null.";

        if (_controller.OperatingMode != ControllerOperatingMode.Auto)
            return "Controller not set to automatic.";

        if (_controller.State != ControllerState.MotorsOn)
            return "Motors not on.";

        using Mastership master = Mastership.Request(_controller);
        _controller.Rapid.Start(RegainMode.Continue, ExecutionMode.Continuous, ExecutionCycle.Once, StartCheck.CallChain);
        return "Program started.";
    }

    string StopCommand()
    {
        if (_controller is null)
            return "Controller is null.";

        if (_controller.OperatingMode != ControllerOperatingMode.Auto)
            return "Controller not set to automatic.";

        using Mastership master = Mastership.Request(_controller);
        _controller.Rapid.Stop(StopMode.Instruction);
        return "Program stopped.";
    }

    string UploadCommand(IProgram program)
    {
        if (_controller is null)
            return "Controller is null.";

        string tempPath = Path.Combine(Path.GetTempPath(), "Robots");

        try
        {
            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath, true);

            Directory.CreateDirectory(tempPath);
            program.Save(tempPath);

            string localFolder = Path.Combine(tempPath, program.Name);
            string robotFolder = Path.Combine(_controller.FileSystem.RemoteDirectory, program.Name);
            string programPath = Path.Combine(robotFolder, $"{program.Name}_T_ROB1.pgf");

            _controller.AuthenticationSystem.DemandGrant(Grant.WriteFtp);
            _controller.FileSystem.PutDirectory(localFolder, program.Name, true);
            AddLog($"Program {program.Name} copied to {_controller.Name}...");

            if (_controller.OperatingMode != ControllerOperatingMode.Auto)
                return $"Skipping loading program to {_controller.Name}. Set controller to automatic to allow loading remotely.";

            using var master = Mastership.Request(_controller);
            _controller.AuthenticationSystem.DemandGrant(Grant.LoadRapidProgram);
            using var task = _controller.Rapid.GetTasks().First();
            task.DeleteProgram();

            int count = 0;

            while (count++ < 10)
            {
                try
                {
                    if (task.LoadProgramFromFile(programPath, RapidLoadMode.Replace))
                        return $"Program {program.Name} loaded to {_controller.Name}.";
                }
                catch (Exception)
                {
                    Thread.Sleep(300);
                    AddLog($"Retrying loading program... ({count}/10)");
                }
            }

            throw new OperationCanceledException("Could not load program after 10 attempts.");
        }
        catch (Exception e)
        {
            return $"Error: {e.Message}";
        }
        finally
        {
            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath, true);
        }
    }

    public JointTarget GetPosition()
    {
        if (!Connected)
            Connect();

        if (_controller is null)
            throw new ArgumentNullException(nameof(_controller));

        var joints = _controller.MotionSystem.ActiveMechanicalUnit.GetPosition();
        var values = new double[] { joints.RobAx.Rax_1, joints.RobAx.Rax_2, joints.RobAx.Rax_3, joints.RobAx.Rax_4, joints.RobAx.Rax_5, joints.RobAx.Rax_6 };

        for (int i = 0; i < 6; i++)
            values[i] = RobotAbb.ABBDegreeToRadian(values[i], i);

        var target = new JointTarget(values);
        return target;
    }
}
