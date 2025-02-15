using Godot;
using System;
using System.Linq;

public partial class TownPortalButton : TextureRect {
    public bool Triggered = false;
    public float CastTime = 5.0f;
    public float TimeSinceTriggered = 0.0f;
    public override void _GuiInput(InputEvent @event)
    {
        if(@event is InputEventMouseButton mouseButton) {
            if(mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed) {
                Trigger();
            }
        }
    }

    void Trigger() {
        Triggered = true;
        TimeSinceTriggered = 0.0f;
    }

    public override void _PhysicsProcess(double delta)
    {
        if(TimeSinceTriggered < CastTime && Triggered) {
            TimeSinceTriggered += (float)delta;
            Unit[] units = World.Instance.Units.Where(u => u.Team == 1).ToArray();
            foreach(Unit unit in units) {
                unit.Modulate = new Color(1.0f, 2.5f, 1.0f);
            }
            
            if(TimeSinceTriggered >= CastTime * 0.8f) {
                foreach(Unit unit in units) {
                    unit.Modulate = new Color(1.5f, 2.0f, 4.0f);
                }
            }
        }
        else if(Triggered) {
            World.Instance.ChangeMap("TehhanasRefuge");
            Triggered = false;
        }
    }
}