using Godot;
using System;
using System.Linq;

public partial class NewSaveButton : Button
{
    public override void _Pressed()
    {
        var saveDir = DirAccess.Open("user://saves");
        if (saveDir == null)
        {
            GD.Print("No save directory found. Creating one...");
            saveDir = DirAccess.Open("user://");
            saveDir.MakeDir("saves");
            saveDir = DirAccess.Open("user://saves");
        }

        int biggestSaveNumber = -1;
        foreach (var file in saveDir.GetFiles())
        {
            var fileName = file.GetFile();
            if (fileName.StartsWith("save"))
            {
                var number = int.Parse(fileName.Substring(4, fileName.Length - 9));
                if (number > biggestSaveNumber)
                {
                    biggestSaveNumber = number;
                }
            }
        }

        biggestSaveNumber++;
        
        var saveFile = FileAccess.Open("user://saves/save" + biggestSaveNumber.ToString() + ".json", FileAccess.ModeFlags.Write);
        Godot.Collections.Dictionary<string, Variant> dictData = new();
        dictData["metadata"] = new Godot.Collections.Dictionary();
        dictData["metadata"].AsGodotDictionary()["save_name"] = "save" + biggestSaveNumber.ToString();
        dictData["metadata"].AsGodotDictionary()["save_date"] = DateTime.Now.ToString();
        dictData["gamedata"] = new Godot.Collections.Dictionary();
        dictData["gamedata"].AsGodotDictionary()["resources"] = new Godot.Collections.Dictionary();
        foreach(string resource in Enum.GetNames(typeof(GameManager.ResourceTypes)))
        {
            dictData["gamedata"].AsGodotDictionary()["resources"].AsGodotDictionary()[resource] = "0";
        }
        dictData["unlocked_units"] = new Godot.Collections.Array();
        dictData["unlocked_units"].AsGodotArray().Add("fighter");
        string saveData = Json.Stringify(dictData);
        
        saveFile.StoreString(saveData);
        saveFile.Close();

        GetNode<SavesList>("../SavesList").ReloadSavesList();
    }
}
