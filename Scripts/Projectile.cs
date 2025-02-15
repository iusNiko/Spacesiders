using Godot;
using System;
using System.Linq;

public partial class Projectile : Area2D
{
    public Unit Target;
    public Weapon SourceWeapon;
    [Export] public float Speed = 100f;
    [Export] public float Acceleration = 10f;
    [Export] public float _damage = 0f;
    public float Damage {
        get {
            float damage = _damage;
            foreach(PropertyModifier modifier in SourceWeapon.SourceUnit.PropertyModifiers.Where(m => m.PropertyName == "Damage")) {
                damage += modifier.FlatModifier;
                damage *= modifier.PercentageModifier;
            }
            return damage;
        }
        set { _damage = value; }
    }
    [Export] float _armorPenetration = 0;
    public float ArmorPenetration {
        get {
            float armorPenetration = _armorPenetration;
            foreach(PropertyModifier modifier in SourceWeapon.SourceUnit.PropertyModifiers.Where(m => m.PropertyName == "ArmorPenetration")) {
                armorPenetration += modifier.FlatModifier;
                armorPenetration *= modifier.PercentageModifier;
            }
            return armorPenetration;
        }
        set { _armorPenetration = value; }
    }
    [Export] public bool FriendlyFire = false;
    public bool AlreadyHit = false;
    public AnimatedSprite2D AnimationSprite;

    public virtual void OnHit(Unit target = null) {
        if(AlreadyHit) {
            return;
        }
        if(target == null) {
            target = Target;
        }
        target.Damage(Damage, SourceWeapon, this);
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
    public virtual void OnAreaEntered(Area2D area) {
        if(area is Unit unit) {
            if(IsInstanceValid(SourceWeapon) && unit == SourceWeapon.SourceUnit) {
                return;
            }
            if(FriendlyFire == false && unit.Team == SourceWeapon.SourceUnit.Team) {
                return;
            }
            OnHit(unit);
        }
    }
    public virtual void OnAnimationFinished() {
        QueueFree();
    }
    public override void _Ready()
    {
        AnimationSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        AreaEntered += OnAreaEntered;
        ZIndex = 1000;
    }

    public override void _PhysicsProcess(double delta)
    {
        if(!IsInstanceValid(SourceWeapon)) {
            QueueFree();
            return;
        }
        if(!IsInstanceValid(Target)) {
            QueueFree();
            return;
        }   
        Vector2 velocity = GlobalPosition.DirectionTo(Target.GlobalPosition) * (float)delta * Speed;
        LookAt(Target.GlobalPosition);
        if(velocity.Length() > GlobalPosition.DistanceTo(Target.GlobalPosition))
        {
            GlobalPosition = Target.GlobalPosition;
            OnHit();
            return;
        }
        else {
            GlobalPosition += velocity;
            Speed += Acceleration;
        }
    }
}
