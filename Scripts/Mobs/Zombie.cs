namespace GameE;

public partial class Zombie : RigidBody2D
{
    PackedScene zombie = ResourceLoader.Load<PackedScene>("res://Scenes/Mobs/Zombie.tscn");

    CharacterBody2D Target;
    Node2D rootNode;
    Area2D contactArea;

    int Speed;
    int HP;
    int Tier = 3;
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

        float offset = 0.5f * Tier - 1;
        GetNode<CollisionShape2D>("CollisionShape2D").Scale = new Vector2(0.5f + offset,0.5f + offset);
        GetNode<Sprite2D>("Sprite2D").Scale = new Vector2(0.5f + offset,0.5f + offset);
        Mass = Tier;

        HP = HpPerTier[Tier - 1];
        Speed = 150;

        Timer timer = new()
        {
            Autostart = true,
            OneShot = true,
            WaitTime = 0.05
        };

        timer.Timeout += () => Visible = true;
        AddChild(timer);
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
                    Zombie z = zombie.Instantiate<Zombie>();
                    z.Position = Position + Dir[i];
                    z.Visible= false;
                    z.Tier = Tier - 1;
                    rootNode.AddChild(z);
                }
            }
            QueueFree();
        }
    }
}

