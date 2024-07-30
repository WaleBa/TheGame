namespace GameE;
public partial class FloatingEvilBullet : EvilBullet
{
    Timer _respawnTimer;
    Sprite2D sprite;
    CollisionShape2D collisionShape;
    public int timerOffset;
    public override void _Ready()
    {
        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        base._Ready();
        _respawnTimer = GetNode<Timer>("respawn");
        _respawnTimer.WaitTime += timerOffset;
        sprite = GetNode<Sprite2D>("Sprite2D");
        _respawnTimer.Timeout += () => 
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
                player.Hit();
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
        _respawnTimer.Start();
    }
}
