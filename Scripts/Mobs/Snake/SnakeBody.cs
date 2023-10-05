namespace GameE;

public partial class SnakeBody : Area2D
{
    public delegate void DeathEventHandler(SnakeBody cell);
    public event DeathEventHandler Death;

    protected int hp = 50;
	public Node2D Target = null;
    public Sprite2D Sprite;
    public CollisionShape2D CollisionShape;
    protected int Speed = 300;
    public float DistanceBetweenCells = 30;
    public override void _Ready()
    {
        Sprite = GetNode<Sprite2D>("Sprite2D");
        CollisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        if(Target is SnakeBody snakeCell)
            snakeCell.Death +=  someCell => Die();
    }
    public override void _PhysicsProcess(double delta)
    {
        LookAt(Target.Position);
        if(Position.DistanceTo(Target.Position) > DistanceBetweenCells) 
            Position += Transform.X * (float)delta * Speed;
    }

    protected virtual void  Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
    {
        hp -= damage;
        if(hp <= 0)
            Die();
    }

    protected virtual void Die()
    {
        Death?.Invoke(this);
        if(IsInstanceValid(this) == true)//die after a while => await nie timer
            QueueFree();
    }
}
