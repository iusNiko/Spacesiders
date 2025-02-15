using Godot;
using System;

public partial class RewardUnits : Reward
{
    [Export] public string[] UnitTypes = Array.Empty<string>();
    [Export] public int[] UnitCounts = Array.Empty<int>();
    [Export] public Vector2[] UnitPositions = Array.Empty<Vector2>();
    [Export] public int[] UnitTeams = Array.Empty<int>();
    public override void GrantReward() {
        base.GrantReward();
        for(int i = 0; i < UnitTypes.Length; i++) {
            for(int j = 0; j < UnitCounts[i]; j++) {
                World.Instance.CreateUnit(UnitTypes[i].ToLower(), Encounter.EncounterNode.GlobalPosition + UnitPositions[i], UnitTeams[i]);
            }
        }
    }
}
