namespace GameE;
public partial class FloatingEvilBullet : EvilBullet
{
    Timer timer;
    Sprite2D sprite;
    CollisionShape2D collisionShape;
    public int timerOffset;
    public override void _Ready()
    {
        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        base._Ready();
        timer = GetNode<Timer>("Timer");
        timer.WaitTime += timerOffset;
        sprite = GetNode<Sprite2D>("Sprite2D");
        timer.Timeout += () => 
        {
            collisionShape.Disabled = false;
            sprite.Visible = true;
        };
        Vanish();
    }
    public override void Contact(Node2D body)
    {
        if(collisionShape.Disabled == false)
        {
            if(body is Player player)
            {
                player.Hit(Damage, 3, (player.Position - Position).Normalized());
                CallDeferred("Vanish");
            }
            else if(body is GoodBullet)
                CallDeferred("Vanish");
        }
    }
    public override void _PhysicsProcess(double delta)
    {

    }
    
    private void Vanish()
    {
        collisionShape.Disabled = true;
        sprite.Visible = false;
        timer.Start();
    }
}
