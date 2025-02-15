using Godot;
using System;

public partial class LootRandomUnit : Loot
{
    public Vector2 IconPosition;
    public string UnitToSpawn;
    public LootRandomUnit(string label, Rarity lootRarity, string unitToSpawn) : base(label, lootRarity) {
        Sprite.Texture = (Texture2D)ResourceLoader.Load("res://Art/Loot Icons/InstaShip.png");
        AddChild(Sprite);
        IconPosition = new Vector2(0, 0);
        Sprite.Position = IconPosition;
        UnitToSpawn = unitToSpawn;
    }

    public override void PickUp(Unit unit)
    {
        World.Instance.CreateUnit(UnitToSpawn, GlobalPosition, 1, LootRarity);
        base.PickUp(unit);
    }
}