using Godot;
using System;

public partial class OrderWalk : Order
{
    public OrderWalk(Unit unit, Vector2 targetPosition, Unit targetUnit = null) : base(unit, targetPosition, targetUnit)
    {
    }
    public override void Execute()
    {
        Unit.MovementTarget = TargetPosition;
        Unit.AttackTarget = null;
        Unit.StateManager.ChangeState(new StateWalking());
    }
    public override void Canceled()
    {
        Unit.MovementTarget = Unit.GlobalPosition;
    }
    public override bool IsFinished()
    {
        return Unit.StateManager.CurrentState is not StateWalking;
    }
}