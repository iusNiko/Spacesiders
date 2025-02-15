using Godot;
using System;

public partial class DirectedProjectile : Projectile
{
    public Vector2 Direction;
    [Export] public float MaxLifetime = 15f;
    public float LifeTime = 0f;
    public override void _Ready()
    {
        Direction = GlobalPosition.DirectionTo(Target.GlobalPosition);
        LookAt(Target.GlobalPosition);
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        if(!IsInstanceValid(SourceWeapon)) {
            QueueFree();
            return;
        }
        GlobalPosition += Direction * (float)delta * Speed;
        LifeTime += (float)delta;
        if(LifeTime >= MaxLifetime) {
            QueueFree();
        }
    }
}