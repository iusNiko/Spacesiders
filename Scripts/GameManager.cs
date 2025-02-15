using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

public partial class GameManager : Node
{
    public static GameManager Instance;
    public enum LoggingMode {
        DebugConsole,
        Console,
        File,
        FileAndConsole
    }
    public enum LoggingLevel {
        Info,
        Warning,
        Error
    }
    public enum ResourceTypes {
        Credits,
        Scrap,
        Alloys,
        ExoticMatter,
        StarEssence
    }
    public Dictionary<string, float> Resources = new();
    [Export] public LoggingMode loggingMode = LoggingMode.DebugConsole;
    public Dictionary<string, PackedScene> UNITS_REGISTER = new();
    public Dictionary<string, Dictionary<string, float>> UNITS_COSTS_REGISTER = new();
    public Dictionary<string, PackedScene> ENCOUNTERS_REGISTER = new();
    public Dictionary<string, PackedScene> REWARDS_REGISTER = new();
    public Dictionary<string, Texture2D> REWARD_ICONS_REGISTER = new();
    public Dictionary<string, PackedScene> WEAPONS_REGISTER = new();
    public Dictionary<string, PackedScene> ABILITIES_REGISTER = new();
    public Dictionary<string, Type> AUGMENTS_REGISTER = new();
    public string[] UNLOCKED_WEAPONS = Array.Empty<string>();
    public string[] UNLOCKED_UNITS = Array.Empty<string>();
    public Godot.Collections.Dictionary<string, Variant> SaveData = new();
    public Vector2[][] ENCOUNTER_PATTERNS = new Vector2[][] {
        // Circle pattern
        new Vector2[] {
            new Vector2(0, 0),
            new Vector2(900, 0),
            new Vector2(636.396f, 636.396f),
            new Vector2(0, 900),
            new Vector2(-636.396f, 636.396f),
            new Vector2(-900, 0),
            new Vector2(-636.396f, -636.396f),
            new Vector2(0, -900),
            new Vector2(636.396f, -636.396f)
        },
        // Square pattern
        new Vector2[] {
            new Vector2(0, 0),
            new Vector2(-600, -600),
            new Vector2(-600, 0),
            new Vector2(-600, 600),
            new Vector2(0, -600),
            new Vector2(0, 600),
            new Vector2(600, -600),
            new Vector2(600, 0),
            new Vector2(600, 600)
        },
    };
    public Dictionary<string, Dictionary<string, int>> UNIT_POOLS = new() {
        {
            "west_outskirts",
            new Dictionary<string, int> {
                {"fighter", 75},
                {"pirate_corvette", 40},
                {"pirate_frigate", 50},
                {"electric_drone", 50}
            }
        }
    };
    public int STANDARD_BLINK_TIME = 3;

    public string GetRandomUnitFromPool(string poolName) {
        if(UNIT_POOLS.ContainsKey(poolName)) {
            Dictionary<string, int> pool = UNIT_POOLS[poolName];
            int total = 0;
            foreach(int value in pool.Values) {
                total += value;
            }
            int random = World.Instance.RNG.RandiRange(0, total);
            int current = 0;
            foreach(string key in pool.Keys) {
                current += pool[key];
                if(random <= current) {
                    return key;
                }
            }
        }
        return "";
    }

    public void LoadSave(string saveName)
    {
        var saveFile = FileAccess.Open("user://saves/" + saveName + ".json", FileAccess.ModeFlags.Read);
        if (saveFile == null)
        {
            Log($"[ERROR] Unable to load save file: {saveName}.json", LoggingLevel.Error);
            return;
        }
        string saveText = saveFile.GetAsText();
        Json saveData = new();
        if(saveData.Parse(saveText) != Error.Ok)
        {
            Log($"[ERROR] Unable to parse save file: {saveName}.json", LoggingLevel.Error);
            return;
        }
        SaveData = (Godot.Collections.Dictionary<string, Variant>)saveData.Data;
        Log($"Loaded save file: {saveName}.json");
        saveFile.Close();

        // Apply save data
        UNLOCKED_UNITS = Array.Empty<string>();
        foreach (string unit in (Godot.Collections.Array)SaveData["unlocked_units"])
        {
            UNLOCKED_UNITS = UNLOCKED_UNITS.Append(unit).ToArray();
        }
        foreach (string resource in Enum.GetNames(typeof(ResourceTypes)))
        {
            Resources[resource] = SaveData["gamedata"].AsGodotDictionary()["resources"].AsGodotDictionary()[resource].AsString().ToFloat();
        }
    }

