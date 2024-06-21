namespace GameE;

public partial class Zombie : RigidBody2D
{
        PackedScene zombie = ResourceLoader.Load<PackedScene>("res://Scenes/Mobs/Zombie.tscn");

        CharacterBody2D Target;
        Node2D rootNode;
        Area2D area;

        Vector2? recoilVector = null;
        int? recoil = null;

        int Speed;
        int hp = 50;
        int Tier = 3;
        Vector2[] Dir = {new Vector2(0,1), new Vector2(1,1), new Vector2(-1,-1)};

    public override void _Ready()
        {
                area = GetNode<Area2D>("Area2D");
                rootNode = GetTree().Root.GetNode<Node2D>("MainScene");
                Target = GetTree().Root.GetNode<Node2D>("MainScene").GetNode<CharacterBody2D>("Player");

                float offset = 0.5f * (Tier - 1);
                area.Scale = new Vector2(1 + offset,1 + offset);
                GetNode<CollisionShape2D>("CollisionShape2D").Scale = new Vector2(0.5f + offset,0.5f + offset);
                GetNode<Sprite2D>("Sprite2D").Scale = new Vector2(0.5f + offset,0.5f + offset);
                Mass = Tier;

                hp = 50 * Tier;
                Speed = 200;

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

/*
    public override void _PhysicsProcess(double delta)
        {
                Vector2 vel = newDir() * Speed *100 * (float)delta;
                ApplyCentralForce(vel);
        }

*/
        Vector2 newDir()
        {
                if(area.HasOverlappingAreas() == false)
                        return (Target.Position - Position).Normalized();

                Vector2 ToPlayer = (Target.Position - Position).Normalized();
                Vector2 Dir = ToPlayer;
                Godot.Collections.Array<Area2D> bodies = area.GetOverlappingAreas();
                for(int i = 0;i< bodies.Count; i++)
        {
            if(bodies[i] is not SnakeBody)
                                continue;
                        Vector2 awayDir = (Position - bodies[i].Position).Normalized();
                        Dir = awayDir;
                }
                return Dir.Normalized();
        }
        public void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
    {
                if(hp <= 0)
                        return;
        hp -= damage;

                ApplyCentralImpulse(recoilVectorGiven * recoilPower * 500);
        if(hp <= 0)
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

