using Godot;
using System;
using System.Linq;

public partial class Veterancy
{
    public Unit Unit;
    public string NoLevelName = "Unranked";
    public class VeterancyLevel {
        public string Name;
        public float RequiredExperience;
        public PropertyModifier[] modifiers = Array.Empty<PropertyModifier>();
    }
    protected VeterancyLevel[] Levels = Array.Empty<VeterancyLevel>();
    public int CurrentLevel = -1;
    public float Experience = 0f;
    public float ExperienceToNextLevel {
        get {
            if(CurrentLevel >= Levels.Length - 1) {
                return -1f;
            }
            return Levels[CurrentLevel + 1].RequiredExperience - Experience;
        }
    }
    public string GetLevelName() {
        if(CurrentLevel == -1) {
            return NoLevelName;
        }
        return Levels[CurrentLevel].Name;
    }
    public Veterancy(Unit unit) {
        Unit = unit;
        World.Instance.UnitDied += OnUnitDeath;
    }
    public void OnUnitDeath(Unit deadUnit, Unit killer) {
        if(killer == Unit) {
            Experience += deadUnit.GetValue();
            GD.Print("Experience: " + Experience);
        }
    }

    public virtual void Update(float delta) {
        if(CurrentLevel >= Levels.Length - 1) {
            return;
        }
        if(Experience >= Levels[CurrentLevel + 1].RequiredExperience) {
            CurrentLevel++;
            Experience = Experience - Levels[CurrentLevel].RequiredExperience;
            foreach(PropertyModifier modifier in Levels[CurrentLevel].modifiers) {
                Unit.PropertyModifiers = Unit.PropertyModifiers.Append(modifier).ToArray();
            }
        }
    }

}
