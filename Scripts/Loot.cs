using Godot;
using System;
using System.Linq;

public partial class Loot : Area2D
{
    public enum Rarity {
        Common,
        Rare,
        Epic,
        Legendary,
    }
    public Rarity LootRarity;
    public string Label;
    public Sprite2D Sprite;
    public Sprite2D Glow;
    public CollisionShape2D CollisionShape;
    public Panel HighlightBorder;
    
    public Loot(string label, Rarity lootRarity = Rarity.Common) {
        Label = label;
        LootRarity = lootRarity;

        ZIndex = -10;

        Sprite = new Sprite2D();

        CollisionShape = new CollisionShape2D();
        CollisionShape.Shape = new CircleShape2D();
        ((CircleShape2D)CollisionShape.Shape).Radius = 9;
        AddChild(CollisionShape);
        CollisionShape.Position = new Vector2(0, 0);

        World.Instance.Loots = World.Instance.Loots.Append(this).ToArray();

        if(this is not LootRandomCredits lrc) {
            return;
        }

        switch(LootRarity) {
            case Rarity.Common:
                Sprite.Modulate = new Color(1.6f, 1.6f, 1.6f);
                break;
            case Rarity.Rare:
                Sprite.Modulate = new Color(0f, 0f, 2f);
                break;
            case Rarity.Epic:
                Sprite.Modulate = new Color(2f, 0f, 2f);
                break;
            case Rarity.Legendary:
                Sprite.Modulate = new Color(3f, 1.9f, 0f);
                break;
        }

        
    }

    public override void _MouseEnter()
    {
        World.Instance.HoveredLoot = World.Instance.HoveredLoot.Append(this).ToArray();
        Modulate += new Color(0.2f, 0.2f, 0.2f);
    }

    public override void _MouseExit()
    {
        World.Instance.HoveredLoot = World.Instance.HoveredLoot.Where(x => x != this).ToArray();
        Modulate -= new Color(0.2f, 0.2f, 0.2f);
    }

    public virtual void PickUp(Unit unit) {
        World.Instance.HoveredLoot = World.Instance.HoveredLoot.Where(x => x != this).ToArray();
        World.Instance.Loots = World.Instance.Loots.Where(x => x != this).ToArray();
        QueueFree();
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach(Area2D area in GetOverlappingAreas()) {
            if(area is Loot otherLoot) {
                if(otherLoot.GlobalPosition != GlobalPosition) {
                    GlobalPosition += otherLoot.GlobalPosition.DirectionTo(GlobalPosition) * 100 * (float)delta;
                }
                else {
                    switch(World.Instance.RNG.RandiRange(0, 3)) {
                        case 0:
                            GlobalPosition += Vector2.Up * 100 * (float)delta;
                            break;
                        case 1:
                            GlobalPosition += Vector2.Down * 100 * (float)delta;
                            break;
                        case 2:
                            GlobalPosition += Vector2.Left * 100 * (float)delta;
                            break;
                        case 3:
                            GlobalPosition += Vector2.Right * 100 * (float)delta;
                            break;
                    }
                }
            }
         }
    }

}
