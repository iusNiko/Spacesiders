using Godot;
using System;

public partial class RammingWeapon : Weapon
{
	public override void Shoot() {
        TimeSinceLastShot = 0f;
        SourceUnit.AttackTarget.Alert(SourceUnit.GlobalPosition);
        if(ShootPoint == null) {
            ShootPoint = SourceUnit.GetNode<Node2D>("ShootPoints").GetChildren().PickRandom() as Marker2D;
        }
        Projectile projectile = ProjectilePrefab.Instantiate<Projectile>();
        projectile.SourceWeapon = this;
        projectile.Target = SourceUnit.AttackTarget;
        projectile.GlobalPosition = ShootPoint.GlobalPosition;
        World.Instance.AddChild(projectile);
        projectile.GlobalPosition = ShootPoint.GlobalPosition;
		SourceUnit.Health -= MathF.Min(SourceUnit.AttackTarget.Armor / 2f, 10f);
    }
}
