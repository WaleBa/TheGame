namespace GameE;

public partial class Prefabs
{
    public static PackedScene ScoreLabel {get; } = Load("ScoreNode");
    public static PackedScene MainCristal {get; } = Load("Mobs/Cristal/MainCristal");
    public static PackedScene Cristal {get; } = Load("Mobs/Cristal/Cristal");
    public static PackedScene SnakeHead {get; } = Load("Mobs/Snake/SnakeHead");
    public static PackedScene SnakeCell {get; } = Load("Mobs/Snake/SnakeCell");
    public static PackedScene Zombie {get; } = Load("Mobs/Zombie");
    
    public static PackedScene GoodBullet {get; } = Load("Projectiles/GoodBullet");
    public static PackedScene FloatingEvilBullet {get; } = Load("Projectiles/FloatingEvilBullet");

    static PackedScene Load(string path) =>
        ResourceLoader.Load<PackedScene>($"res://Scenes/Prefabs/{path}.tscn");

    public Queue<Node2D> zombiePool;
}