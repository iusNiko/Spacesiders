using Godot;
using System;

public partial class Vendors : Control
{
    public static Vendors Instance;
    public Vector2 LastVendorPosition = new Vector2(0, 0);

    Vendors() {
        Instance = this;
    }
}