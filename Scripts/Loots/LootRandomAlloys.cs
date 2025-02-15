using Godot;
using System;

public partial class LootRandomAlloys : Loot
{
    public Vector2 IconPosition;
    public float Value = 0;
    public LootRandomAlloys(string label, float value) : base(label) {
        Sprite.Texture = (Texture2D)ResourceLoader.Load("res://Art/Loot Icons/Alloys.png");
        AddChild(Sprite);
        IconPosition = new Vector2(0, 0);
        Sprite.Position = IconPosition;
        Value = value;
        Sprite.Modulate = new Color(1.31f, 1.76f, 2f);
    }

    public override void PickUp(Unit unit)
    {
        GameManager.Instance.Resources["Alloys"] += Value;
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