namespace GameE;

public partial class SnakeCell : Area2D
{
	public delegate void DeathEventHandler(Node2D me);
	public event DeathEventHandler Death;
    //public DeathEventHandler handler = Die();///check

    public Node2D Target { get; set; }
    public int HP { get; set; }
    public int Speed { get; set; }
     int _startHP;
    public float DistanceBetweenCells;//not constant -> if larger can't shoot easly
    	    Sprite2D _sprite;
        Color _finalColor = new Color(0.5f, 0, 0.25f, 1);
    Color _startColor = new Color(0.5f, 0, 1, 1);
    public virtual void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
    {
        HP -= damage;
        float offset = 1 - ((float)HP / (float)_startHP);
        _sprite.Modulate = _startColor.Lerp(_finalColor, offset);
        if(HP <= 0)
            CallDeferred("Die", this);
    }

    protected void RaiseDeathEvent() => Death?.Invoke(this);

    protected virtual void Die(Node2D me)
    {
        if(IsInstanceValid(this) == false)
            return;
                    Death?.Invoke(this); 
       // ((SnakeCell)Target).Death -= Die;
        ProcessMode =  ProcessModeEnum.Disabled;
        Visible = false;     
    }

    public void DirtySet(Node2D target, Vector2 position, int hp)
    {
        ProcessMode =  ProcessModeEnum.Pausable;
        Visible = true; 
        
        _startHP = hp;
        Target = target;
        Position = position;
        HP = hp;
        GD.Print("Drity set");
        ((SnakeCell)Target).Death += Die;//someCell => Die();        
    }

    public override void _Ready()
    {    
                _sprite = GetNode<Sprite2D>("Sprite2D");
        AddToGroup("Mobs");
//start hp dirty
        _startHP = HP;
                ((SnakeCell)Target).Death += Die;
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
