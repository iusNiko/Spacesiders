using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class EnemiesMob : Node2D
{
    [Export] public string UnitPool;
    [Export] public int MinCount = 3;
    [Export] public int MaxCount = 5;
    [Export] public float Radius = 5f;
    [Export] public bool RandomRarity = true;
    [Export] public Array<Loot.Rarity> ExcludedRarities = new Array<Loot.Rarity>();
    [Export] Loot.Rarity Rarity = Loot.Rarity.Common;
    Unit[] Units;

    public bool IsDead() {
        foreach(Unit unit in Units) {
            if(!unit.IsDead) {
                return false;
            }
        }
        return true;
    }

    public override void _Ready()
    {
        Loot.Rarity rarity = Rarity;
        if(RandomRarity) {
            rarity = World.Instance.GetRarity(Loot.Rarity.Common, ExcludedRarities.ToArray());
        }
        Units = World.Instance.CreateUnitsGroup(GameManager.Instance.GetRandomUnitFromPool(UnitPool), World.Instance.RNG.RandiRange(MinCount, MaxCount), 2, rarity, GlobalPosition, Radius);
    }

}
