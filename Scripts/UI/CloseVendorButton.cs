using Godot;
using System;

public partial class CloseVendorButton : TextureRect
{
	public override void _GuiInput(InputEvent @event)
    {
        if(@event is InputEventMouseButton mouseButton) {
            if(mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed) {
                foreach(Control child in Vendors.Instance.GetChildren()) {
					child.Hide();
				}
            }
        }
    }
}
