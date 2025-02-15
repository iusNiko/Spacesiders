using Godot;
public partial class PhotonTorpedo : DirectedProjectile
{
    [Export] public float ExplosionRadius = 12f;
    
    public override void OnHit(Unit target = null) {
        if(AlreadyHit) {
            return;
        }
        if(target == null) {
            target = Target;
        }
        foreach(Unit unit in World.Instance.GetUnitsInRadius(target.GlobalPosition, ExplosionRadius)) {
            unit.Damage(Damage, SourceWeapon, this);
        }
        
        if(AnimationSprite.SpriteFrames.HasAnimation("hit")) {
            AnimationSprite.Play("hit");
            AnimationSprite.AnimationFinished += OnAnimationFinished;
            AlreadyHit = true;
        }
        else {
            QueueFree();
            return;
        }
    }
}