using Godot;
using System;

public partial class Map : Node2D
{
	[Export] public string MapName = "";
	[Export] public string Region = "";
	[Export] public string MainUnitPool = "";
	[Export] public string NextMap = "";
	[Export] public Marker2D SpawnPoint = null;
	[Export] public float NorthernBoundary = 0;
	[Export] public float SouthernBoundary = 0;
	[Export] public float EasternBoundary = 0;
	[Export] public float WesternBoundary = 0;
}
