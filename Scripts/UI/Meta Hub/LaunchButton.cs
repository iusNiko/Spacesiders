using Godot;
using System;
using System.Linq;

public partial class LaunchButton : TextureRect
{
    public override void _GuiInput(InputEvent @event)
    {
        if(@event is InputEventMouseButton mouseButton) {
            if(mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed) {
                
                GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
            }
        }
    }
}
