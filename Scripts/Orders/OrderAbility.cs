using Godot;
using System;
public partial class OrderAbility : Order
{
    public Ability Ability;
    public OrderAbility(Unit unit, Ability ability, Vector2 targetPosition, Unit targetUnit = null) : base(unit, targetPosition, targetUnit)
    {
        Ability = ability;
    }
    public override void Execute()
    {
        Ability.Execute();
    }
    public override void Update(float delta)
    {
        Ability.Update(delta);
    }
    public override bool IsFinished()
    {
        return Ability.IsFinished();
    }
}