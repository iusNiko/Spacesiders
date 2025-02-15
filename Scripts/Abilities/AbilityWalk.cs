using Godot;
using System;
using System.Linq;

public partial class AbilityWalk : Ability
{
    public override void Use()
    {
        if(World.Instance.HoveredUnits.Where(unit => unit.Team != Unit.Team && unit.Team != 0).Any()) {
            return;
        }
        if(World.Instance.HoveredLoot.Length > 0) {
            Loot target = World.Instance.HoveredLoot[0];
            Unit.OrderQueue.Enqueue(new OrderCollectLoot(Unit, target.GlobalPosition, target));
            return;
        }
        if(Unit.HasAbility(typeof(AbilityHarvest))) {
            AbilityHarvest[] abilities = Unit.Abilities.Where(a => a.GetType() == typeof(AbilityHarvest)).Cast<AbilityHarvest>().ToArray();
            for(int i = 0; i < abilities.Length; i++) {
                abilities[i].Target = null;
            }
        }
        Unit[] selectedUnitsWithWalk = World.Instance.SelectedUnits.Where(unit => unit.HasAbility(typeof(AbilityWalk))).ToArray();
        Order order;
        Vector2 groupCenter = new();

        for(int i = 0; i < selectedUnitsWithWalk.Length; i++) {
            groupCenter += selectedUnitsWithWalk[i].GlobalPosition;
        }
        groupCenter /= selectedUnitsWithWalk.Length;
        if(Unit.GetGlobalMousePosition().DistanceTo(groupCenter) < World.Instance.UnitClusteringThreshold * MathF.Log10(World.Instance.SelectedUnits.Length)) {
            order = new OrderWalk(Unit, Unit.GetGlobalMousePosition());
            Order = order;
        }
        else {
            order = new OrderWalk(Unit, Unit.GlobalPosition + (Unit.GetGlobalMousePosition() - groupCenter));
        }
        
        if(!Input.IsActionPressed("shift")) {
            Unit.ClearOrders();
        }

        Unit.OrderQueue.Enqueue(order);
    }
}