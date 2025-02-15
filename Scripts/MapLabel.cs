using Godot;
using System;

public partial class MapLabel : Node2D
{
    Label MapName;
    Label Region;
    int Frame = 0;
    int FramesVisible = 300;
    float ModulatePerStep = 1f / 256f;
    public MapLabel(string mapName, string region)
    {
        MapName = new Label();
        Region = new Label();
        MapName.Text = mapName;
        Region.Text = region;
        LabelSettings mapNameSettings = new LabelSettings();
        mapNameSettings.FontSize = 48;
        MapName.LabelSettings = mapNameSettings;
        LabelSettings regionSettings = new LabelSettings();
        regionSettings.FontSize = 24;
        Region.LabelSettings = regionSettings;
    }

    public override void _Ready()
    {
        World.Instance.GetNode<CanvasLayer>("../UI").AddChild(MapName);
        World.Instance.GetNode<CanvasLayer>("../UI").AddChild(Region);
        MapName.Position = GetViewportRect().Size / 2 - MapName.GetRect().Size / 2 - new Vector2(0, GetViewportRect().Size.Y / 4);
        Region.Position = GetViewportRect().Size / 2 - Region.GetRect().Size / 2 + new Vector2(0, MapName.GetRect().Size.Y + 4) - new Vector2(0, GetViewportRect().Size.Y / 4);
    }

    public override void _Process(double delta)
    {
        Frame++;
        if (Frame > FramesVisible)
        {
            MapName.Modulate = new Color(1, 1, 1, MapName.Modulate.A - ModulatePerStep);
            Region.Modulate = new Color(1, 1, 1, Region.Modulate.A - ModulatePerStep);
        }
        if(MapName.Modulate.A <= 0)
        {
            MapName.QueueFree();
            Region.QueueFree();
            QueueFree();
        }
        
    }

    public void OnCameraZoom(Vector2 zoom) {
        MapName.Scale = new Vector2(1/zoom.X, 1/zoom.Y);
        Region.Scale = new Vector2(1/zoom.X, 1/zoom.Y);
        MapName.Position = new Vector2(0, -GetViewportRect().Size.Y / zoom.X / 4) - MapName.GetRect().Size / 2;
        Region.Position = new Vector2(0, MapName.GetRect().Size.Y + 4 - GetViewportRect().Size.Y / zoom.X / 4) - Region.GetRect().Size / 2;
    }
}
