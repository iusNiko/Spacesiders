using Godot;
using System;

public partial class AbilityTrain : Ability {
    [Export] public string UnitToTrain = "";
    [Export] public float TrainTime = 5f;
    public new bool CanPay {
        get {
            foreach(string costType in CostTypes) {
                if(GameManager.Instance.Resources.ContainsKey(costType) && GameManager.Instance.Resources[costType] < CostAmounts[Array.IndexOf(CostTypes, costType)]) {
                    return false;
                }
                
            }
            foreach(string resourceType in GameManager.Instance.UNITS_COSTS_REGISTER[UnitToTrain].Keys) {
                if(GameManager.Instance.Resources[resourceType] < GameManager.Instance.UNITS_COSTS_REGISTER[UnitToTrain][resourceType]) {
                    return false;
                }
            }
            return true;
        }
    }
    public override void Execute()
    {
        Unit.ProductionQueue.AddItem(UnitToTrain, TrainTime);
    }
    public override void Use()
    {
        CostPayed = false;
        if(!CanPay) {
            return;
        }
        foreach(string costType in CostTypes) {
            if(GameManager.Instance.Resources.ContainsKey(costType)) {
                GameManager.Instance.Resources[costType] -= CostAmounts[Array.IndexOf(CostTypes, costType)];
            }
        }
        foreach(string resourceType in GameManager.Instance.UNITS_COSTS_REGISTER[UnitToTrain].Keys) {
            GameManager.Instance.Resources[resourceType] -= GameManager.Instance.UNITS_COSTS_REGISTER[UnitToTrain][resourceType];
        }
        CostPayed = true;
        if(!Input.IsActionPressed("shift")) {
            Unit.ClearOrders();
        }
        Unit.OrderQueue.Enqueue(new OrderAbility(Unit, this, Unit.GlobalPosition, null));
    }
}