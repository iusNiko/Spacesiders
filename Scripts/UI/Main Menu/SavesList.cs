using Godot;
using System;

public partial class SavesList : ItemList
{
    public void ReloadSavesList()
    {
        Clear();
        var saveDir = DirAccess.Open("user://saves");
        foreach (var file in saveDir.GetFiles())
        {
            var fileName = file.GetFile();
            if (fileName.StartsWith("save"))
            {
                AddItem(fileName.TrimSuffix(".json"));
            }
        }
    }
    public override void _Ready()
    {
        ReloadSavesList();
        Select(0);
    }
}
