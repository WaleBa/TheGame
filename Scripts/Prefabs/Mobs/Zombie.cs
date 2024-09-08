namespace GameE;

public partial class Zombie : RigidBody2D
{
    public delegate void DeathEventHandler(Node2D me);//add mob type and tier
	public event DeathEventHandler Death;

    public int Tier { get; set; }

    static int[] _hpPerTier = { 9, 27, 150, 200, 250, 300, 350, 400, 450, 500 };

    static Vector2[] _spreadDirection = {
                                new Vector2(0,1),
                                new Vector2(1,1), 
                                new Vector2(-1,-1) };//nie dokladne

    Node2D _mainScene;
    Area2D _collisionBox;//less hp - stasiu more bullets
    Node2D _target;
    Sprite2D _sprite;
    int _hp; 
    int _speed = 600;

    MobFabric Fabricate;

    Color _finalColor = new Color(1, 0.5f, 0, 1);
    Color _startColor = new Color(0, 0.5f, 0, 1);
    
    public void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
    {
        if(!IsInstanceValid(this))
            return;

        _hp -= damage;
        float offset = 1 - ((float)_hp / (float)_hpPerTier[Tier -1]);
        _sprite.Modulate = _startColor.Lerp(_finalColor, offset);

        ApplyCentralImpulse(recoilVectorGiven * recoilPower * 500);

        if(_hp <= 0)
            CallDeferred("Die");
    }

    void Die()//differednt name "death"?
    {
        if(!IsInstanceValid(this))
            return;//when?
        
        if(Tier > 1 == true)
        {
                            int t = Tier;
        GD.Print($"tt : {t}");
            GD.Print("start");
            for(int i = 0; i < 3;i++)
            {
                Zombie zombie = Prefabs.Zombie.Instantiate<Zombie>();
                zombie.Tier = Tier - 1;
                zombie.Position = Position + _spreadDirection[i];
                _mainScene.AddChild(zombie);
            }
            GD.Print("end");
        }
        
        Death?.Invoke(this);
        ProcessMode =  ProcessModeEnum.Disabled;
        Visible = false; 
    }

    Vector2 NewDirection()
    {
        Vector2 newDirection = (_target.Position - Position).Normalized();
        
        Godot.Collections.Array<Area2D> bodies = _collisionBox.GetOverlappingAreas();
        
        for(int i = 0;i< bodies.Count; i++)
        {
            if(bodies[i] is not SnakeCell)
                continue;
            
            newDirection += (Position - bodies[i].GlobalPosition).Normalized();
        }

        return newDirection.Normalized();
    }

    public void Activate()
    {
        GD.Print($"act: {Tier}");
        ProcessMode =  ProcessModeEnum.Pausable;
        Visible = true; 

        Mass = Tier;//need mass?
        _hp = _hpPerTier[Tier - 1];

        _sprite.Modulate = _startColor;

        _collisionBox.GetNode<CollisionShape2D>("CollisionShape2D").Scale  = new Vector2(1,1) * Tier; //snake collision box
        GetNode<CollisionShape2D>("CollisionShape2D").Scale  = new Vector2(1,1) * Tier; 
        GetNode<Sprite2D>("Sprite2D").Scale  = new Vector2(0.2f, 0.2f) * Tier; 
    }

    public override void _Ready()
    {//no tier usage here
        
        Fabricate = GetTree().Root.GetNode<MobFabric>("MobFabric");
        
        AddToGroup("Mobs");

        _mainScene = GetTree().Root.GetNode<Node2D>("MainScene");
        _target = _mainScene.GetNode<Player>("Player");
        _collisionBox = GetNode<Area2D>("collision_box");
        _sprite = GetNode<Sprite2D>("Sprite2D");

        _speed = 300;
                Mass = Tier;//need mass?
        _hp = 9 * Tier;//_hpPerTier[Tier - 1] / 2;
        _sprite.Modulate = _startColor;

        _collisionBox.GetNode<CollisionShape2D>("CollisionShape2D").Scale  = new Vector2(1,1) * Tier; //snake collision box
        GetNode<CollisionShape2D>("CollisionShape2D").Scale  = new Vector2(1,1) * Tier; 
        GetNode<Sprite2D>("Sprite2D").Scale  = new Vector2(0.2f, 0.2f) * Tier; 
        _collisionBox.BodyEntered += (Node2D body) =>
        {
            if(body is Player player)
                player.Hit(Tier * 5, false);
        };
    }

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)//change place
    {
        ApplyCentralForce(NewDirection() * _speed * 10);
    }
}
