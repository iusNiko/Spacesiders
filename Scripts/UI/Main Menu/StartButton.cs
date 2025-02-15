using Godot;
using System;
using System.Linq;

public partial class StartButton : TextureRect
{
    public ItemList SavesList;

    public override void _Ready()
    {
        SavesList = GetNode<ItemList>("../SavesList");
    }
    public override void _GuiInput(InputEvent @event)
    {
        if(@event is InputEventMouseButton mouseButton) {
            if(mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed && SavesList.GetSelectedItems().Length == 1) {
                GameManager.Instance.LoadSave(SavesList.GetItemText(SavesList.GetSelectedItems()[0]));
                GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
            }
        }
    }
}
