using Godot;
using System;

public partial class StateWalking : State
{
    public override void Enter() {
        
    }
    public override void Exit() {
        
    }
    public override void Update(float delta) {
        Unit unit = StateManager.Unit;

        if(unit.GlobalPosition == unit.MovementTarget) {
            StateManager.ChangeState(new StateIdle());
            return;
        }

        if(unit.IsAttacking) {
            unit.LookAt(unit.AttackTarget.GlobalPosition);
            unit.RotationDegrees += 90 + unit.BonusAttackRotationDegrees;
        }
        else {
            unit.LookAt(unit.MovementTarget);
            unit.RotationDegrees += 90;
        }
        
        Vector2 velocity = unit.GlobalPosition.DirectionTo(unit.MovementTarget) * unit.MoveSpeed * unit.Delta;
       
        
        
        if(velocity.Length() > unit.GlobalPosition.DistanceTo(unit.MovementTarget)) {
            unit.GlobalPosition = unit.MovementTarget;
            StateManager.ChangeState(new StateIdle());
            return;
        }
        unit.GlobalPosition += velocity;
    }
}