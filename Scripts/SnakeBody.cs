namespace GameE;

public partial class SnakeBody : Area2D
{
    public delegate void DeathEventHandler(SnakeBody myCell);
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
        if(IsInstanceValid(Target))
        {
            LookAt(Target.Position);
            if(Position.DistanceTo(Target.Position) > DistanceBetweenCells) 
                Position += Transform.X * (float)delta * Speed;
        }
    }
    protected virtual void  Hit(int damage)
    {
        hp -= damage;
        if(hp <= 0)
            Die();
    }

    protected virtual void Die()
    {
        if(IsInstanceValid(this) == true)
        {
            Death?.Invoke(this);//die after a while => await nie timer
            QueueFree();
        }
    }
}
