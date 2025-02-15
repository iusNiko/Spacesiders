using Godot;
using System;
using System.Linq;

public partial class InteractableObject : Area2D
{
    [Export] public string Label;
    public Sprite2D Sprite;
    public CollisionShape2D CollisionShape;
    public float MaxDistance = 50f;
    public Unit[] GuardingUnits = Array.Empty<Unit>();
    
    public override void _Ready() {
        ZIndex = 10;

        CollisionShape = new CollisionShape2D();
        CollisionShape.Shape = new CircleShape2D();
        ((CircleShape2D)CollisionShape.Shape).Radius = 9;
        AddChild(CollisionShape);
        CollisionShape.Position = new Vector2(0, 0);
        World.Instance.InteractableObjects = World.Instance.InteractableObjects.Append(this).ToArray();
    }

    public override void _MouseEnter()
    {
        World.Instance.HoveredInteractableObjects = World.Instance.HoveredInteractableObjects.Append(this).ToArray();
        Modulate += new Color(0.2f, 0.2f, 0.2f);
    }

    public override void _MouseExit()
    {
        World.Instance.HoveredInteractableObjects = World.Instance.HoveredInteractableObjects.Where(x => x != this).ToArray();
        Modulate -= new Color(0.2f, 0.2f, 0.2f);
    }

    public virtual bool CanUse() {
        if(!World.Instance.Units.Where(u => u.Team == 1 && u.GlobalPosition.DistanceTo(GlobalPosition) < MaxDistance).Any()) {
            return false;
        }
        if(GuardingUnits.Where(u => !u.IsDead).Any()) {
            return false;
        }
        return true;
    }

    public virtual void Use() {
        
    }

}
