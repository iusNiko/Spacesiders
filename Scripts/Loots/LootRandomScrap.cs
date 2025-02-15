using Godot;
using System;

public partial class LootRandomScrap : Loot
{
    public Vector2 IconPosition;
    public float Value = 0;
    public LootRandomScrap(string label, float value) : base(label) {
        Sprite.Texture = (Texture2D)ResourceLoader.Load("res://Art/Loot Icons/Scrap.png");
        AddChild(Sprite);
        IconPosition = new Vector2(0, 0);
        Sprite.Position = IconPosition;
        Value = value;
        Sprite.Modulate = new Color(1.6f, 1.6f, 1.6f);
    }

    public override void PickUp(Unit unit)
    {
        GameManager.Instance.Resources["Scrap"] += Value;
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