    public void SaveGame()
    {
        string saveName = SaveData["gamedata"].AsGodotDictionary()["save_name"].AsString();
        var saveFile = FileAccess.Open("user://saves/" + saveName + ".json", FileAccess.ModeFlags.Write);
        if (saveFile == null)
        {
            Log($"[ERROR] Unable to save game to file: {saveName}.json", LoggingLevel.Error);
            return;
        }
        foreach (string resource in Enum.GetNames(typeof(ResourceTypes)))
        {
            SaveData["gamedata"].AsGodotDictionary()["resources"].AsGodotDictionary()[resource] = Resources[resource].ToString();
        }
        SaveData["unlocked_units"] = new Godot.Collections.Array();
        foreach (string unit in UNLOCKED_UNITS)
        {
            SaveData["unlocked_units"].AsGodotArray().Add(unit);
        }
        string saveText = Json.Stringify(SaveData);
        saveFile.StoreString(saveText);
        saveFile.Close();
        Log($"Saved game to file: {saveName}.json");
    }

    public void UnlockWeapon(string weaponName)
    {
        if(!UNLOCKED_WEAPONS.Contains(weaponName) && WEAPONS_REGISTER.ContainsKey(weaponName)) {
            UNLOCKED_WEAPONS = UNLOCKED_WEAPONS.Append(weaponName).ToArray();
        }
        else {
            Log($"[WARNING] Unable to unlock weapon: {weaponName}", LoggingLevel.Warning);
        }
    }

    public static Variant GetPropertyFromPackedScene(PackedScene scene, string property) {
        SceneState scene_state = scene.GetState();
        Variant? variant = null;
        for(int node_prop_idx = 0; node_prop_idx < scene_state.GetNodePropertyCount(0); node_prop_idx++) {
            if(scene_state.GetNodePropertyName(0, node_prop_idx) == property) {
                return scene_state.GetNodePropertyValue(0, node_prop_idx);
            }
        }
        if(variant == null) {
            variant = 0f;
        }
        return (Variant)variant;
    }

    public static string GetUnitName(string type) {
        string t = type.Replace("_", " ");
        string[] words = t.Split(" ");
        string result = "";
        foreach(string word in words) {
            result += word.Substring(0, 1).ToUpper() + word.Substring(1) + " ";
        }
        return result.Trim();
    }

    public static string GetUnitType(string name) {
        return name.Replace(" ", "_").ToLower();
    }

    public string CanAfford(string[] costTypes, float[] costAmounts) {
        for(int i = 0; i < costTypes.Length; i++) {
            if(Resources[costTypes[i]] < costAmounts[i]) {
                return costTypes[i];
            }
        }
        return "";
    }

    public void Pay(string[] costTypes, float[] costAmounts) {
        for(int i = 0; i < costTypes.Length; i++) {
            Resources[costTypes[i]] -= costAmounts[i];
        }
    }

    public void Crash() {
        while(true) {
            
        }
    }
    public void Log(string message, LoggingLevel level = LoggingLevel.Info) {
        if(loggingMode == LoggingMode.DebugConsole) {
            switch(level) {
                case LoggingLevel.Info:
                    GD.Print(message);
                    break;
                case LoggingLevel.Warning:
                    GD.PrintErr(message);
                    break;
                case LoggingLevel.Error:
                    GD.PrintErr(message);
                    break;
            }
        }
        if(loggingMode == LoggingMode.Console || loggingMode == LoggingMode.FileAndConsole) {
            if(Console.Instance == null) {
                return;
            }
            switch(level) {
                case LoggingLevel.Info:
                    Console.Instance.ConsoleText.Text += "\n" + message;
                    break;
                case LoggingLevel.Warning:
                    Console.Instance.ConsoleText.Text += "\n" + $"[WARNING] {message}";
                    break;
                case LoggingLevel.Error:
                    Console.Instance.ConsoleText.Text += "\n" + $"[ERROR] {message}";
                    break;
            }
        }
    }

