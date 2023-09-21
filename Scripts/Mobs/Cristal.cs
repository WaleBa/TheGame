namespace GameE;
public partial class Cristal : Area2D
{
	public delegate void DeathEventHandler(Cristal me);
	public delegate void HittedEventHandler(Vector2 direction, int power);
	public event DeathEventHandler Death;
	public event HittedEventHandler Hitted;
	PackedScene cristal = ResourceLoader.Load<PackedScene>("res://Scenes/Mobs/Cristal.tscn");
	PackedScene floatingBullet = ResourceLoader.Load<PackedScene>("res://Scenes/Projectiles/FloatingEvilBullet.tscn");
	PackedScene bullet = ResourceLoader.Load<PackedScene>("res://Scenes/Projectiles/EvilBullet.tscn");

	Cristal parentCristal;
	Timer ubgradeTimer, cooldownTimer;
	Node2D rotationPoint, rotationPointCristal, Target, rootNode;

	Random rand = new Random();
	Vector2 _velocity = new Vector2();
	List<Cristal> cristals = new List<Cristal>();
	Vector2? recoilVector = null;
	int? recoil = null;

	int HP = 150;
	int radius = 500;
	int Speed = 30;
	int Tier = 3;
	bool HasParent = false;
	int odleglosc = 250;//jf

	public override void _Ready()
	{
		AddToGroup("Mobs");
		ubgradeTimer = GetNode<Timer>("UbgradeTimer");
		ubgradeTimer.Timeout += () => CallDeferred("Ubgrade");

		cooldownTimer = GetNode<Timer>("Cooldown");
		rotationPointCristal = GetNode<Node2D>("rotationPointCristal");
		rotationPoint = GetNode<Node2D>("rotationPoint");
		rootNode = GetTree().Root.GetNode<Node2D>("MainScene");
		Target = rootNode.GetNode<CharacterBody2D>("Player");

		if(HasParent == true)
		{
			parentCristal = GetParent().GetParent() as Cristal;
			return;
		}
		NoCristalParent();
	}
	public override void _PhysicsProcess(double delta)
	{
		if(HasParent == true)
		{
			if(GlobalPosition.Y < parentCristal.GlobalPosition.Y)
				ZIndex = -1;
			else 
				ZIndex = 0;
			return;
		}
		rotationPoint.Rotate(-(float)delta/2);
		rotationPointCristal.Rotate((float)delta/2);

		if(Position.DistanceTo(Target.Position) > radius)
		{
			_velocity = newDir() * Speed * (float)delta;
		}
		if(recoil != null && recoilVector != null)
		{
			_velocity += (Vector2)recoilVector *(int)recoil/3 * (float)delta;
			recoil = recoil /2;
			if(recoil <= 0)
			{
				recoil = null;
				recoilVector = null;
			}
		}
		_velocity = _velocity.Lerp(Vector2.Zero, 0.1f);
		Position += _velocity;
	}
	public void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
    {
		if(IsInstanceValid(this) == false)
			return;
		Hitted?.Invoke(recoilVectorGiven, recoilPower);
		if(HP <= 0)
			return;
        HP -= damage;
		
		recoilVector = recoilVectorGiven;
		recoil = recoilPower * 100;
        if(HP <= 0)
            CallDeferred("Die");
		ShootRequest();
    }

	void Die()
    {
		if(IsInstanceValid(this) == false)
			return;
		Death?.Invoke(this);
		if(HasParent == false)
		{
			var criz = rotationPointCristal.GetChildren();
			for(int i = 0; i < criz.Count;i++)
			{
				Cristal c = criz[i] as Cristal;
				c.Reparent(rootNode, true);
				c.HasParent = false;
				c.NoCristalParent();
				c.Hit(0, 100, Position.DirectionTo(c.GlobalPosition));
			}
		}
		QueueFree();
    }
	void Ubgrade()
	{
		if(IsInstanceValid(this) == false)
			return;
		if(cristals.Count >= Tier)
			return;
		Cristal cris = cristal.Instantiate<Cristal>();
		cris.Hitted += (Vector2 recoilVectorGiven, int recoilPower) =>
		{
			recoilVector = recoilVectorGiven;
			recoil = recoilPower;

			ShootRequest();
		};
		cris.Death += (Cristal cristal) => 
		{
			if(IsInstanceValid(this) == false)
				return;
			ubgradeTimer.Start();
			cristals.Remove(cristal);
			PlaceCristals();
		};
		cris.Tier = Tier - 1;
		cris.HasParent = true;
		cristals.Add(cris);
		rotationPointCristal.AddChild(cris);
		PlaceCristals();
	}
	void ShootRequest()
	{
		if(IsInstanceValid(this) == false)
			return;
		if(HasParent == true)
			return;
		if(cooldownTimer.IsStopped() == false)
			return;
		cooldownTimer.Start();	
		CallDeferred("Shoot");	
	}
	async void Shoot()
	{
		if(IsInstanceValid(this) == false)
			return;
		int ArmCount = 2 + Tier;
		int BulletArmCount = 1 + Tier * 4;

		float bulletRotation = 0;
		float angle = 2 * Mathf.Pi / ArmCount;

		for(int i = 0; i < BulletArmCount; i++)
		{
			for(int z = 0; z < ArmCount; z++)
			{
				EvilBullet bull = bullet.Instantiate<EvilBullet>();
				bull.Position = Position;
				bull.Rotate(angle * z + bulletRotation);
				rootNode.AddChild(bull);
			}
			bulletRotation += 0.1f;
			await Task.Delay(50);
		}
	}
	void PlaceCristals()
	{
		float angle = 2 * Mathf.Pi / cristals.Count;
		float vec = rand.Next(0, 5);
		for(int i = 0; i < cristals.Count;i++)
		{
			cristals[i].Position = new Vector2(50,0).Rotated(vec + angle * i);
		}
	}
	void AddFloatingBullets()
	{
		if(IsInstanceValid(this) == false)
			return;
		int ArmCount = 2 + Tier;
		int BulletArmCount = 1 + Tier * 2;
		float angle = 2 * Mathf.Pi / ArmCount;
		for(int i = 1; i <= BulletArmCount;i++)
		{
			for(int k = 0; k < ArmCount; k++)
			{
				FloatingEvilBullet bull = floatingBullet.Instantiate<FloatingEvilBullet>();
				var offset = angle/6 * i;
				bull.Position = new Vector2(odleglosc + 50 * i, 0).Rotated(-(angle * k + offset));
				rotationPoint.AddChild(bull);
			}
		}
	}
	void NoCristalParent()
	{	
		if(IsInstanceValid(this) == false)
			return;
		cooldownTimer.Start();
		ubgradeTimer.Start();
		AddFloatingBullets();
	}
	Vector2 newDir()
	{
		if(HasOverlappingAreas() == false)
			return (Target.Position - Position).Normalized();

		Vector2 ToPlayer = (Target.Position - Position).Normalized();
		Vector2 Dir = ToPlayer;
		Godot.Collections.Array<Area2D> bodies = GetOverlappingAreas();
		for(int i = 0;i< bodies.Count; i++)
        {
            if(bodies[i] is Cristal cristal)
			{
				if(cristal.HasParent == true)
					continue;
				Vector2 awayDir = (Position - bodies[i].GlobalPosition).Normalized();
				Dir += awayDir;
			}
		}
		return Dir.Normalized();
	}
}

