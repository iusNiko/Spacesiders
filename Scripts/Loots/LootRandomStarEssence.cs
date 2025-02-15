using Godot;
using System;

public partial class LootRandomStarEssence : Loot
{
    public Vector2 IconPosition;
    public float Value = 0;
    public LootRandomStarEssence(string label, float value) : base(label) {
        Sprite.Texture = (Texture2D)ResourceLoader.Load("res://Art/Loot Icons/StarEssence.png");
        AddChild(Sprite);
        IconPosition = new Vector2(0, 0);
        Sprite.Position = IconPosition;
        Value = value;
        Sprite.Modulate = new Color(2f, 1.9f, 1.34f);
    }

    public override void PickUp(Unit unit)
    {
        GameManager.Instance.Resources["StarEssence"] += Value;
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