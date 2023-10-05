namespace GameE;

public partial class Zombie : CharacterBody2D
{
	PackedScene zombie = ResourceLoader.Load<PackedScene>("res://Scenes/Mobs/Zombie.tscn");
	
	CharacterBody2D Target;
	Node2D rootNode;
 	Area2D area;

	Vector2? recoilVector = null;
	int? recoil = null;

	int Speed;
	int hp = 50;
	int Tier = 4;
	Vector2[] Dir = {new Vector2(0,1), new Vector2(1,1), new Vector2(-1,-1)}; 

	public override void _Ready()
	{
		area = GetNode<Area2D>("Area2D");
		rootNode = GetTree().Root.GetNode<Node2D>("MainScene");
		Target = GetTree().Root.GetNode<Node2D>("MainScene").GetNode<CharacterBody2D>("Player");
		
		float offset = 0.5f * (Tier - 1); 
		Scale = new Vector2(1 + offset,1 + offset);
		hp = 50 * Tier;
		Speed = 150 / Tier;

        Timer timer = new()
        {
            Autostart = true,
            OneShot = true,
            WaitTime = 0.3
        };
		timer.Timeout += () => Visible = true;
        AddChild(timer);
	}
	public override void _PhysicsProcess(double delta)
	{
		Vector2 vel = new Vector2(0,0);
		
		vel += newDir()  * Speed * (float)delta;
		if(recoil != null && recoilVector != null)
		{
			vel += (Vector2)recoilVector *(int)recoil * (float)delta;
			recoil = recoil /2;
			if(recoil <= 0)
			{
				recoil = null;
				recoilVector = null;
			}
		}
		MoveAndCollide(vel);
	}
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
			Vector2 awayDir = (Position - bodies[i].GlobalPosition).Normalized();
			Dir += awayDir;
		}
		return Dir.Normalized();
	}
	public void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
    {
		if(hp <= 0)
			return;
        hp -= damage;
		
		recoilVector = recoilVectorGiven;
		recoil = recoilPower * 300;
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
					z.Speed = Speed * 2;
					z.Visible= false;
					z.Tier = Tier - 1;
					rootNode.AddChild(z);
				}
			}
            QueueFree();
        }
    }
}
