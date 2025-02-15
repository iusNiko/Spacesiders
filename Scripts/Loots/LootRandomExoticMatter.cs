using Godot;
using System;

public partial class LootRandomExoticMatter : Loot
{
    public Vector2 IconPosition;
    public float Value = 0;
    public LootRandomExoticMatter(string label, float value) : base(label) {
        Sprite.Texture = (Texture2D)ResourceLoader.Load("res://Art/Loot Icons/ExoticMatter.png");
        AddChild(Sprite);
        IconPosition = new Vector2(0, 0);
        Sprite.Position = IconPosition;
        Value = value;
        Sprite.Modulate = new Color(1.71f, 1f, 2f);
    }

    public override void PickUp(Unit unit)
    {
        GameManager.Instance.Resources["ExoticMatter"] += Value;
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