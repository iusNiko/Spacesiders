using Godot;
using System;

public partial class Foundry : InteractableObject
{
	public override void _Ready() {
        base._Ready();
		CollisionShape.Shape = new RectangleShape2D();
		((RectangleShape2D)CollisionShape.Shape).Size = new Vector2(32, 50);
        Sprite = new Sprite2D();
        Sprite.Texture = (Texture2D)ResourceLoader.Load("res://Art/InteractableObjects/Foundry.png");
        AddChild(Sprite);
        Sprite.Position = new Vector2(0, 0);
        MaxDistance = 9999f;
    }

    public override void Use() {
        if(!CanUse()) return;
        Vendors.Instance.GetNode<Control>("FoundryUI").Show();
        Vendors.Instance.LastVendorPosition = GlobalPosition;
    }
}
