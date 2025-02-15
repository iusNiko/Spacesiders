using Godot;
using System;
using System.Linq;

public partial class UnitTooltip : Panel
{
    public Unit Unit;
    public Label UnitNameLabel;
    public Label UnitHealthLabel;
    public Label UnitDPSLabel;
    public Label UnitLevelLabel;
    public Label UnitXPToNextLevelLabel;
    public UnitTooltip(Unit unit) {
        Unit = unit;
    }
    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(200, 110);
        
        StyleBoxFlat styleBoxFlat = new StyleBoxFlat();
        switch(Unit.Rarity) {
            case Loot.Rarity.Common:
                styleBoxFlat.BgColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);
                break;
            case Loot.Rarity.Rare:
                styleBoxFlat.BgColor = new Color(0.1f, 0.1f, 0.5f, 0.4f);
                break;
            case Loot.Rarity.Epic:
                styleBoxFlat.BgColor = new Color(0.5f, 0.1f, 0.5f, 0.4f);
                break;
            case Loot.Rarity.Legendary:
                styleBoxFlat.BgColor = new Color(0.5f, 0.5f, 0.1f, 0.4f);
                break;
        }
        styleBoxFlat.BorderWidthTop = 1;
        styleBoxFlat.BorderWidthBottom = 1;
        styleBoxFlat.BorderWidthLeft = 1;
        styleBoxFlat.BorderWidthRight = 1;
        styleBoxFlat.BorderColor = new Color(styleBoxFlat.BgColor.R, styleBoxFlat.BgColor.G, styleBoxFlat.BgColor.B, 1f);
        AddThemeStyleboxOverride("panel", styleBoxFlat);
        MouseFilter = MouseFilterEnum.Ignore;

        UnitNameLabel = new Label();
        UnitNameLabel.Text = GameManager.GetUnitName(Unit.UnitType);
        AddChild(UnitNameLabel);
        UnitNameLabel.Size = new Vector2(180, 20);
        UnitNameLabel.Position = new Vector2(10, 5);
        UnitNameLabel.HorizontalAlignment = HorizontalAlignment.Left;

        UnitHealthLabel = new Label();
        UnitHealthLabel.Text = Unit.Health.ToString("0.0") + "/" + Unit.MaxHealth.ToString("0.0") + " HP";
        AddChild(UnitHealthLabel);
        UnitHealthLabel.Size = new Vector2(180, 20);
        UnitHealthLabel.Position = new Vector2(10, 25);
        UnitHealthLabel.HorizontalAlignment = HorizontalAlignment.Left;

        UnitDPSLabel = new Label();
        float DPS = 0;
        foreach(Weapon weapon in Unit.Weapons) {
            DPS += weapon.Damage * weapon.AttackSpeed;
        }
        UnitDPSLabel.Text = DPS.ToString("0.0") + " DPS";
        AddChild(UnitDPSLabel);
        UnitDPSLabel.Size = new Vector2(180, 20);
        UnitDPSLabel.Position = new Vector2(10, 65);
        UnitDPSLabel.HorizontalAlignment = HorizontalAlignment.Left;

        if(Unit.Veterancy == null) return;

        UnitLevelLabel = new Label();
        UnitLevelLabel.Text = Unit.Veterancy.GetLevelName();
        AddChild(UnitLevelLabel);
        UnitLevelLabel.Size = new Vector2(180, 20);
        UnitLevelLabel.Position = new Vector2(10, 45);
        UnitLevelLabel.HorizontalAlignment = HorizontalAlignment.Left;

        UnitXPToNextLevelLabel = new Label();
        if(Unit.Veterancy.ExperienceToNextLevel == -1) {
            UnitXPToNextLevelLabel.Text = "";
        }
        else {
            UnitXPToNextLevelLabel.Text = Unit.Veterancy.ExperienceToNextLevel.ToString("0") + " XP to level up";
        }
        AddChild(UnitXPToNextLevelLabel);
        UnitXPToNextLevelLabel.Size = new Vector2(180, 20);
        UnitXPToNextLevelLabel.Position = new Vector2(10, 85);
        UnitXPToNextLevelLabel.HorizontalAlignment = HorizontalAlignment.Left;
    }
    public override void _Process(double delta)
    {
        GlobalPosition = GetGlobalMousePosition();
        Scale = new Vector2(1/World.Instance.Camera.Zoom.X, 1/World.Instance.Camera.Zoom.Y);
        UnitHealthLabel.Text = Unit.Health.ToString("0.0") + "/" + Unit.MaxHealth.ToString("0.0") + " HP";
        float DPS = 0;
        foreach(Weapon weapon in Unit.Weapons) {
            DPS += weapon.Damage * weapon.AttackSpeed;
        }
        UnitDPSLabel.Text = DPS.ToString("0.0") + " DPS";

        if(Unit.Veterancy == null) return;
        UnitLevelLabel.Text = Unit.Veterancy.GetLevelName();
        if(Unit.Veterancy.ExperienceToNextLevel == -1) {
            UnitXPToNextLevelLabel.Text = "";
        }
        else {
            UnitXPToNextLevelLabel.Text = Unit.Veterancy.ExperienceToNextLevel.ToString("0") + " XP to level up";
        }
    }
}
