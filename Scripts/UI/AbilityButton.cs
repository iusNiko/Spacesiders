using Godot;
using System;

public partial class AbilityButton : RichTextLabel
{
    public Ability[] Abilities;

    public override void _GuiInput(InputEvent @event)
    {
        if(@event is InputEventMouseButton mouseButton) {
            if(mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed) {
                foreach(Ability ability in Abilities) {
                    ability.Use();
                }
            }
        }
    }
}
