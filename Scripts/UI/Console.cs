using Godot;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;

public partial class Console : Control
{
    public static Console Instance;
    public TextEdit ConsoleText;
    public LineEdit ConsoleInput;
    public virtual void CommandProcessor(string input) {
        string[] command = input.Split(" ");
        Type type = this.GetType();
        MethodInfo method = type.GetMethod(command[0]);
        if(method != null) {
            method.Invoke(this, new object[] { command });
        }
        else {
            ConsoleText.Text += "\n" + $"Command \"" + command[0] + "\" not found";
        }
    }
    public override void _Ready()
    {
        Instance = this;
        ConsoleText = GetNode<TextEdit>("TextEdit");
        ConsoleInput = GetNode<LineEdit>("LineEdit");
    }

    public override void _Process(double delta)
    {
        if(Input.IsActionJustPressed("open_console"))
        {
            Visible = !Visible;
            if(Visible) {
                ConsoleInput.GrabFocus();
                ConsoleInput.Text = ConsoleInput.Text.Replace("`", "");
            }
            else {
                ConsoleInput.ReleaseFocus();
            }
        }
        if (Input.IsActionJustPressed("submit") && Visible)
        {
            string input = ConsoleInput.Text.ToLower();
            ConsoleInput.Text = "";
            ConsoleText.Text += "\n> " + input;
            CommandProcessor(input);
            ConsoleText.ScrollVertical = ConsoleText.GetVScrollBar().MaxValue;
        }
    }

    //Commands
    public void help(string[] args)
    {
        ConsoleText.Text += "\n" + "Available commands:";
        Type type = this.GetType();
        ConsoleText.Text += "\n";
        foreach(MethodInfo method in type.GetMethods()) {
            MethodInfo mi = type.GetMethod(method.Name.ToLower());
            if(mi != null) {
                ConsoleText.Text += method.Name.ToLower() + "; ";
            }
        }
    }
    public void clear(string[] args)
    {
        ConsoleText.Text = "";
    }
    public void crash(string[] args)
    {
        GameManager.Instance.Crash();
    }
    public void spawn(string[] args)
    {
        if(args.Length < 2) {
            ConsoleText.Text += "\n" + "Usage: spawn <unit_name> <unit_count=1> <team=1> <rarity=0>";
            return;
        }
        string unitName = args[1].ToLower();
        if(!GameManager.Instance.UNITS_REGISTER.ContainsKey(unitName)) {
            ConsoleText.Text += "\n" + "Unit " + unitName + " not found";
            return;
        }
        int unitCount = 1;
        if(args.Length > 2) {
            if(!int.TryParse(args[2], out unitCount)) {
                ConsoleText.Text += "\n" + "Unit count must be an integer";
                return;
            }
        }
        int team = 1;
        if(args.Length > 3) {
            if(!int.TryParse(args[3], out team)) {
                ConsoleText.Text += "\n" + "Team must be an integer";
                return;
            }
        }
        int rarity = 0;
        if(args.Length > 4) {
            if(!int.TryParse(args[4], out rarity)) {
                ConsoleText.Text += "\n" + "Rarity must be an integer";
                return;
            }
        }
        for(int i = 0; i < unitCount; i++) {
            Vector2 spawnPos = World.Instance.GetNode<Camera2D>("../Camera2D").GlobalPosition;
            World.Instance.CreateUnit(unitName, spawnPos, team, (Loot.Rarity)rarity);
        }
        ConsoleText.Text += "\n" + "Spawned " + unitCount + " " + unitName;
    }
    public void list_units(string[] args)
    {
        ConsoleText.Text += "\n";
        foreach(string unitName in GameManager.Instance.UNITS_REGISTER.Keys) {
            ConsoleText.Text += unitName + "; ";
        }
    }
    public void list_encounters(string[] args)
    {
        ConsoleText.Text += "\n";
        foreach(string encounterName in GameManager.Instance.ENCOUNTERS_REGISTER.Keys) {
            ConsoleText.Text += encounterName + "; ";
        }
    }
    public void list_rewards(string[] args)
    {
        ConsoleText.Text += "\n";
        foreach(string rewardName in GameManager.Instance.REWARDS_REGISTER.Keys) {
            ConsoleText.Text += rewardName + "; ";
        }
    }
    public void list_weapons(string[] args)
    {
        ConsoleText.Text += "\n";
        foreach(string weaponName in GameManager.Instance.WEAPONS_REGISTER.Keys) {
            ConsoleText.Text += weaponName + "; ";
        }
    }
    public void list_abilities(string[] args)
    {
        ConsoleText.Text += "\n";
        foreach(string abilityName in GameManager.Instance.ABILITIES_REGISTER.Keys) {
            ConsoleText.Text += abilityName + "; ";
        }
    }
    public void list_augments(string[] args)
    {
        ConsoleText.Text += "\n";
        foreach(string augmentName in GameManager.Instance.AUGMENTS_REGISTER.Keys) {
            ConsoleText.Text += augmentName + "; ";
        }
    }
    public void kill(string[] args)
    {
        foreach(Unit unit in World.Instance.SelectedUnits) {
            unit.OnDeath();
        }
    }

