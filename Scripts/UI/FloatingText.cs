using Godot;
using System;

public partial class FloatingText : Label
{
    public int Frame = 0;
    Vector2 Velocity = new Vector2(0, -1);

    public static void CreateFloatingText(string text, Color color, Vector2 position) {
        FloatingText floatingText = new FloatingText(text, color, position);
        World.Instance.AddChild(floatingText);
    }
    FloatingText(string text, Color color, Vector2 position) {
        ZIndex = 4096;
        Text = text;
        Modulate = color;
        GlobalPosition = position;
        Scale = new Vector2(1/World.Instance.Camera.MaxZoom, 1/World.Instance.Camera.MaxZoom);
        LabelSettings = new LabelSettings();
        LabelSettings.FontSize = 40;
    }

    public override void _Process(double delta)
    {
        Frame++;
        Velocity.Y += 1f/60f;
        GlobalPosition += Velocity;
        Modulate -= new Color(0, 0, 0, 1/60f);
        if(Frame > 60) {
            QueueFree();
        }
    }
}