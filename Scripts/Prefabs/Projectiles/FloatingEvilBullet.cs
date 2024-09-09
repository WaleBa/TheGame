
namespace GameE;
public partial class FloatingEvilBullet : Area2D
{
        public delegate void DeathEventHandler(Node2D me);//add mob type and tier
	public event DeathEventHandler Death;
    public int SpawnTimerOffset;
    
    Player Target;
    Node2D _mainScene;

    Timer _vanishTimer;
    CollisionShape2D _collisionShape;
    Sprite2D _sprite;

    void Contact(Node2D body)
    {            
        if(body is Player player)
        {
            CallDeferred("Vanish");
        }
        //else if(body is GoodBullet)
        //    CallDeferred("Vanish");
    }
    
    void Vanish()
    {//have !visible one call methiod type
        _collisionShape.Disabled = true;
        _sprite.Visible = false;
        _vanishTimer.Start();
    }

    public override void _Ready()
    {
        AddToGroup("Projectiles");

		_mainScene = GetTree().Root.GetNode<Node2D>("MainScene");
        Target = _mainScene.GetNode<Player>("Player");
        _sprite = GetNode<Sprite2D>("Sprite2D");
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");//naming conventin in editor
        _vanishTimer = GetNode<Timer>("vanish_cooldown");
        
        Scale = new Vector2(0.5f, 0.5f);

        AreaEntered += Contact;
        BodyEntered += Contact;

        _vanishTimer.Timeout += () => 
        {
            if(Position.DistanceTo(Target.Position) <= 130)
            {
                _vanishTimer.WaitTime = 2.0f;
                _vanishTimer.Start();
                return;
            }
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