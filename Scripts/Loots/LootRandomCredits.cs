using Godot;
using System;

public partial class LootRandomCredits : Loot
{
    public Vector2 IconPosition;
    public float Value = 0;
    public LootRandomCredits(string label, Rarity lootRarity, float value) : base(label, lootRarity) {
        Sprite.Texture = (Texture2D)ResourceLoader.Load("res://Art/Loot Icons/Credits.png");
        AddChild(Sprite);
        IconPosition = new Vector2(0, 0);
        Sprite.Position = IconPosition;
        Value = value * World.Instance.GetRarityValue(lootRarity);
    }

    public override void PickUp(Unit unit)
    {
        GameManager.Instance.Resources["Credits"] += Value;
        base.PickUp(unit);
    }

    public override void _Process(double delta)
    {
        Unit unit = World.Instance.GetNearestUnit(GlobalPosition, 1);
        if(unit == null) {
            return;
        }
        if(unit.GlobalPosition.DistanceTo(GlobalPosition) < 20) {
            PickUp(unit);
        }
    }

}