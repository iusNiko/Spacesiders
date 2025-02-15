using Godot;
using System;
using System.Collections.Generic;

public partial class FoundryUI : Panel
{
	public ItemList ShipList;
	public TextureRect ShipImage;
	public Label ShipName;
	public Label StatusBar;
	public FoundryForgeButton ForgeButton;
	public CloseVendorButton CloseButton;
	public string[] CostTypes;
	public float[] CostAmounts;

    public override void _Ready()
    {
		ShipList = new();
		ShipImage = new();
		ShipName = new();
		StatusBar = new();
		ForgeButton = new();
		CloseButton = new();
		AddChild(ShipList);
		AddChild(ShipImage);
		AddChild(ShipName);
		AddChild(StatusBar);
		AddChild(ForgeButton);
		AddChild(CloseButton);

		ShipList.Position = new Vector2(10, 10);
		ShipList.CustomMinimumSize = new Vector2(400, 1000);
		foreach(string unit in GameManager.Instance.UNLOCKED_UNITS) {
			ShipList.AddItem(GameManager.GetUnitName(unit));
		}

		ShipName.Position = new Vector2(420, 10);
		ShipImage.Position = new Vector2(420, 40);
		StatusBar.Position = new Vector2(420, 800);
		StatusBar.CustomMinimumSize = new Vector2(600, 80);
		ForgeButton.Position = new Vector2(620, 900);
		CloseButton.Position = new Vector2(920, 900);
		ForgeButton.CustomMinimumSize = new Vector2(200, 120);
		CloseButton.CustomMinimumSize = new Vector2(200, 120);
		CloseButton.Texture = (Texture2D)ResourceLoader.Load("res://Art/UI/Game/CloseButton.png");
		ForgeButton.Texture = (Texture2D)ResourceLoader.Load("res://Art/UI/Game/ForgeButton.png");

		ShipList.ItemSelected += OnItemSelected;
    }

	public void OnItemSelected(long index) {
		string selectedUnit = GameManager.GetUnitType(ShipList.GetItemText((int)index));
		ShipName.Text = GameManager.GetUnitName(selectedUnit);
		StatusBar.Text = "";
		selectedUnit = GameManager.GetUnitName(selectedUnit).Replace(" ", "_");
		ShipImage.Texture = (Texture2D)ResourceLoader.Load("res://Art/Units/" + selectedUnit + ".png");
		foreach(Label resourceLabel in GetTree().GetNodesInGroup("CostLabels")) {
			resourceLabel.QueueFree();
		}
		CostTypes = (string[])GameManager.GetPropertyFromPackedScene(GameManager.Instance.UNITS_REGISTER[selectedUnit.ToLower()], "CostTypes");
		CostAmounts = (float[])GameManager.GetPropertyFromPackedScene(GameManager.Instance.UNITS_REGISTER[selectedUnit.ToLower()], "CostAmounts");
		GD.Print(CostTypes.Length);
		for(int i = 0; i < CostTypes.Length; i++) {
			Label resourceLabel = new();
			resourceLabel.Text = CostTypes[i] + ": " + CostAmounts[i];
			AddChild(resourceLabel);
			resourceLabel.Position = new Vector2(420, 250 + i * 20);
			resourceLabel.AddToGroup("CostLabels");
		}
		ShipImage.CustomMinimumSize = ShipImage.Texture.GetSize() * 4;
	}
}
