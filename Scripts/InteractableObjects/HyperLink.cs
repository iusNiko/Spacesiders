using Godot;
using System;

public partial class HyperLink : InteractableObject
{
	[Export] public string NextMap = "";
	public override void _Ready() {
        base._Ready();
        Sprite = new Sprite2D();
        Sprite.Texture = (Texture2D)ResourceLoader.Load("res://Art/InteractableObjects/HyperLink.png");
        AddChild(Sprite);
        Sprite.Position = new Vector2(0, 0);
    }

    public override void Use() {
		if(!CanUse()) return;
		if(NextMap != "") {
			World.Instance.ChangeMap(NextMap);
			return;
		}
		if(World.Instance.Map.NextMap != "") {
			World.Instance.ChangeMap(World.Instance.Map.NextMap);
			return;
		}
		GameManager.Instance.Log("No next map specified for " + World.Instance.Map.MapName);
		World.Instance.ChangeMap("TehhanasRefuge");
    }
}
