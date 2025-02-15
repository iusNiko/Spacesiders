using Godot;
using System;

public partial class OrderCollectLoot : Order
{
    Loot TargetLoot;
    public OrderCollectLoot(Unit unit, Vector2 targetPosition, Loot targetLoot, Unit targetUnit = null) : base(unit, targetPosition, targetUnit)
    {
        TargetLoot = targetLoot;
    }
    public override void Execute()
    {
        Unit.StateManager.ChangeState(new StateCollectLoot(TargetLoot));
    }
    public override void Canceled()
    {
        Unit.MovementTarget = Unit.GlobalPosition;
    }
    public override bool IsFinished()
    {
        return Unit.StateManager.CurrentState is not StateCollectLoot;
    }
}