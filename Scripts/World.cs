using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Collections.Concurrent;
using System.Linq;

public partial class World : Node2D
{
    public static World Instance;
    public RandomNumberGenerator RNG = new RandomNumberGenerator();
    public Camera Camera;
    public Unit[] Units = Array.Empty<Unit>();
    public Unit[] SelectedUnits = Array.Empty<Unit>();
    public Unit[] HoveredUnits = Array.Empty<Unit>();
    public Unit[] ResourceDeposits = Array.Empty<Unit>();
    public Unit[] ResourceDropoffs = Array.Empty<Unit>();
    public Loot[] Loots = Array.Empty<Loot>();
    public Loot[] HoveredLoot = Array.Empty<Loot>();
    public InteractableObject[] InteractableObjects = Array.Empty<InteractableObject>();
    public InteractableObject[] HoveredInteractableObjects = Array.Empty<InteractableObject>();
    public float LootCollectionRange = 50f;
    public EncounterNode[] EncounterNodes = Array.Empty<EncounterNode>();
    [Export] public int EncounterDifficulty = 1;
    [Export] public int RewardRichness = 1;
    [Export] public int ResourceNodes = 4;
    public WorldEnvironment Environment;
    [Export] public float UnitClusteringThreshold = 40f;
    Augment[] Augments = Array.Empty<Augment>();
    [Signal] public delegate void UnitDiedEventHandler(Unit deadUnit, Unit killer);
    public UnitTooltip UnitTooltip = null;
    Unit LastHoveredUnit = null;
    float TimeSinceHoverSelection = 0f;
    public Map Map = null;

    public override void _Ready()
    {
        Instance = this;
        RNG.Randomize();
        Camera = GetNode<Camera>("../Camera2D");
        Environment = GetNode<WorldEnvironment>("../Environment");

        while(Units.Where(u => u.Team == 1).ToArray().Length < 10) {
            CreateUnit("pirate_corvette", new Vector2(0, 0), 1, Loot.Rarity.Common);
        }

        ChangeMap("TehhanasRefuge");
        UnitDied += SpawnLootOnUnitDeath;
    }

    public Loot.Rarity GetRarity(Loot.Rarity Rarity = Loot.Rarity.Common, Loot.Rarity[] ExcludedRarities = null) {
        Loot.Rarity selectedRarity = Loot.Rarity.Common;
        Dictionary<Loot.Rarity, int> weights = new Dictionary<Loot.Rarity, int>  {
            {Loot.Rarity.Common, 850},
            {Loot.Rarity.Rare, 100},
            {Loot.Rarity.Epic, 50},
            {Loot.Rarity.Legendary, 10}
        };

        if((int)Rarity >= (int)Loot.Rarity.Rare) {
            weights[Loot.Rarity.Common] -= 100;
            weights[Loot.Rarity.Rare] += 100;
            weights[Loot.Rarity.Epic] += 50;
            weights[Loot.Rarity.Legendary] += 10;
        }
        if((int)Rarity >= (int)Loot.Rarity.Epic) {
            weights[Loot.Rarity.Common] -= 100;
            weights[Loot.Rarity.Rare] += 100;
            weights[Loot.Rarity.Epic] += 90;
            weights[Loot.Rarity.Legendary] += 10;
        }
        if((int)Rarity >= (int)Loot.Rarity.Legendary) {
            weights[Loot.Rarity.Common] -= 200;
            weights[Loot.Rarity.Epic] += 90;
            weights[Loot.Rarity.Legendary] += 100;
        }

        foreach(Loot.Rarity key in weights.Keys) {
            if(weights[key] < 0) {
                weights[key] = 0;
            }
        }

        if(ExcludedRarities != null) {
            foreach(Loot.Rarity r in ExcludedRarities) {
                weights[r] = 0;
            }
        }

        int totalWeight = 0;
        foreach(int weight in weights.Values) {
            totalWeight += weight;
        }
        int randomValue = RNG.RandiRange(0, totalWeight);
        int currentWeight = 0;
        foreach(Loot.Rarity key in weights.Keys) {
            currentWeight += weights[key];
            if(randomValue <= currentWeight) {
                selectedRarity = key;
                break;
            }
        }

        return selectedRarity;
    }

