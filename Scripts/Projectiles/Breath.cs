using Godot;
using System;

public partial class Breath : Cannonball
{
    public override void OnHit(Unit target = null)
    {
        if(target == null) {
            target = Target;
        }
        target.Damage(Damage, SourceWeapon, this);
    }
}
