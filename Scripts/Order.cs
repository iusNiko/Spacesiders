using Godot;
using System;

public partial class Order
{
    public Unit Unit;
    public Vector2 TargetPosition;
    public Unit TargetUnit;
    public bool Executed = false;
    public Order(Unit unit, Vector2 targetPosition, Unit targetUnit = null)
    {
        Unit = unit;
        if(targetPosition != Vector2.Zero) {
            TargetPosition = targetPosition;
        }
        TargetUnit = targetUnit;
    }
    public virtual void Execute()
    {
        
    }
    public virtual void Update(float delta)
    {

    }
    public virtual void Finished()
    {

    }
    public virtual void Canceled() {
        
    }
    public virtual bool IsFinished()
    {
        return true;
    }
}