    public float GetDropFactor(Loot.Rarity rarity) {
        switch(rarity) {
            case Loot.Rarity.Common:
                return 0.3f;
            case Loot.Rarity.Rare:
                return 1f;
            case Loot.Rarity.Epic:
                return 2f;
            case Loot.Rarity.Legendary:
                return 5f;
            default:
                return 0.3f;
        }
    }

    public float GetRarityValue(Loot.Rarity rarity) {
        switch(rarity) {
            case Loot.Rarity.Common:
                return 1f;
            case Loot.Rarity.Rare:
                return 2f;
            case Loot.Rarity.Epic:
                return 5f;
            case Loot.Rarity.Legendary:
                return 10f;
            default:
                return 1f;
        }
    }

    public void ChangeMap(string mapName)
    {
        Unit[] unitsToDelete = Units.Where(unit => unit.Team != 1).ToArray();
        Units = Units.Where(unit => unit.Team == 1).ToArray();
        unitsToDelete.ToList().ForEach(unit => unit.QueueFree());
        Map?.QueueFree();
        Loots.ToList().ForEach(loot => loot.QueueFree());
        Loots = Array.Empty<Loot>();
        InteractableObjects.ToList().ForEach(interactableObject => interactableObject.QueueFree());
        InteractableObjects = Array.Empty<InteractableObject>();
        HoveredInteractableObjects = Array.Empty<InteractableObject>();
        HoveredLoot = Array.Empty<Loot>();
        Map map = ((PackedScene)GD.Load("res://Scenes/Maps/" + mapName + ".tscn")).Instantiate<Map>();
        AddChild(map);
        Map = map;
        MapLabel ml = new MapLabel(Map.MapName, Map.Region);
        AddChild(ml);
        Unit[] playerUnits = Units.Where(u => u.Team == 1).ToArray();
        Vector2 playerUnitsNewPosition = new Vector2(0, 0);
        if(Map.SpawnPoint != null) {
            playerUnitsNewPosition = Map.SpawnPoint.GlobalPosition;
        }
        Camera.GlobalPosition = playerUnitsNewPosition;
        foreach(Unit unit in playerUnits) {
            unit.Modulate = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            unit.GlobalPosition = playerUnitsNewPosition;
            unit.MovementTarget = playerUnitsNewPosition;
        }
    }

    public Unit[] CreateUnitsGroup(string unitName, int count, int team, Loot.Rarity rarity, Vector2 position, float radius)
    {
        Unit[] units = Array.Empty<Unit>();
        for(int i = 0; i < count; i++) {
            Vector2 spawnPos = position + new Vector2(RNG.RandfRange(-radius, radius), RNG.RandfRange(-radius, radius));
            units = units.Append(CreateUnit(unitName, spawnPos, team, rarity)).ToArray();
        }
        return units;
    }

    public void CreateFeatures() {
        EncounterNode[] resourceNodes = Array.Empty<EncounterNode>();
        for(int i = 0; i < ResourceNodes; i++) {
            EncounterNode[] potentialResourceNodes = EncounterNodes.Where(en => !resourceNodes.Contains(en)).ToArray();
            resourceNodes = resourceNodes.Append(potentialResourceNodes[RNG.RandiRange(0, potentialResourceNodes.Length - 1)]).ToArray();
        }
        foreach(EncounterNode resourceNode in resourceNodes) {
            Unit resourceDeposit = CreateUnit("scrap_asteroid", resourceNode.GlobalPosition + new Vector2(RNG.RandfRange(-150, 150), RNG.RandfRange(-150, 150)), 0);
        }
    }

