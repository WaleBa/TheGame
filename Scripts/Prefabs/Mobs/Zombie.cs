namespace GameE;

public partial class Zombie : RigidBody2D
{
    public delegate void DeathEventHandler(Node2D me);
    public event DeathEventHandler Death;
    CharacterBody2D Target;
    Node2D rootNode;
    Area2D contactArea;

    int Speed;
    int HP;
    public int Tier;
    Vector2[] Dir = {new Vector2(0,1), new Vector2(1,1), new Vector2(-1,-1)};

    public int[] HpPerTier = {
        50,
        100,
        150,
        200,
        250,
        300,
        350,
        400,
        450,
        500
    };
    /*
            50,
        250,
        900,
        2900,
        8950,
        27150,
        81800,
        245800,
        737850,
        2214050
        */

    public override void _Ready()
    {
        rootNode = GetTree().Root.GetNode<Node2D>("MainScene");
        Target = GetTree().Root.GetNode<Node2D>("MainScene").GetNode<CharacterBody2D>("Player");
        contactArea = GetNode<Area2D>("Area2D");
        contactArea.GetNode<CollisionShape2D>("CollisionShape2D").Scale *= Tier;
        GetNode<CollisionShape2D>("CollisionShape2D").Scale *= Tier;
        GetNode<Sprite2D>("Sprite2D").Scale *= Tier;
        Mass = Tier;

        HP = HpPerTier[Tier - 1];
        Speed = 150;
    }

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        Vector2 vel = newDir() * Speed * 10;
        ApplyCentralForce(vel);
    }

    Vector2 newDir()
    {
        Vector2 ToPlayer = (Target.Position - Position).Normalized();
        Vector2 Dir = ToPlayer;
        Godot.Collections.Array<Area2D> bodies = contactArea.GetOverlappingAreas();
        for(int i = 0;i< bodies.Count; i++)
        {
            if(bodies[i] is not SnakeBody)
                continue;
            Vector2 awayDir = (Position - bodies[i].GlobalPosition).Normalized();
            Dir += awayDir;
        }
        Godot.Collections.Array<Node2D> bodiez = contactArea.GetOverlappingBodies();
        for(int i = 0; i< bodiez.Count; i++)
        {
            if(bodiez[i] is Player player)
                player.Hit(1, 3, (player.Position - Position).Normalized());
        }

        return Dir.Normalized();
    }

        
    public void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
    {
        if(HP <= 0)
            return;
        HP -= damage;
        ApplyCentralImpulse(recoilVectorGiven * recoilPower * 500);

        if(HP <= 0)
            CallDeferred("Die");
    }

    void Die()
    {
        if(IsInstanceValid(this) == true)
        {
            if(Tier > 1)
            {
                for(int i = 0; i < 3;i++)
                {
                    Zombie z = Prefabs.Zombie.Instantiate<Zombie>();
                    z.Position = Position + Dir[i];
                    z.Tier = Tier - 1;
                    rootNode.AddChild(z);
                }
            }
        
            Death?.Invoke(this);
            ProcessMode = ProcessModeEnum.Disabled;
            Visible = false;
            //QueueFree();//important to be at the end
        }
    }
}

