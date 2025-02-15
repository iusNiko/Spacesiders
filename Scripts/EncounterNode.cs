using Godot;
using System;
using System.Linq;

public partial class EncounterNode : Node2D
{
    public Encounter[] Encounters = Array.Empty<Encounter>();

    public override void _Process(double delta)
    {
        if(Encounters.Length == 0) {
            return;
        }
        int endedEncounters = 0;
        foreach(Encounter encounter in Encounters) {
            if(encounter.IsOver()) {
                endedEncounters++;
            }
        }
    }
}
