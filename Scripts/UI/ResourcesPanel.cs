using Godot;
using System;
using System.Linq;

public partial class ResourcesPanel : VBoxContainer
{
    Label[] resourceLabels = Array.Empty<Label>();
    string[] resourceNames = Array.Empty<string>();
    public override void _Ready()
    {
        foreach(GameManager.ResourceTypes resourceType in Enum.GetValues(typeof(GameManager.ResourceTypes))) {
            Label label = new Label();
            label.Text = Enum.GetName(resourceType);
            AddChild(label);
            resourceNames = resourceNames.Append(Enum.GetName(resourceType)).ToArray();
            resourceLabels = resourceLabels.Append(label).ToArray();
        }
    }

    public override void _Process(double delta)
    {
        for(int i = 0; i < resourceLabels.Length; i++) {
            resourceLabels[i].Text = $"{Enum.GetName((GameManager.ResourceTypes)i)}: {GameManager.Instance.Resources[resourceNames[i]]}";
        }
    }
}
