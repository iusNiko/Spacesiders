using Godot;
using Godot.Collections;
using System;   
using System.Linq;

public partial class RandomEnemies : Node2D {
    [Export] public int PotentialPointsCount = 10000;
    [Export] public float RandomEnemiesRadius = 1000f;
    [Export] public float RandomEnemiesMinDistance = 100f;
    [Export] public int RandomEnemiesMaxGroups = 10;
    [Export] public string UnitsPool = "";
    [Export] public int EnemiesPerGroup = 5;
    [Export] public Array<Loot.Rarity> ExcludedRarities = new Array<Loot.Rarity>();
    [Export] public bool IncludePlayerSpawnPoint = false;
    [Export] public bool IncludeExit = false;

    public override void _Ready()
    {
        Vector2[] potentialPoints = new Vector2[PotentialPointsCount];
        Vector2[] confirmedPoints = System.Array.Empty<Vector2>();

        for(int i = 0; i < PotentialPointsCount; i++) {
            potentialPoints[i].X = World.Instance.RNG.RandfRange(-RandomEnemiesRadius, RandomEnemiesRadius);
            potentialPoints[i].Y = World.Instance.RNG.RandfRange(-RandomEnemiesRadius, RandomEnemiesRadius);
        }

        if(IncludePlayerSpawnPoint) RandomEnemiesMaxGroups++;
        
        
        while(confirmedPoints.Length < RandomEnemiesMaxGroups && potentialPoints.Length > 0) {
            Vector2 point = potentialPoints[World.Instance.RNG.RandiRange(0, potentialPoints.Length - 1)];
            potentialPoints = potentialPoints.Where(p => p != point).ToArray();
            Vector2[] toRemove = System.Array.Empty<Vector2>();
            foreach(Vector2 pp in potentialPoints) {
                if(point.DistanceTo(pp) < RandomEnemiesMinDistance) {
                    toRemove = toRemove.Append(pp).ToArray();
                }
            }
            potentialPoints = potentialPoints.Where(p => !toRemove.Contains(p)).ToArray();
            confirmedPoints = confirmedPoints.Append(point).ToArray();
        }
        

        Vector2 FarthestPoint = confirmedPoints[0];
        foreach(Vector2 point in confirmedPoints) {
            if(point.Length() > FarthestPoint.Length()) {
                FarthestPoint = point;
            }
        }

        if(IncludePlayerSpawnPoint) {
            confirmedPoints = confirmedPoints.Where(p => p != FarthestPoint).ToArray();

            Unit[] playerUnits = World.Instance.Units.Where(u => u.Team == 1).ToArray();

            FarthestPoint = ToGlobal(FarthestPoint + (new Vector2(0, 0).DirectionTo(FarthestPoint) * 100));
            foreach(Unit u in playerUnits) {
                u.GlobalPosition = FarthestPoint;
            }

            World.Instance.Camera.GlobalPosition = FarthestPoint;
            World.Instance.Camera.Zoom = new Vector2(2, 2);
        }

        int exitIndex = World.Instance.RNG.RandiRange(0, confirmedPoints.Length - 1);
            
        int j = 0;
        foreach(Vector2 point in confirmedPoints) {
            Unit[] units = World.Instance.CreateUnitsGroup(GameManager.Instance.GetRandomUnitFromPool(UnitsPool), EnemiesPerGroup, 2, World.Instance.GetRarity(Loot.Rarity.Common, ExcludedRarities.ToArray()), ToGlobal(point), 5f);
            if(IncludeExit && j == exitIndex) {
                HyperLink exit = new HyperLink();
                exit.GlobalPosition = point;
                exit.GuardingUnits = units;
                World.Instance.AddChild(exit);
            }
            j++;
        }
    }
}