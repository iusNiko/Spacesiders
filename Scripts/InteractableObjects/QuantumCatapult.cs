using Godot;
using System;

public partial class QuantumCatapult : InteractableObject
{
    public override void _Ready() {
        base._Ready();
        Sprite = new Sprite2D();
        Sprite.Texture = (Texture2D)ResourceLoader.Load("res://Art/InteractableObjects/QuantumCatapult.png");
        AddChild(Sprite);
        Sprite.Position = new Vector2(0, 0);
    }

    public override void Use() {
        if(!CanUse()) return;
        World.Instance.ChangeMap("WesternBorderlands1");
    }
}