    public void CreateEncounterNodes(bool StrictDifficulty = false, bool StrictRichness = false){
        Vector2[] pattern = GameManager.Instance.ENCOUNTER_PATTERNS[RNG.RandiRange(0, GameManager.Instance.ENCOUNTER_PATTERNS.Length - 1)];
        Unit[] playerUnits = Units.Where(u => u.Team == 1).ToArray();
        int playerSpot = RNG.RandiRange(0, pattern.Length - 1);
        foreach(Unit unit in playerUnits) {
            unit.GlobalPosition = pattern[playerSpot];
        }
        Camera.GlobalPosition = pattern[playerSpot];
        pattern = pattern.Where((pos, index) => index != playerSpot).ToArray();
        while(pattern.Length > 0) {
            EncounterNode encounterNode = new EncounterNode();
            encounterNode.GlobalPosition = pattern[0];
            pattern = pattern.Where((pos, index) => index != 0).ToArray();
            AddChild(encounterNode);
            EncounterNodes = EncounterNodes.Append(encounterNode).ToArray();
        }
        CreateFeatures();
        foreach(EncounterNode encounterNode in EncounterNodes) {
            int remainingDifficulty = EncounterDifficulty;
            int remainingRichness = RewardRichness;
            while(remainingDifficulty > 0) {
                string[] encounters = Array.Empty<string>();
                foreach(string encounterName in GameManager.Instance.ENCOUNTERS_REGISTER.Keys.ToArray()) {
                    int difficulty = encounterName.Split('_')[0].ToInt();
                    if(StrictDifficulty) {
                        if(difficulty == remainingDifficulty) {
                            encounters = encounters.Append(encounterName).ToArray();
                        }
                    }
                    if(!StrictDifficulty || encounters.Length == 0) {
                        if(difficulty <= remainingDifficulty) {
                            encounters = encounters.Append(encounterName).ToArray();
                        }
                    }
                }
                string encounter = encounters[RNG.RandiRange(0, encounters.Length - 1)];
                remainingDifficulty -= encounter.Split('_')[0].ToInt();

                Encounter enc = GameManager.Instance.ENCOUNTERS_REGISTER[encounter].Instantiate<Encounter>();
                while(remainingRichness > 0) {
                    string[] rewards = Array.Empty<string>();
                    foreach(string rewardName in GameManager.Instance.REWARDS_REGISTER.Keys.ToArray()) {
                        int richness = rewardName.Split('_')[0].ToInt();
                        if(StrictRichness) {
                            if(richness == remainingRichness) {
                                rewards = rewards.Append(rewardName).ToArray();
                            }
                        }
                        if(!StrictRichness || rewards.Length == 0) {
                            if(richness <= remainingRichness) {
                                rewards = rewards.Append(rewardName).ToArray();
                            }
                        }
                    }
                    string reward = rewards[RNG.RandiRange(0, rewards.Length - 1)];
                    remainingRichness -= reward.Split('_')[0].ToInt();
                    
                    Reward rew = GameManager.Instance.REWARDS_REGISTER[reward].Instantiate<Reward>();
                    rew.Encounter = enc;
                    enc.Rewards = enc.Rewards.Append(rew).ToArray();
                    enc.AddChild(rew);
                }
                enc.EncounterNode = encounterNode;
                encounterNode.Encounters = encounterNode.Encounters.Append(enc).ToArray();
                encounterNode.AddChild(enc);
            }
        }
    }

    public Unit CreateUnit(string unitName, Vector2 position, int team, Loot.Rarity rarity = Loot.Rarity.Common, string unitPool = "")
    {
        unitName = unitName.ToLower();
        if(!GameManager.Instance.UNITS_REGISTER.ContainsKey(unitName)) {
            GameManager.Instance.Log($"Unit {unitName} not found!", GameManager.LoggingLevel.Warning);
            return null;
        }
        Unit unit = GameManager.Instance.UNITS_REGISTER[unitName].Instantiate<Unit>();
        unit.UnitType = unitName;
        unit.GlobalPosition = position;
        unit.Team = team;
        unit.Rarity = rarity;
        unit.UnitPool = unitPool;
        AddChild(unit);
        return unit;
    }