    public void PREFABS_REGISTRATION()
    {
        string path = "res://Prefabs/Units/";
        var dir = DirAccess.Open(path);
        if (dir != null)
        {
            foreach(string fileName in dir.GetFiles())
            {
                if(fileName.Contains(".tscn"))
                {
                    string fn = fileName;
                    if(fn.Contains(".remap")) fn = fn.Replace(".remap", "");
                    Log($"Found Unit file: {path + fn}");
                    PackedScene unit = (PackedScene)GD.Load(path + fn);
                    UNITS_REGISTER[fileName.Substr(0, fileName.Find(".tscn")).ToLower()] = unit;
                }
            }
        }
        else
        {
            Log($"[ERROR] Unable to find Units folder! Can't proceed further.", LoggingLevel.Error);
            Crash();
        }
        path = "res://Prefabs/Encounters/";
        dir = DirAccess.Open(path);
        if (dir != null)
        {
            foreach(string fileName in dir.GetFiles())
            {
                if(fileName.Contains(".tscn"))
                {
                    string fn = fileName;
                    if(fn.Contains(".remap")) fn = fn.Replace(".remap", "");
                    Log($"Found Encounter file: {path + fn}");
                    PackedScene unit = (PackedScene)GD.Load(path + fn);
                    ENCOUNTERS_REGISTER[fileName.Substr(0, fileName.Find(".tscn")).ToLower()] = unit;
                }
            }
        }
        else
        {
            Log($"[ERROR] Unable to find Encounters folder! Can't proceed further.", LoggingLevel.Error);
            Crash();
        }
        path = "res://Prefabs/Rewards/";
        dir = DirAccess.Open(path);
        if (dir != null)
        {
            foreach(string fileName in dir.GetFiles())
            {
                if(fileName.Contains(".tscn"))
                {
                    string fn = fileName;
                    if(fn.Contains(".remap")) fn = fn.Replace(".remap", "");
                    Log($"Found Reward file: {path + fn}");
                    PackedScene unit = (PackedScene)GD.Load(path + fn);
                    REWARDS_REGISTER[fileName.Substr(0, fileName.Find(".tscn")).ToLower()] = unit;
                }
            }
        }
        else
        {
            Log($"[ERROR] Unable to find Rewards folder! Can't proceed further.", LoggingLevel.Error);
            Crash();
        }

        path = "res://Prefabs/Weapons/";
        dir = DirAccess.Open(path);
        if (dir != null)
        {
            foreach(string fileName in dir.GetFiles())
            {
                if(fileName.Contains(".tscn"))
                {
                    string fn = fileName;
                    if(fn.Contains(".remap")) fn = fn.Replace(".remap", "");
                    Log($"Found Weapon file: {path + fn}");
                    PackedScene unit = (PackedScene)GD.Load(path + fn);
                    WEAPONS_REGISTER[fileName.Substr(0, fileName.Find(".tscn")).ToLower()] = unit;
                }
            }
        }
        else
        {
            Log($"[ERROR] Unable to find Weapons folder! Can't proceed further.", LoggingLevel.Error);
            Crash();
        }
        path = "res://Prefabs/Abilities/";
        dir = DirAccess.Open(path);
        if (dir != null)
        {
            foreach(string fileName in dir.GetFiles())
            {
                if(fileName.Contains(".tscn"))
                {
                    string fn = fileName;
                    if(fn.Contains(".remap")) fn = fn.Replace(".remap", "");
                    Log($"Found Ability file: {path + fn}");
                    PackedScene unit = (PackedScene)GD.Load(path + fn);
                    ABILITIES_REGISTER[fileName.Substr(0, fileName.Find(".tscn")).ToLower()] = unit;
                }
            }
        }
        else
        {
            Log($"[ERROR] Unable to find Abilities folder! Can't proceed further.", LoggingLevel.Error);
            Crash();
        }
        path = "res://Scripts/Augments/";
        dir = DirAccess.Open(path);
        if(dir != null)
        {
            foreach(string fileName in dir.GetFiles())
            {
                if(fileName.Contains(".cs"))
                {
                    Log($"Found Augment file: {path + fileName}");
                    string className = fileName.Substr(0, fileName.Find(".cs"));
                    Type augmentType = Type.GetType(className);
                    if(augmentType != null)
                    {
                        AUGMENTS_REGISTER[className.ToLower()] = augmentType;
                    }
                    else
                    {
                        Log($"[ERROR] Unable to find Augment class: {className}", LoggingLevel.Error);
                    }
                }
            }
        }
        else
        {
            Log($"[ERROR] Unable to find Augments folder! Can't proceed further.", LoggingLevel.Error);
            Crash();
        }
    }

    public virtual void UNITS_COSTS_REGISTRATION() {
        foreach(string unitName in UNITS_REGISTER.Keys) {
            UNITS_COSTS_REGISTER[unitName] = new Dictionary<string, float>();
            foreach(string resourceType in Enum.GetNames(typeof(ResourceTypes))) {
                UNITS_COSTS_REGISTER[unitName][resourceType] = 0;
            }
        }
        foreach(string unitName in UNITS_COSTS_REGISTER.Keys) {
            string[] cost_names = Array.Empty<string>();
            float[]  cost_values = Array.Empty<float>();
            SceneState packed_unit = UNITS_REGISTER[unitName].GetState();
            for(int node_prop_idx = 0; node_prop_idx < packed_unit.GetNodePropertyCount(0); node_prop_idx++) {
                if(packed_unit.GetNodePropertyName(0, node_prop_idx) == "CostTypes") {
                    cost_names = (string[])packed_unit.GetNodePropertyValue(0, node_prop_idx);
                }
                if(packed_unit.GetNodePropertyName(0, node_prop_idx) == "CostAmounts") {
                    cost_values = (float[])packed_unit.GetNodePropertyValue(0, node_prop_idx);
                }
            }
            for(int i = 0; i < cost_names.Length; i++) {
                UNITS_COSTS_REGISTER[unitName][cost_names[i]] = cost_values[i];
            }
        }
    }
    public override void _Ready()
    {
        foreach(string resourceType in Enum.GetNames(typeof(ResourceTypes))) {
            Resources[resourceType] = 0;
        }
        Instance = this;
        PREFABS_REGISTRATION();
        UNITS_COSTS_REGISTRATION();
    }

    
}
