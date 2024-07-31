namespace GameE;

public partial class SnakeCell : Area2D
{
	public delegate void DeathEventHandler(Node2D me);
	public event DeathEventHandler Death;

    public Node2D Target { get; set; }
    public int HP { get; set; }
    public int Speed { get; set; }
    
    public int DistanceBetweenCells;//not constant -> if larger can't shoot easly

    public virtual void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
    {
        HP -= damage;

        if(HP <= 0)
            Die();
    }

    protected virtual void Die()
    {
        if(IsInstanceValid(this) == false)
            return;

        Death?.Invoke(this);
        ProcessMode =  ProcessModeEnum.Disabled;
        Visible = false;         
    }

    public override void _Ready()
    {    
        AddToGroup("Mobs");

        ((SnakeCell)Target).Death +=  someCell => Die();

        BodyEntered += (Node2D body) =>
        {
            if(body is Player player)
                player.Hit();
        };
    }

    public override void _PhysicsProcess(double delta)
    {
        LookAt(Target.Position);
        
        if(Position.DistanceTo(Target.Position) > DistanceBetweenCells) 
            Position += Transform.X * (float)delta * Speed;
    }
}
