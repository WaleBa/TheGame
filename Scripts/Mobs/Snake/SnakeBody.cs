namespace GameE;

public partial class SnakeBody : Area2D
{
    public delegate void DeathEventHandler(SnakeBody cell);
    public event DeathEventHandler Death;

    protected int hp = 50;
	public Node2D Target = null;

    protected int Speed = 300;
    protected int DistanceBetweenCells = 20;
    public override void _Ready()
    {
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
