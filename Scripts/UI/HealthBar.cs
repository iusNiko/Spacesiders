using Godot;
using System;

public partial class HealthBar : ProgressBar
{
    public Unit Parent {
        get { return GetParent<Unit>(); }
    }

    Vector2 UnitStartPos;
    Vector2 StartPos;

    public override void _Ready()
	{
		if(Parent.Team == 2) {
			Modulate = new Color(1, 0, 0);
		}
		else if(Parent.Team == 1) {
			Modulate = new Color(0, 1, 0);
		}
		else {
			Modulate = new Color(1, 1, 1);
		}
        UnitStartPos = Parent.GlobalPosition;
        StartPos = GlobalPosition;
	}

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition = StartPos + (Parent.GlobalPosition - UnitStartPos);
        Rotation = -Parent.Rotation;
        MaxValue = Parent.MaxHealth;
        Value = Parent.Health;
        if(Value != MaxValue) {
            Visible = true;
        }
        else {
            Visible = false;
        }
    }
}
