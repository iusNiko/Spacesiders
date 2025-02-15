using Godot;
using System;

public partial class FoundryForgeButton : TextureRect
{
    public override void _GuiInput(InputEvent @event)
    {
        if(@event is InputEventMouseButton mouseButton) {
            if(mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed) {
                FoundryUI foundryUI = (FoundryUI)Vendors.Instance.GetNode("FoundryUI");
                string lackingResource = GameManager.Instance.CanAfford(foundryUI.CostTypes, foundryUI.CostAmounts);
                if(lackingResource != "") {
                    foundryUI.StatusBar.Text = "Not enough " + lackingResource + "!";
                    return;
                }
                GameManager.Instance.Pay(foundryUI.CostTypes, foundryUI.CostAmounts);
                if(foundryUI.ShipList.GetSelectedItems().Length == 0) return;
                string selectedUnit = GameManager.GetUnitType(foundryUI.ShipList.GetItemText(foundryUI.ShipList.GetSelectedItems()[0]));
                World.Instance.CreateUnit(selectedUnit, Vendors.Instance.LastVendorPosition, 1);
                foundryUI.StatusBar.Text = "Ship Forged!";
            }
        }
    }
}