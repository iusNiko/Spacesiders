using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Unit : Area2D
{
    [Export] public float Health = 0f;
    [Export] float _maxHealth = 0f;
    public float MaxHealth {
        get {
            float maxHealth = _maxHealth;
            foreach(PropertyModifier modifier in PropertyModifiers.Where(m => m.PropertyName == "MaxHealth")) {
                maxHealth += modifier.FlatModifier;
                maxHealth *= modifier.PercentageModifier;
            }
            return maxHealth;
        }
        set {
            float healthPercentage = Health / MaxHealth;
            _maxHealth = value;
            Health = MaxHealth * healthPercentage;
        }
    }
    [Export] float _armor = 0f;
    public float Armor {
        get {
            float armor = _armor;
            foreach(PropertyModifier modifier in PropertyModifiers.Where(m => m.PropertyName == "Armor")) {
                armor += modifier.FlatModifier;
                armor *= modifier.PercentageModifier;
            }
            return armor;
        }
        set { _armor = value; }
    }
    [Export] float _moveSpeed = 0f;
    public float MoveSpeed {
        get {
            float moveSpeed = _moveSpeed;
            foreach(PropertyModifier modifier in PropertyModifiers.Where(m => m.PropertyName == "MoveSpeed")) {
                moveSpeed += modifier.FlatModifier;
                moveSpeed *= modifier.PercentageModifier;
            }
            return moveSpeed;
        }
        set { _moveSpeed = value; }
    }
    [Export] public int Team = 1;
    [Export] public float Radius = 6f;
    public float AttackRange {
        get {
            float attackRange = 0f;
            foreach(Weapon weapon in Weapons) {
                if(weapon.Range > attackRange) {
                    attackRange = weapon.Range;
                }
            }
            return attackRange;
        }
    }
    public Unit AttackTarget = null;
    public bool ForcedAttackTarget = false;
    public bool IsAttacking  {
        get {
            if(AttackTarget == null) {
                return false;
            }
            return AttackTarget.GlobalPosition.DistanceTo(GlobalPosition) <= AttackRange;
        }
    }
    public StateManager StateManager;
    public Weapon[] Weapons = Array.Empty<Weapon>();
    public Ability[] Abilities = Array.Empty<Ability>();
    public bool Selected {
        get {
            return World.Instance.SelectedUnits.Contains(this);
        }
    }
    public PropertyModifier[] PropertyModifiers = Array.Empty<PropertyModifier>();
    public Queue<Order> OrderQueue = new Queue<Order>();
    public Vector2 MovementTarget;
    public float Delta;
    public string UnitType = "";
    public Panel SelectionBox;
    [Signal] public delegate void UnitSelectedEventHandler(Unit unit);
    [Signal] public delegate void UnitDeselectedEventHandler(Unit unit);
    [Export] public float BonusAttackRotationDegrees = 0f;
    [Export] public Loot.Rarity Rarity = Loot.Rarity.Common;
    [Export] public int LootAmount = 1;
    public Color SelectionBoxColor = new Color(1f, 1f, 1f, 1f);
    public AnimatedSprite2D Animation;
    public int BlinkFrames = 0;
    [Export] public bool Invincible = false;
    public bool IsDead = false;
    public ProductionQueue ProductionQueue = null;
    [Export] public string[] Flags = Array.Empty<string>();
    [Export] public string[] CostTypes = Array.Empty<string>();
    [Export] public float[] CostAmounts = Array.Empty<float>();
    public Veterancy Veterancy = null;
    [Export] public string UnitPool = "";

    public Unit SearchClosestTarget() {
        Unit[] units = World.Instance.Units.Where(u => u.Team != Team && u.Team != 0).ToArray();
        if(units.Length == 0) {
            return null;
        }
        Unit closestUnit = units[0];
        float closestDistance = GlobalPosition.DistanceTo(closestUnit.GlobalPosition);
        for(int i = 1; i < units.Length; i++) {
            float distance = GlobalPosition.DistanceTo(units[i].GlobalPosition);
            if(distance < closestDistance) {
                closestUnit = units[i];
                closestDistance = distance;
            }
        }
        return closestUnit;
    }

    public void Alert(Vector2 position) {
        if(Team == 1) {
            return;
        }
        MovementTarget = position;
        StateManager.ChangeState(new StateWalking());
        foreach(Unit unit in World.Instance.GetUnitsInRadius(GlobalPosition, 150f, Team).Where(u => u.StateManager.CurrentState is not StateWalking && u.AttackRange < u.GlobalPosition.DistanceTo(position))) {
            unit.Alert(position);
        }
    }

    public bool HasAbility(Type abilityType) {
        return Abilities.Any(a => a.GetType() == abilityType);
    }

    public bool HasAllFlags(string[] flags) {
        foreach(string flag in flags) {
            if(Flags.Contains(flag) == false) {
                return false;
            }
        }
        return true;
    }

    public void AddWeapon(Weapon weapon) {
        GetNode<Node>("Weapons").AddChild(weapon);
        Weapons = GetNode<Node>("Weapons").GetChildren().Cast<Weapon>().ToArray();
    }

    public Weapon AddWeapon(string weaponName) {
        Weapon weapon = GameManager.Instance.WEAPONS_REGISTER[weaponName].Instantiate<Weapon>();
        GetNode<Node>("Weapons").AddChild(weapon);
        Weapons = GetNode<Node>("Weapons").GetChildren().Cast<Weapon>().ToArray();
        return weapon;
    }
    public Ability AddAbility(string ability, string shortcut) {
        AbilityTrain abil = GameManager.Instance.ABILITIES_REGISTER[ability].Instantiate<AbilityTrain>();
        abil.Shortcut = shortcut;
        GetNode<Node>("Abilities").AddChild(abil);
        Abilities = GetNode<Node>("Abilities").GetChildren().Cast<Ability>().ToArray();
        return abil;
    }

    public virtual void Damage(float damage, Weapon weapon, Projectile projectile) {
        if(Invincible || Health <= 0) {
            return;
        }
        damage = damage * 100f / Mathf.Max(100f + Armor - projectile.ArmorPenetration, 100f);
        Health -= damage;
        FloatingText.CreateFloatingText(((int)damage).ToString(), new Color(1f, 0f, 0f, 1f), GlobalPosition);
        if(Health <= 0) {
            OnDeath();
            World.Instance.EmitSignal("UnitDied", this, weapon.SourceUnit);
        }
        BlinkFrames = GameManager.Instance.STANDARD_BLINK_TIME;
    }

    public virtual void OnDeath() {
        World.Instance.Units = World.Instance.Units.Where(u => u != this).ToArray();
        World.Instance.HoveredUnits = World.Instance.HoveredUnits.Where(u => u != this).ToArray();
        World.Instance.SelectedUnits = World.Instance.SelectedUnits.Where(u => u != this).ToArray();
        World.Instance.ResourceDeposits = World.Instance.ResourceDeposits.Where(u => u != this).ToArray();
        World.Instance.ResourceDropoffs = World.Instance.ResourceDropoffs.Where(u => u != this).ToArray();
        
        Visible = false;
        CollisionLayer = 0;
        StateManager.ChangeState(null);
        IsDead = true;
    }
    public virtual void OnUnitSelected(Unit unit) {
        World.Instance.SelectedUnits = World.Instance.SelectedUnits.Append(this).ToArray();
        SelectionBox.Modulate = SelectionBoxColor;
        SelectionBox.Visible = true;
    }
    public virtual void OnUnitDeselected(Unit unit) {
        World.Instance.SelectedUnits = World.Instance.SelectedUnits.Where(u => u != this).ToArray();
        SelectionBox.Visible = false;
    }

    public virtual void ClearOrders() {
        while(OrderQueue.Count > 0) {
            OrderQueue.Dequeue().Canceled();
        }
    }

    public ResourceDeposit[] GetResourceDeposits() {
        return GetChildren().Where(c => c is ResourceDeposit).Cast<ResourceDeposit>().ToArray();
    }

    public float GetValue() {
        float value = MaxHealth * (1f/(100f / Mathf.Max(100f + Armor, 100f)));
        foreach(Weapon weapon in Weapons) {
            float dmg = (float)GameManager.GetPropertyFromPackedScene(weapon.ProjectilePrefab, "_damage");
            foreach(PropertyModifier modifier in PropertyModifiers.Where(m => m.PropertyName == "Damage")) {
                dmg += modifier.FlatModifier;
                dmg *= modifier.PercentageModifier;
            }
            float dps = dmg * weapon.AttackSpeed;
            value += dps * 3;
        }
        return value;
    }

    public override void _MouseEnter()
    {
        World.Instance.HoveredUnits = World.Instance.HoveredUnits.Append(this).ToArray();
        if(World.Instance.SelectedUnits.Contains(this)) {
            return;
        }
        SelectionBox.Visible = true;
        SelectionBox.Modulate = new Color(SelectionBoxColor.R, SelectionBoxColor.G, SelectionBoxColor.B, SelectionBoxColor.A * 0.4f);
        
    }

    public override void _MouseExit()
    {
        World.Instance.HoveredUnits = World.Instance.HoveredUnits.Where(u => u != this).ToArray();
        if(World.Instance.SelectedUnits.Contains(this)) {
            return;
        }
        SelectionBox.Visible = false;
        SelectionBox.Modulate = SelectionBoxColor;
    }

    public override void _Ready() {
        World.Instance.Units = World.Instance.Units.Append(this).ToArray();
        if(GetResourceDeposits().Length > 0) {
            World.Instance.ResourceDeposits = World.Instance.ResourceDeposits.Append(this).ToArray();
        }
        if(GetChildren().Where(c => c is ResourceDropoff).Any()) {
            World.Instance.ResourceDropoffs = World.Instance.ResourceDropoffs.Append(this).ToArray();
        }
        MovementTarget = GlobalPosition;
        StateManager = new StateManager(this);
        StateManager.ChangeState(new StateIdle());
        UnitSelected += OnUnitSelected;
        UnitDeselected += OnUnitDeselected;
        Animation = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        Weapons = GetNode<Node>("Weapons").GetChildren().Cast<Weapon>().ToArray();
        Abilities = GetNode<Node>("Abilities").GetChildren().Cast<Ability>().ToArray();
        SelectionBox = GetNode<Panel>("SelectionBox");
        ProductionQueue = GetNode<ProductionQueue>("ProductionQueue");
        if(Team == 1) {
            SelectionBoxColor = new Color(0f, 1f, 0f, 1f);
        }
        else if(Team != 0) {
            SelectionBoxColor = new Color(1f, 0f, 0f, 1f);
        }
        SelectionBox.Modulate = SelectionBoxColor;
        foreach(Augment augment in World.Instance.GetAugments()) {
            if(HasAllFlags(augment.RequiredFlags) && augment.Team == Team) {
                foreach(PropertyModifier propertyModifier in augment.PropertyModifiers) {
                    PropertyModifiers = PropertyModifiers.Append(propertyModifier).ToArray();
                }
            }
        }
        if(Team == 1) {
            Veterancy = new VeterancyImperial(this);
        }
        switch(Rarity) {
            case Loot.Rarity.Common:
                break;
            case Loot.Rarity.Rare:
                Modulate = new Color(1f, 1f, 1.3f, 1f);
                break;
            case Loot.Rarity.Epic:
                Modulate = new Color(1.3f, 1f, 1.3f, 1f);
                break;
            case Loot.Rarity.Legendary:
                Modulate = new Color(1.9f, 1.9f, 1f, 1f);
                break;
        }
        PropertyModifiers = PropertyModifiers.Append(new PropertyModifier("MaxHealth", World.Instance.GetRarityValue(Rarity))).ToArray();
        PropertyModifiers = PropertyModifiers.Append(new PropertyModifier("Damage", World.Instance.GetRarityValue(Rarity))).ToArray();
        Health = MaxHealth;
    }

    public override void _PhysicsProcess(double delta) {
        if(IsDead) {
            return;
        }
        Delta = (float) delta;
        if(BlinkFrames > 0) {
            BlinkFrames--;
            Animation.Modulate = new Color(1.4f, 1.4f, 1.4f, 1.4f);
        }
        else {
            Animation.Modulate = new Color(1f, 1f, 1f, 1f);
        }

        if(!IsInstanceValid(AttackTarget)) AttackTarget = null;

        if(ForcedAttackTarget && AttackTarget != null && !AttackTarget.IsDead) {
            if(AttackTarget.GlobalPosition.DistanceTo(GlobalPosition) > AttackRange) {
                MovementTarget = AttackTarget.GlobalPosition;
                StateManager.ChangeState(new StateWalking());
            }
            else {
                StateManager.ChangeState(new StateIdle());
                for(int i = 0; i < Weapons.Length; i++) {
                    if(Weapons[i].TimeSinceLastShot >= 1/Weapons[i].AttackSpeed && AttackTarget != null && AttackTarget.GlobalPosition.DistanceTo(GlobalPosition) <= Weapons[i].Range) {
                        Weapons[i].Shoot();
                    }
                }
            }
        }
        else {
            ForcedAttackTarget = false;
            AttackTarget = SearchClosestTarget();

            for(int i = 0; i < Weapons.Length; i++) {
                if(Weapons[i].TimeSinceLastShot >= 1/Weapons[i].AttackSpeed && AttackTarget != null && AttackTarget.GlobalPosition.DistanceTo(GlobalPosition) <= Weapons[i].Range) {
                    Weapons[i].Shoot();
                }
            }
        }

        if(Team > 1 && StateManager.CurrentState is StateWalking && AttackTarget.GlobalPosition.DistanceTo(GlobalPosition) <= AttackRange) {
            StateManager.ChangeState(new StateIdle());
        }

        foreach(Ability ability in Abilities) {
            ability.TimeSinceLastUse += Delta;
            if(ability.TimeSinceLastUse >= ability.Cooldown && Input.IsActionJustPressed(ability.Shortcut) && Selected) {
                ability.TimeSinceLastUse = 0;
                ability.Use();
            }
        }

        StateManager.CurrentState.Update(Delta);
        if(OrderQueue.Count > 0) {
            Order order = OrderQueue.Peek();
            if(order.Executed == false) {
                order.Execute();
                order.Executed = true;
            }
            order.Update(Delta);
            if(order.IsFinished()) {
                order.Finished();
                OrderQueue.Dequeue();
            }
        }

        if(Veterancy != null) {
            Veterancy.Update(Delta);
        }

        if(GlobalPosition.X > World.Instance.Map.EasternBoundary) {
            GlobalPosition = new Vector2(World.Instance.Map.EasternBoundary, GlobalPosition.Y);
        }
        if(GlobalPosition.X < World.Instance.Map.WesternBoundary) {
            GlobalPosition = new Vector2(World.Instance.Map.WesternBoundary, GlobalPosition.Y);
        }
        if(GlobalPosition.Y < World.Instance.Map.NorthernBoundary) {
            GlobalPosition = new Vector2(GlobalPosition.X, World.Instance.Map.NorthernBoundary);
        }
        if(GlobalPosition.Y > World.Instance.Map.SouthernBoundary) {
            GlobalPosition = new Vector2(GlobalPosition.X, World.Instance.Map.SouthernBoundary);
        }
    }
}
