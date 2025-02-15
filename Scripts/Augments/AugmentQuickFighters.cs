using System;
using Godot;

public partial class AugmentQuickFighters : Augment {
    public AugmentQuickFighters(int team = 1) : base(team) {
    }
    public override void _Ready()
    {
        PropertyModifiers = new PropertyModifier[] {
            new PropertyModifier("MoveSpeed", 1.2f),
            new PropertyModifier("AttackSpeed", 1.2f),
        };
        RequiredFlags = new string[] {
            "fighter"
        };
        Description = "Increases the movement and attack speed of all fighter class units.";
    }
}