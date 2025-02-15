using Godot;
using System;

public partial class PropertyModifier
{
    public string PropertyName;
    public float PercentageModifier;
    public float FlatModifier;
    public PropertyModifier(string propertyName = "", float percentageModifier = 1f, float flatModifier = 0f)
    {
        PropertyName = propertyName;
        PercentageModifier = percentageModifier;
        FlatModifier = flatModifier;
    }
}
