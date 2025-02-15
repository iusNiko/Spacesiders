using Godot;
using System;

public partial class Reward : Node
{
    public Encounter Encounter;
    public virtual void GrantReward() {
        GameManager.Instance.Log("Reward granted!");
    }
}