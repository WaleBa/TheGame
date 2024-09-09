namespace GameE;

public partial class GoodBullet : Area2D
{   
        public delegate void DeathEventHandler(Node2D me);//add mob type and tier
	public event DeathEventHandler Death;
    public int Speed { get; set; }
	public int Range { get; set; }
    public int Damage { get; set; }

    Vector2 _startingPosition;

    void Contact(Node2D body)
    {
        if(body.IsInGroup("Mobs"))
        {
            body.Call("Hit",1, 3, (body.Position - Position).Normalized());//recoil is only for zombies so they should calculate it 

            CallDeferred("Die");
        }
        else if(body.IsInGroup("Projectiles") && body is not GoodBullet)//just floating_evil_bullet
        {      
           // CallDeferred("Die");
        }
        else if(body.IsInGroup("HitBox"))
        {
            body.GetParent().Call("Hit", Damage, 3, (body.Position - Position).Normalized());
            CallDeferred("Die");
        }
    }
    void Die()
    {
        if(!IsInstanceValid(this))
            return;

        Death?.Invoke(this); 
        ProcessMode =  ProcessModeEnum.Disabled;
        Visible = false;     
    }

    public override void _Ready()
    {  
        ProcessMode =  ProcessModeEnum.Disabled;
        Visible = false; 
        
        AddToGroup("Projectiles");

        Scale = new Vector2(0.5f, 0.5f);

        AreaEntered += Contact;
        BodyEntered += Contact;
    }
    
    public void Activate()//still are on main scene
    {
        ProcessMode =  ProcessModeEnum.Pausable;
        Visible = true; 
        
        _startingPosition = Position;
    }

    public override void _PhysicsProcess(double delta)
    { 
        Position += Transform.X * Speed * (float)delta;

        if(Position.DistanceTo(_startingPosition) > Range)
            Die();
    }
}