    public Unit GetNearestUnit(Vector2 position, int team = -1) {
        return Units.Where(u => u.Team == team || team == -1).OrderBy(u => u.GlobalPosition.DistanceTo(position)).FirstOrDefault();
    }
    public Unit GetNearestSelectedUnit(Vector2 position) {
        return SelectedUnits.OrderBy(u => u.GlobalPosition.DistanceTo(position)).FirstOrDefault();
    }

    public Unit GetNearestResourceDropoff(Vector2 position, int team) {
        Unit[] dropoffs = ResourceDropoffs.Where(dropoff => dropoff.Team == team).ToArray();
        if(dropoffs.Length == 0) {
            return null;
        }
        Unit nearestDropoff = dropoffs[0];
        float nearestDistance = position.DistanceTo(nearestDropoff.GlobalPosition);
        for(int i = 1; i < dropoffs.Length; i++) {
            float distance = position.DistanceTo(dropoffs[i].GlobalPosition);
            if(distance < nearestDistance) {
                nearestDistance = distance;
                nearestDropoff = dropoffs[i];
            }
        }
        return nearestDropoff;
    }

    public Unit GetNearestResourceDeposit(Vector2 position) {
        if(ResourceDeposits.Length == 0) {
            return null;
        }
        Unit nearestDeposit = ResourceDeposits[0];
        float nearestDistance = position.DistanceTo(nearestDeposit.GlobalPosition);
        for(int i = 1; i < ResourceDeposits.Length; i++) {
            float distance = position.DistanceTo(ResourceDeposits[i].GlobalPosition);
            if(distance < nearestDistance) {
                nearestDistance = distance;
                nearestDeposit = ResourceDeposits[i];
            }
        }
        return nearestDeposit;
    }

    public void OnAreaSelected(Area2D area)
    {
        if(CommandCard.Instance.IsMouseOverCommandCard) {
            return;
        }
        if(!Input.IsActionPressed("shift")) {
            foreach(Unit unit in SelectedUnits) {
                unit.EmitSignal("UnitDeselected", unit);
            }
        }
        foreach(Area2D node in area.GetOverlappingAreas()) {
            if(node is Unit unit) {
                if(unit.Team == 1) {
                    unit.EmitSignal("UnitSelected", unit);
                }
            }
        }
        CommandCard.Instance.CreateAbilityButtons();
    }

    public Unit[] GetUnitsInRadius(Vector2 position, float radius, int team = -1) {
        return Units.Where(u => u.Team == team || team == -1).Where(u => u.GlobalPosition.DistanceTo(position) <= radius).ToArray();
    }

    public Augment[] GetAugments()
    {
        return Augments;
    }

    public void AddAugment(Augment augment)
    {
        Augments = Augments.Append(augment).ToArray();
        foreach(Unit unit in Units) {
            if(unit.HasAllFlags(augment.RequiredFlags) == false || augment.Team != unit.Team) {
                continue;
            }
            foreach(PropertyModifier propertyModifier in augment.PropertyModifiers) {
                unit.PropertyModifiers = unit.PropertyModifiers.Append(propertyModifier).ToArray();
            }
        }
    }

    public void AddAugment(string augmentName, int team = 1) {
        if(!GameManager.Instance.AUGMENTS_REGISTER.ContainsKey(augmentName)) {
            GameManager.Instance.Log($"Augment {augmentName} not found!", GameManager.LoggingLevel.Warning);
            return;
        }
        Type augmentType = GameManager.Instance.AUGMENTS_REGISTER[augmentName];
        AddAugment((Augment)Activator.CreateInstance(augmentType, team));
    }

    public void RemoveAugment(Augment augment)
    {
        Augments = Augments.Where(a => a != augment).ToArray();
        foreach(Unit unit in Units) {
            foreach(PropertyModifier propertyModifier in augment.PropertyModifiers) {
                unit.PropertyModifiers = unit.PropertyModifiers.Where(p => p != propertyModifier).ToArray();
            }
        }
    }

