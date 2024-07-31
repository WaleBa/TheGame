namespace GameE;

public partial class Zombie : RigidBody2D
{
    public delegate void DeathEventHandler(Node2D me);//add mob type and tier
	public event DeathEventHandler Death;

    public Node2D Target { get; set; }
    public int Tier { get; set; }
    public int HP { get; set; }
    public int Speed { get; set; }//somehow works

    static int[] _hpPerTier = { 50, 100, 150, 200, 250, 300, 350, 400, 450, 500 };

    static Vector2[] _spreadDirection = {
                                new Vector2(0,1),
                                new Vector2(1,1), 
                                new Vector2(-1,-1) };//nie dokladne

    Node2D _mainScene;
    Area2D _collisionBox;
    
    public void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
    {
        HP -= damage;

        ApplyCentralImpulse(recoilVectorGiven * recoilPower * 500);

        if(HP <= 0)
            CallDeferred("Die");
    }

    void Die()//differednt name "death"?
    {
        if(IsInstanceValid(this) == false || Tier == 1)
            return;
        
        for(int i = 0; i < 3;i++)
        {
            Zombie zombie = Prefabs.Zombie.Instantiate<Zombie>();
                    
            zombie.Position = Position + _spreadDirection[i];
            zombie.Tier = Tier - 1;
            
            _mainScene.AddChild(zombie);
        }
        
        //Death?.Invoke(this, MobType.Zombie, Tier);
        ProcessMode =  ProcessModeEnum.Disabled;
        Visible = false;            //QueueFree
    }

    Vector2 NewDirection()
    {
        Vector2 newDirection = (Target.Position - Position).Normalized();
        
        Godot.Collections.Array<Area2D> bodies = _collisionBox.GetOverlappingAreas();
        
        for(int i = 0;i< bodies.Count; i++)
        {
            if(bodies[i] is not SnakeCell)
                continue;
            
            newDirection += (Position - bodies[i].GlobalPosition).Normalized();
        }

        return newDirection.Normalized();
    }

    public override void _Ready()
    {
        AddToGroup("Mobs");
        Tier = 3;
        _mainScene = GetTree().Root.GetNode<Node2D>("MainScene");
        Target = _mainScene.GetNode<Player>("Player");
        _collisionBox = GetNode<Area2D>("collision_box");
        
        _collisionBox.GetNode<CollisionShape2D>("CollisionShape2D").Scale *= Tier;
        GetNode<CollisionShape2D>("CollisionShape2D").Scale *= Tier;
        GetNode<Sprite2D>("Sprite2D").Scale *= Tier;
        
        Mass = Tier;
        HP = _hpPerTier[Tier - 1];
        Speed = 150;

        _collisionBox.BodyEntered += (Node2D body) =>
        {
            if(body is Player player)
                player.Hit();
        };
    }

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        ApplyCentralForce(NewDirection() * Speed * 10);
    }
}
