using Godot;
using System;

public partial class SelectionBox : Panel
{
    public Area2D Area2D;
    public Vector2 DragStartingPosition;
    [Signal] public delegate void AreaSelectedEventHandler(Area2D area);

    public override void _Ready()
    {
        Area2D = GetNode<Area2D>("Area2D");
        AreaSelected += World.Instance.OnAreaSelected;
    }

    public override void _Process(double delta)
    {
        if(Input.IsActionJustPressed("left_click")) {
            DragStartingPosition = GetGlobalMousePosition();
            GlobalPosition = DragStartingPosition;
            Visible = true;
        }
        if(Input.IsActionPressed("left_click")) {

            GlobalPosition = new Vector2(Math.Min(DragStartingPosition.X, GetGlobalMousePosition().X), Math.Min(DragStartingPosition.Y, GetGlobalMousePosition().Y));
            Vector2 size = GetGlobalMousePosition() - DragStartingPosition;
            if(size.X < 0) size.X *= -1;
            if(size.Y < 0) size.Y *= -1;
            Size = size;
            Area2D.Scale = size;
        }
        if(Input.IsActionJustReleased("left_click")) {
            Visible = false;
            EmitSignal("AreaSelected", Area2D);
        }
    }
}
