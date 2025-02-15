using Godot;
using System;

public partial class Cannonball : DirectedProjectile
{
    public override void OnHit(Unit target = null)
    {
        if(target == null) {
            target = Target;
        }
        target.Damage(Damage, SourceWeapon, this);
        target.GlobalPosition += GlobalPosition.DirectionTo(target.GlobalPosition) * target.MoveSpeed * target.Delta * 4f;
        target.BlinkFrames = GameManager.Instance.STANDARD_BLINK_TIME;
    }
}