    void SpawnLootOnUnitDeath(Unit deadUnit, Unit killer)
    {
        if(deadUnit.Team == 1) {
            return;
        }
        for(int i = 0; i < deadUnit.LootAmount; i++) {
            float dropFactor = (float)GetDropFactor(deadUnit.Rarity);
            float randf = RNG.RandfRange(0, 1f);
            while(dropFactor > randf) {
                randf = RNG.RandfRange(0, 1f);
                int dropType = (int)GetRarity(deadUnit.Rarity);
                float deadUnitValue = deadUnit.GetValue();
                if(dropType == (int)Loot.Rarity.Common) {
                    LootRandomCredits lrc = new LootRandomCredits("lrc", deadUnit.Rarity, RNG.RandfRange(0.5f, 1.5f) * deadUnitValue);
                    Loots = Loots.Append(lrc).ToArray();
                    CallDeferred("add_child", lrc);
                    lrc.GlobalPosition = deadUnit.GlobalPosition;
                    if(RNG.RandfRange(0, 1) > 0.6f) {
                        LootRandomScrap lrs = new LootRandomScrap("lrs", 1);
                        Loots = Loots.Append(lrs).ToArray();
                        CallDeferred("add_child", lrs);
                        lrs.GlobalPosition = deadUnit.GlobalPosition;
                    }
                }
                else if(dropType == (int)Loot.Rarity.Rare) {
                    LootRandomAlloys lra = new LootRandomAlloys("lra", 1);
                    Loots = Loots.Append(lra).ToArray();
                    CallDeferred("add_child", lra);
                    lra.GlobalPosition = deadUnit.GlobalPosition;
                    dropFactor *= 1.25f;
                }
                else if(dropType == (int)Loot.Rarity.Epic) {
                    LootRandomExoticMatter lrem = new LootRandomExoticMatter("lrem", 1);
                    Loots = Loots.Append(lrem).ToArray();
                    CallDeferred("add_child", lrem);
                    lrem.GlobalPosition = deadUnit.GlobalPosition;
                    dropFactor *= 1.7f;
                }
                else if(dropType == (int)Loot.Rarity.Legendary) {
                    LootRandomStarEssence lrse = new LootRandomStarEssence("lrse", 1);
                    Loots = Loots.Append(lrse).ToArray();
                    CallDeferred("add_child", lrse);
                    lrse.GlobalPosition = deadUnit.GlobalPosition;
                    dropFactor *= 2f;
                }
                dropFactor /= 2;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if(Input.IsActionJustPressed("right_click")) {
            Unit[] hoveredUnits = HoveredUnits.Where(u => u.Team != 1 && u.Team != 0).ToArray();
            if(hoveredUnits.Length > 0) {
                foreach(Unit unit in SelectedUnits) {
                    unit.AttackTarget = hoveredUnits[0];
                    unit.ForcedAttackTarget = true;
                }
            }
            if(HoveredLoot.Length > 0 && SelectedUnits.Length > 0) {
                if(GetNearestSelectedUnit(HoveredLoot[0].GlobalPosition).GlobalPosition.DistanceTo(HoveredLoot[0].GlobalPosition) < LootCollectionRange) {
                    HoveredLoot[0].PickUp(GetNearestSelectedUnit(HoveredLoot[0].GlobalPosition));
                }
            }
        }
        if(HoveredUnits.Length > 0) {
            if(LastHoveredUnit == HoveredUnits[0]) {
                TimeSinceHoverSelection += (float)delta;
            }
            else {
                TimeSinceHoverSelection = 0f;
                LastHoveredUnit = HoveredUnits[0];
                UnitTooltip?.QueueFree();
                UnitTooltip = null;
            }
            if(UnitTooltip == null && TimeSinceHoverSelection > 0.25f) {
                UnitTooltip = new UnitTooltip(LastHoveredUnit);
                AddChild(UnitTooltip);
                UnitTooltip.GlobalPosition = GetGlobalMousePosition();
            }
        }
        else {
            UnitTooltip?.QueueFree();
            UnitTooltip = null;
            TimeSinceHoverSelection = 0f;
        }

        if(Input.IsActionJustPressed("left_click")) {
            if(HoveredInteractableObjects.Length > 0) {
                HoveredInteractableObjects[0].Use();
            }
        }

        
    }
}
