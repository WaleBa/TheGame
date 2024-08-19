
namespace GameE;
public partial class FloatingEvilBullet : Area2D
{
    public int SpawnTimerOffset;
    
    Timer _vanishTimer;
    CollisionShape2D _collisionShape;
    Sprite2D _sprite;

    void Contact(Node2D body)
    {            
        if(body is Player player)
        {
            CallDeferred("Vanish");
        }
        else if(body is GoodBullet)
            CallDeferred("Vanish");
    }
    
    void Vanish()
    {
        _collisionShape.Disabled = true;
        _sprite.Visible = false;
        _vanishTimer.Start();
    }

    public override void _Ready()
    {
        AddToGroup("Projectiles");

        _sprite = GetNode<Sprite2D>("Sprite2D");
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");//naming conventin in editor
        _vanishTimer = GetNode<Timer>("vanish_cooldown");
        
        Scale = new Vector2(0.5f, 0.5f);

        AreaEntered += Contact;
        BodyEntered += Contact;

        _vanishTimer.Timeout += () => 
        {
            _vanishTimer.WaitTime = 5.0f;

            _collisionShape.Disabled = false;
            _sprite.Visible = true;
        };

        _vanishTimer.WaitTime += SpawnTimerOffset;
        Vanish();

        BodyEntered += (Node2D body) =>
        {
            if(body is Player player)
                player.Hit(1, true);
        };
    }
}