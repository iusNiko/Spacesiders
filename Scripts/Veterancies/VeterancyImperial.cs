using Godot;
using System;

public partial class VeterancyImperial : Veterancy {
    public VeterancyImperial(Unit unit) : base(unit) {
        Levels = new VeterancyLevel[] {
            new VeterancyLevel {
                Name = "Veterancy 1",
                RequiredExperience = 100f,
                modifiers = new PropertyModifier[] {
                    new PropertyModifier("Damage", 1.1f),
                    new PropertyModifier("MaxHealth", 1.1f)
                }
            },
            new VeterancyLevel {
                Name = "Veterancy 2",
                RequiredExperience = 200f,
                modifiers = new PropertyModifier[] {
                    new PropertyModifier("Damage", 1.1f),
                    new PropertyModifier("MaxHealth", 1.1f)
                }
            },
            new VeterancyLevel {
                Name = "Veterancy 3",
                RequiredExperience = 300f,
                modifiers = new PropertyModifier[] {
                    new PropertyModifier("Damage", 1.2f),
                    new PropertyModifier("MaxHealth", 1.2f)
                }
            }
        };
    }
}