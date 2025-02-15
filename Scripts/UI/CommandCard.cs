using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CommandCard : Panel
{
    public static CommandCard Instance;
    AbilityButton[] AbilityButtons = Array.Empty<AbilityButton>();
    VBoxContainer AbilityButtonsContainer;
    public bool IsMouseOverCommandCard {
        get {
            return GetGlobalRect().HasPoint(GetGlobalMousePosition());
        }
    }
    public virtual void CreateAbilityButtons() {
        foreach(AbilityButton button in AbilityButtons) {
            button.QueueFree();
        }
        AbilityButtons = Array.Empty<AbilityButton>();
        Dictionary<string, Ability[]> AbilitiesRegister = new Dictionary<string, Ability[]>();
        foreach(Unit unit in World.Instance.SelectedUnits) {
            foreach(Ability ability in unit.Abilities) {
                if(!ability.ShowOnCommandCard) {
                    continue;
                }
                if(AbilitiesRegister.ContainsKey(ability.CommandCardName) && AbilitiesRegister[ability.CommandCardName][0].Unit != ability.Unit) {
                    AbilitiesRegister[ability.CommandCardName] = AbilitiesRegister[ability.CommandCardName].Append(ability).ToArray();
                } else {
                    AbilitiesRegister[ability.CommandCardName] = new Ability[] { ability };
                }
            }
        }
        foreach(KeyValuePair<string, Ability[]> pair in AbilitiesRegister) {
            AbilityButton button = new AbilityButton();
            AbilityButtonsContainer.AddChild(button);
            button.CustomMinimumSize = new Vector2(Instance.GetRect().Size.X - 2, 48);
            button.Text = "[" + pair.Value[0].Shortcut.ToUpper() + "] " + pair.Key + " (x" + pair.Value.Length + ")";
            button.Abilities = pair.Value;
            AbilityButtons = AbilityButtons.Append(button).ToArray();
        }
    }

    public void UnitDied(Unit unit, Unit killer) {
        CreateAbilityButtons();
    }

    public override void _Process(double delta)
    {
        if(World.Instance.SelectedUnits.Length == 0) {
            Visible = false;
            return;
        }
        Visible = true;
    }

    public override void _Ready()
    {
        Instance = this;
        AbilityButtonsContainer = GetNode<VBoxContainer>("ScrollContainer/VBoxContainer");
        World.Instance.UnitDied += UnitDied;
    }   
}