    public void add_weapon(string[] args)
    {
        if(args.Length < 2) {
            ConsoleText.Text += "\n" + "Usage: add_weapon <weapon_name>";
            return;
        }
        string weaponName = args[1].ToLower();
        if(!GameManager.Instance.WEAPONS_REGISTER.ContainsKey(weaponName)) {
            ConsoleText.Text += "\n" + "Weapon " + weaponName + " not found";
            return;
        }
        foreach(Unit unit in World.Instance.SelectedUnits) {
            Weapon weapon = GameManager.Instance.WEAPONS_REGISTER[weaponName].Instantiate<Weapon>();
            unit.AddWeapon(weapon);
        }
    }

    public void echo(string[] args) {
        if(args.Length < 2) {
            ConsoleText.Text += "\n" + "Usage: echo <text>";
            return;
        }
        string text = args[1];
        ConsoleText.Text += "\n" + text;
    }

    public void movecam(string[] args) {
        if(args.Length < 3) {
            ConsoleText.Text += "\n" + "Usage: movecam <x> <y>";
            return;
        }
        int x = 0;
        int y = 0;
        if(!int.TryParse(args[1], out x)) {
            ConsoleText.Text += "\n" + "X must be an integer";
            return;
        }
        if(!int.TryParse(args[2], out y)) {
            ConsoleText.Text += "\n" + "Y must be an integer";
            return;
        }
        World.Instance.GetNode<Camera2D>("../Camera2D").GlobalPosition = new Vector2(x, y);
    }

    public void stats(string[] args) {
        if(World.Instance.SelectedUnits.Length == 0) {
            ConsoleText.Text += "\n" + "No units selected";
            return;
        }
        foreach(Unit unit in World.Instance.SelectedUnits) {
            ConsoleText.Text += "\n" + unit.Name + " stats:";
            ConsoleText.Text += "\n" + "Health: " + unit.Health + "/" + unit.MaxHealth;
            ConsoleText.Text += "\n" + "Speed: " + unit.MoveSpeed;
            ConsoleText.Text += "\n" + "Value: " + unit.GetValue();
        }
    }

    public void set_glow_strength(string[] args) {
        if(args.Length < 2) {
            ConsoleText.Text += "\n" + "Usage: set_glow_strength <strength>";
            return;
        }
        float strength = 0;
        if(!float.TryParse(args[1], out strength)) {
            ConsoleText.Text += "\n" + "Strength must be a float";
            return;
        }
        World.Instance.Environment.Environment.GlowMix = strength;
    }

    public void change_map(string[] args) {
        if(args.Length < 2) {
            ConsoleText.Text += "\n" + "Usage: change_map <map_name>";
            return;
        }
        World.Instance.ChangeMap(args[1]);
    }
}
