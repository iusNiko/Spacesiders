using Godot;
using System;

public partial class Background : TileMap
{
    public override void _Ready()
    {
        GlobalPosition = World.Instance.Camera.GlobalPosition;
    }
    public override void _Process(double delta)
	{
		Vector2 movement = new Vector2(0, 0);
		if (World.Instance.Camera.GlobalPosition.X - GlobalPosition.X > 0)
		{
			movement.X = 1;
		}
		else if (World.Instance.Camera.GlobalPosition.X - GlobalPosition.X < 0)
		{
			movement.X = -1;
		}
		if (World.Instance.Camera.GlobalPosition.Y - GlobalPosition.Y > 0)
		{
			movement.Y = 1;
		}
		else if (World.Instance.Camera.GlobalPosition.Y - GlobalPosition.Y < 0)
		{
			movement.Y = -1;
		}
		if (movement != new Vector2(0, 0))
		{
			GlobalPosition += movement * 1024;
		}
	}
}
