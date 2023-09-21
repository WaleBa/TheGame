namespace GameE;

public partial class Zombie : CharacterBody2D
{
	PackedScene zombie = ResourceLoader.Load<PackedScene>("res://Scenes/Mobs/Zombie.tscn");
	
	CharacterBody2D Target;
	Node2D rootNode;
 	Area2D area;

	Vector2? recoilVector = null;
	int? recoil = null;

	int Speed = 150;
	int hp = 50;
	int Tier = 3;
	Vector2[] Dir = {new Vector2(0,1), new Vector2(1,1), new Vector2(-1,-1)}; 

	public override void _Ready()
	{
		area = GetNode<Area2D>("Area2D");
		rootNode = GetTree().Root.GetNode<Node2D>("MainScene");
		Target = GetTree().Root.GetNode<Node2D>("MainScene").GetNode<CharacterBody2D>("Player");

		Scale = new Vector2(1,1) * (float)Tier;
		hp = 50 * Tier;
		Speed = Speed / Tier;

		Timer timer = new();//NEED change
        timer.Autostart = true;
        timer.OneShot = true;
		timer.WaitTime = 0.3;
		AddChild(timer);
        timer.Timeout += () => Visible = true;
	}
	public override void _PhysicsProcess(double delta)
	{
		Vector2 vel = new Vector2(0,0);
		if(Position.DistanceTo(Target.Position) > 50)//change for tiers
		{
			vel += newDir()  * Speed * (float)delta;
		}
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
