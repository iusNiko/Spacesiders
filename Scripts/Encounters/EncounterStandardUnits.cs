using Godot;
using System;
using System.Linq;

public partial class EncounterStandardUnits : Encounter
{
    [Export] public string[] UnitTypes = Array.Empty<string>();
    [Export] public int[] UnitCounts = Array.Empty<int>();
    [Export] public Vector2[] UnitPositions = Array.Empty<Vector2>();
    [Export] public int[] UnitTeams = Array.Empty<int>();

    // All Above Arrays have to be the same length

    Unit[] SpawnedEnemies = Array.Empty<Unit>();

    public override void _Ready() {
        base._Ready();
        for(int i = 0; i < UnitTypes.Length; i++) {
            for(int j = 0; j < UnitCounts[i]; j++) {
                Unit unit = World.Instance.CreateUnit(UnitTypes[i].ToLower(), EncounterNode.GlobalPosition + UnitPositions[i], UnitTeams[i]);
                if(unit.Team > 1) {
                    SpawnedEnemies = SpawnedEnemies.Append(unit).ToArray();
                }
            }
        }
    }

    public override bool IsOver() {
        for(int i = 0; i < SpawnedEnemies.Length; i++) {
            if(SpawnedEnemies[i] != null && IsInstanceValid(SpawnedEnemies[i]) && SpawnedEnemies[i].IsDead == false) {
                return false;
            }
        }
        return true;
    }
}