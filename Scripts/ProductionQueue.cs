using Godot;
using System;
using System.Linq;

public partial class ProductionQueue : Node {
    public struct QueueItem {
        public string ItemName;
        public float Progress;
        public float MaxProgress;
    }
    public Unit Unit {
        get { 
            return GetParent<Unit>();
        }
    }
    [Export] public int MaxSlots = 5;
    [Export] public int ProductionSlots = 1;
    public QueueItem[] Queue = Array.Empty<QueueItem>();

    public bool AddItem(string itemName, float maxProgress) {
        if(Queue.Length < MaxSlots) {
            QueueItem item = new QueueItem();
            item.ItemName = itemName;
            item.MaxProgress = maxProgress;
            Queue = Queue.Append(item).ToArray();
            return true;
        }
        return false;
    }

    public override void _Process(double delta)
    {
        for(int i = 0; i < ProductionSlots && i < Queue.Length; i++) {
            Queue[i].Progress += (float)delta;
            if(Queue[i].Progress >= Queue[i].MaxProgress) {
                World.Instance.CreateUnit(Queue[i].ItemName, Unit.GlobalPosition, Unit.Team);
                Queue = Queue.Where((item, index) => index != i).ToArray();
            }
        }
    }
}