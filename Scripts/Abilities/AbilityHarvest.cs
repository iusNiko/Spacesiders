using Godot;
using System;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.AccessControl;

public partial class AbilityHarvest : Ability
{
    [Export] public string[] ResourceTypes = Array.Empty<string>();
    [Export] public float[] HarvestRates = Array.Empty<float>();
    public float[] ResourceAmounts = Array.Empty<float>();
    [Export] public float[] ResourceMaxAmounts = Array.Empty<float>();
    [Export] public float[] HarvestSpeeds = Array.Empty<float>();
    [Export] public float HarvestRange = 4f;
    [Export] public float DropoffRange = 4f;
    public float TimeSinceLastHarvest = 0f;
    public Unit Target = null;

    public bool IsFull() {
        for(int i = 0; i < ResourceTypes.Length; i++) {
            if(ResourceAmounts[i] >= ResourceMaxAmounts[i]) {
                return true;
            }
        }
        return false;
    }
    public override void Execute()
    {
        
    }
    public override void Use()
    {
        Unit[] potentialTargets = World.Instance.HoveredUnits.Where(unit => unit.GetResourceDeposits().Where(rd => ResourceTypes.Contains(rd.ResourceType)).Any()).ToArray();
        if(potentialTargets.Length == 0) {
            return;
        }
        Target = potentialTargets[0];
        
    }
    public override void _Ready()
    {
        ResourceAmounts = new float[ResourceTypes.Length];
    }
    public override void _Process(double delta)
    {
        TimeSinceLastHarvest += (float)delta;
        if(Target == null) {
            return;
        }
        if(Target.GlobalPosition.DistanceTo(Unit.GlobalPosition) > HarvestRange && !IsFull()) {
            Unit.MovementTarget = Target.GlobalPosition;
            if(Unit.StateManager.CurrentState is not StateWalking) {
                Unit.StateManager.ChangeState(new StateWalking());
            }
        }
        else if(!IsFull()) {
            float combinedHarvestSpeed = 0f;
            for(int i = 0; i < ResourceTypes.Length; i++) {
                combinedHarvestSpeed += HarvestSpeeds[i];
            }
            if(TimeSinceLastHarvest > 1f / combinedHarvestSpeed) {
                TimeSinceLastHarvest = 0f;
                for(int i = 0; i < ResourceTypes.Length; i++) {
                    float amount = Math.Min(HarvestRates[i], Math.Min(ResourceMaxAmounts[i] - ResourceAmounts[i], Target.GetResourceDeposits().Where(rd => rd.ResourceType == ResourceTypes[i]).First().ResourceAmount));
                    ResourceAmounts[i] += amount;
                    Target.GetResourceDeposits().Where(rd => rd.ResourceType == ResourceTypes[i]).First().ResourceAmount -= amount;
                }
                int emptyResourceDeposits = 0;
                foreach(ResourceDeposit resourceDeposit in Target.GetResourceDeposits()) {
                    if(resourceDeposit.ResourceAmount <= 0) {
                        emptyResourceDeposits++;
                    }
                }
                if(emptyResourceDeposits == Target.GetResourceDeposits().Length) {
                    World.Instance.ResourceDeposits = World.Instance.ResourceDeposits.Where(rd => rd != Target).ToArray();
                    Target.OnDeath();
                    Target = World.Instance.GetNearestResourceDeposit(Unit.GlobalPosition);
                }
            }
        }
        else {
            Unit dropoff = World.Instance.GetNearestResourceDropoff(Unit.GlobalPosition, Unit.Team);
            if(dropoff.GlobalPosition.DistanceTo(Unit.GlobalPosition) > DropoffRange) {
                Unit.MovementTarget = dropoff.GlobalPosition;
                if(Unit.StateManager.CurrentState is not StateWalking) {
                    Unit.StateManager.ChangeState(new StateWalking());
                }
            }
            else {
                for(int i = 0; i < ResourceTypes.Length; i++) {
                    GameManager.Instance.Resources[ResourceTypes[i]] += ResourceAmounts[i];
                    ResourceAmounts[i] = 0;
                    GD.Print(GameManager.Instance.Resources[ResourceTypes[i]]);
                }
            }
        }
    }
}