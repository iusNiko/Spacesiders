using Godot;
using System;
public partial class Encounter : Node
{
    public EncounterNode EncounterNode;
    public Reward[] Rewards = Array.Empty<Reward>();
    public override void _Ready() {

    }
    public override void _Process(double delta) {
        if(IsOver()) {
            End();
        }
    }
    public virtual void End() {
        GameManager.Instance.Log("Encounter ended!");
        foreach(Reward reward in Rewards) {
            reward.GrantReward();
        }
        QueueFree();
    }
    public virtual bool IsOver() {
        return true;
    }
}