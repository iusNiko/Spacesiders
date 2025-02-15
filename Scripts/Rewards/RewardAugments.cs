using System;
using Godot;

public partial class RewardAugments : Reward {
    [Export] public string[] AugmentNames;
    [Export] public int[] AugmentTeams;
    public override void GrantReward() {
        base.GrantReward();
        for(int i = 0; i < AugmentNames.Length; i++) {
            World.Instance.AddAugment(AugmentNames[i], AugmentTeams[i]);
        }
    }
}