using Godot;
using System;
using System.Linq;

public partial class Ability : Node {
    [Export] public string Shortcut = "";
    [Export] public bool ShowOnCommandCard = false;
    [Export] public string CommandCardName = "";
    [Export] public string[] CostTypes = Array.Empty<string>();
    [Export] public float[] CostAmounts = Array.Empty<float>();
    public bool CostPayed = false;
    public bool CanPay {
        get {
            foreach(string costType in CostTypes) {
                if(GameManager.Instance.Resources.ContainsKey(costType) && GameManager.Instance.Resources[costType] < CostAmounts[Array.IndexOf(CostTypes, costType)]) {
                    return false;
                }
            }
            return true;
        }
    }
    public Unit Unit {
        get {
            return GetParent<Node>().GetParent<Unit>();
        }
    }
    public Order Order;
    public float TimeSinceLastUse = 0;
    public float Cooldown = 0;
    
    public virtual void Execute() {
        
    }
    public virtual bool IsFinished() {
        return true;
    }

    public virtual void Use() {
        CostPayed = false;
        if(!CanPay) {
            return;
        }
        foreach(string costType in CostTypes) {
            if(GameManager.Instance.Resources.ContainsKey(costType)) {
                GameManager.Instance.Resources[costType] -= CostAmounts[Array.IndexOf(CostTypes, costType)];
            }
        }
        CostPayed = true;
    }

    public virtual void Update(float delta)
    {
        
    }
}