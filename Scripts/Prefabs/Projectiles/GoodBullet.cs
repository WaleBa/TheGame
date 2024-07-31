namespace GameE;

public partial class GoodBullet : Area2D
{   
    public int Speed { get; set; }
	public int Range { get; set; }
    public int Damage { get; set; }

    Vector2 _startingPosition;

    void Contact(Node2D body)
    {
        if(body.IsInGroup("Mobs"))
        {
            if(IsInstanceValid(body) == false)  
                return;

            body.Call("Hit",Damage, 3, (body.Position - Position).Normalized());

            if(IsInstanceValid(this))
                QueueFree();
        }
        else if(body.IsInGroup("Projectiles") && body is not GoodBullet)//just floating_evil_bullet
        {      
            if(IsInstanceValid(this) == true)
                QueueFree();
        }
    }

    public override void _Ready()
    {  
        AddToGroup("Projectiles");

        _startingPosition = Position;

        AreaEntered += Contact;
        BodyEntered += Contact;
    }

    public override void _PhysicsProcess(double delta)
    { 
        Position += Transform.X * Speed * (float)delta;

        if(Position.DistanceTo(_startingPosition) > Range)
            QueueFree();
    }
}