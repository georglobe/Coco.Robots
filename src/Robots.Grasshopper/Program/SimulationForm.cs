using System.ComponentModel;
using Eto.Drawing;
using Eto.Forms;

namespace Robots.Grasshopper;

class SimulationForm : ComponentForm
{
    readonly Simulation _component;

    internal readonly Eto.Forms.CheckBox Play;

    public SimulationForm(Simulation component)
    {
        _component = component;

        Title = "Playback";
        MinimumSize = new Eto.Drawing.Size(0, 200);

        Padding = new Eto.Drawing.Padding(5);

        var font = new Eto.Drawing.Font(FontFamilies.Sans, 14, Eto.Drawing.FontStyle.None, FontDecoration.None);
        var size = new Eto.Drawing.Size(35, 35);

        Play = new Eto.Forms.CheckBox
        {
            Text = "\u25B6",
            Size = size,
            Font = font,
            Checked = false,
            TabIndex = 0
        };

        Play.CheckedChanged += (s, e) => component.TogglePlay();

        var stop = new Eto.Forms.Button
        {
            Text = "\u25FC",
            Size = size,
            Font = font,
            TabIndex = 1
        };

        stop.Click += (s, e) => component.Stop();

        var slider = new Slider
        {
            Orientation = Eto.Forms.Orientation.Vertical,
            Size = new Eto.Drawing.Size(-1, -1),
            TabIndex = 2,
            MaxValue = 400,
            MinValue = -200,
            TickFrequency = 100,
            SnapToTick = true,
            Value = 100,
        };

        slider.ValueChanged += (s, e) => component.Speed = (double)slider.Value / 100.0; ;

        var speedLabel = new Eto.Forms.Label
        {
            Text = "100%",
            VerticalAlignment = VerticalAlignment.Center,
        };

        var layout = new DynamicLayout();
        layout.BeginVertical();
        layout.AddSeparateRow(padding: new Eto.Drawing.Padding(10), spacing: new Eto.Drawing.Size(10, 0), controls: [Play, stop]);
        layout.BeginGroup("Speed");
        layout.AddSeparateRow(slider, speedLabel);
        layout.EndGroup();
        layout.EndVertical();

        Content = layout;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        _component.Stop();
        base.OnClosing(e);
    }
}
