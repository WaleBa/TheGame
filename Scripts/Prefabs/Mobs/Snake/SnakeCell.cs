namespace GameE;

public partial class SnakeCell : Area2D
{
	public delegate void DeathEventHandler(Node2D me);
	public event DeathEventHandler Death;
    //public DeathEventHandler handler = Die();///check

    public Node2D Target { get; set; }
    public int HP { get; set; }
    public int Speed { get; set; }
    
    public float DistanceBetweenCells;//not constant -> if larger can't shoot easly

    public virtual void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
    {
        HP -= damage;

        if(HP <= 0)
            CallDeferred("Die", this);
    }

    protected virtual void Die(Node2D me)
    {
        if(IsInstanceValid(this) == false)
            return;

        ((SnakeCell)Target).Death -= Die;
        ProcessMode =  ProcessModeEnum.Disabled;
        Visible = false; 
        Death?.Invoke(this);     
    }

    public void DirtySet(Node2D target, Vector2 position, int hp)
    {
        ProcessMode =  ProcessModeEnum.Pausable;
        Visible = true; 
        
        Target = target;
        Position = position;
        HP = hp;
        ((SnakeCell)Target).Death += Die;//someCell => Die();        
    }

    public override void _Ready()
    {    
        AddToGroup("Mobs");

        BodyEntered += (Node2D body) =>
        {
            if(body is Player player)
                player.Hit(15, false);
        };
    }

    public override void _PhysicsProcess(double delta)
    {
        LookAt(Target.Position);
        
        if(Position.DistanceTo(Target.Position) > DistanceBetweenCells) 
            Position += Transform.X * (float)delta * Speed;
    }
}
