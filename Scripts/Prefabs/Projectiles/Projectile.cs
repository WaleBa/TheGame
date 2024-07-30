namespace GameE;
public partial class Projectile : Area2D
{
    public int Speed;
	public int Range;
    public int Damage;

    public Vector2 StartingPos;
    public override void _Ready()
    {
        AddToGroup("Projectiles");
        StartingPos = Position;

        AreaEntered += Contact;
        BodyEntered += Contact;
    }

    public virtual void Contact(Node2D body){}

    public override void _PhysicsProcess(double delta)
    { 
        GlobalPosition += Transform.X * Speed * (float)delta;
        if(Position.DistanceTo(StartingPos) > Range)
            QueueFree();
    }
}
