using Godot;
using System;

public partial class StateIdle : State
{
    public override void Enter() {
        
    }
    public override void Exit() {

    }
    public override void Update(float delta) {
        
        Unit unit = StateManager.Unit;

        if(unit.IsAttacking) {
            unit.LookAt(unit.AttackTarget.GlobalPosition);   
            unit.RotationDegrees += 90 + unit.BonusAttackRotationDegrees;
        }
        
        foreach(Area2D area in unit.GetOverlappingAreas()) {
            if(area is Unit otherUnit) {
                if(otherUnit.Team == unit.Team || otherUnit.Team == 0) {
                    if(otherUnit.GlobalPosition != unit.GlobalPosition) {
                        unit.GlobalPosition += otherUnit.GlobalPosition.DirectionTo(unit.GlobalPosition) * unit.MoveSpeed * delta / 2;
                    }
                    else {
                        switch(World.Instance.RNG.RandiRange(0, 3)) {
                            case 0:
                                unit.GlobalPosition += Vector2.Up * unit.MoveSpeed * delta / 6;
                                break;
                            case 1:
                                unit.GlobalPosition += Vector2.Down * unit.MoveSpeed * delta / 6;
                                break;
                            case 2:
                                unit.GlobalPosition += Vector2.Left * unit.MoveSpeed * delta / 6;
                                break;
                            case 3:
                                unit.GlobalPosition += Vector2.Right * unit.MoveSpeed * delta / 6;
                                break;
                        }
                    }
                    unit.MovementTarget = unit.GlobalPosition;
                }
            }
        }
    }
}
