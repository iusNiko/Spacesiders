using System;
using Godot;

public partial class Augment {
    public PropertyModifier[] PropertyModifiers = Array.Empty<PropertyModifier>();
    public string[] RequiredFlags = Array.Empty<string>();
    public string Description = "";
    public int Team = 1;

    public Augment(int team = 1) {
        Team = team;
        _Ready();
    }

    public virtual void _Ready() {
        //Compose augment here
    }
}