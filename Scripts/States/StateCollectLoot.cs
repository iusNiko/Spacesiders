using Godot;
using System;
using System.Linq;

public partial class StateCollectLoot : State
{
    Loot Loot;
    public StateCollectLoot(Loot loot) {
        Loot = loot;
    }
    public override void Enter() {
        
    }
    public override void Exit() {
        
    }
    public override void Update(float delta) {
        Unit unit = StateManager.Unit;

        if(World.Instance.Loots.Where(l => l == Loot).Any() == false) {
            StateManager.ChangeState(new StateIdle());
            return;
        }

        unit.MovementTarget = unit.GlobalPosition;

        if(unit.GlobalPosition.DistanceTo(Loot.GlobalPosition) < World.Instance.LootCollectionRange) {
            Loot.PickUp(unit);
            StateManager.ChangeState(new StateIdle());
            return;
        }

        if(unit.IsAttacking) {
            unit.LookAt(unit.AttackTarget.GlobalPosition);
            unit.RotationDegrees += 90 + unit.BonusAttackRotationDegrees;
        }
        else {
            unit.LookAt(Loot.GlobalPosition);
            unit.RotationDegrees += 90;
        }
        
        Vector2 velocity = unit.GlobalPosition.DirectionTo(Loot.GlobalPosition) * unit.MoveSpeed * unit.Delta;
       
        unit.GlobalPosition += velocity;
        
    }
}