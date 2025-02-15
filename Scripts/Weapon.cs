using Godot;
using System;
using System.Linq;

public partial class Weapon : Node
{
    public Unit SourceUnit {
        get {
            return GetParent<Node>().GetParent<Unit>();
        }
    }
    [Export] public PackedScene ProjectilePrefab;
    [Export] float _attackSpeed = 0f;
    public float AttackSpeed {
        get {
            float attackSpeed = _attackSpeed;
            foreach(PropertyModifier modifier in SourceUnit.PropertyModifiers.Where(m => m.PropertyName == "AttackSpeed")) {
                attackSpeed += modifier.FlatModifier;
                attackSpeed *= modifier.PercentageModifier;
            }
            return attackSpeed;
        }
        set {
            _attackSpeed = value;
            if(_attackSpeed < 0f) {
                _attackSpeed = 0f;
            }
        }
    }
    [Export] float _range = 0f;
    public float Range {
        get {
            float range = _range;
            foreach(PropertyModifier modifier in SourceUnit.PropertyModifiers.Where(m => m.PropertyName == "Range")) {
                range += modifier.FlatModifier;
                range *= modifier.PercentageModifier;
            }
            return range;
        }
        set {
            _range = value;
            if(_range < 0f) {
                _range = 0f;
            }
        }
    }
    [Export] public Marker2D ShootPoint = null;
    public float TimeSinceLastShot = 0f;
    public float Damage {
        get {
            float damage = (float)GameManager.GetPropertyFromPackedScene(ProjectilePrefab, "_damage");
            foreach(PropertyModifier modifier in SourceUnit.PropertyModifiers.Where(m => m.PropertyName == "Damage")) {
                damage += modifier.FlatModifier;
                damage *= modifier.PercentageModifier;
            }
            return damage;
        }
    }
    public virtual void Shoot() {
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
    }
    public override void _PhysicsProcess(double delta)
    {
        TimeSinceLastShot += (float)delta;
    }
}
