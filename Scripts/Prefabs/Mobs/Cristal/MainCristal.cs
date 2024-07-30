namespace GameE;
public partial class MainCristal : Cristal
{
	public new delegate void DeathEventHandler(Node2D me);
	public new event DeathEventHandler Death;
	Timer ubgradeTimer;
	Node2D Target, rootNode, rotationPointCristal;

	Random rand = new Random();
	Vector2 _velocity = new Vector2();
	List<Cristal> cristals = new List<Cristal>();
	(Node2D rotationPoint, float extra)[] rotationPoints;	
	Area2D HitBox;
	Queue<Vector2> NextCristalPosition;
	int radius;
	int Speed = 50;
	float rotationPointCritalEXTRA = 0;
	public int[] HpPerTier = {
        150,
		750,
		2700,
		8700,
		26850,
		81450,
		245400,
		737400,
		2213550,
		6642150
    };


	public override void _Ready()
	{
		//AddToGroup("Mobs");
		radius = 200 * Tier;
		GetNode<Sprite2D>("Sprite2D").Scale = new Vector2(1,1) * (float)Tier /2;
		HitBox = GetNode<Area2D>("HitBox");
		HitBox.GetNode<CollisionShape2D>("CollisionShape2D").Shape = new CircleShape2D();
		HitBox.Scale = new Vector2(1, 1) * (float)Tier;
		GetNode<CollisionShape2D>("CollisionShape2D").Scale = new Vector2(1,1) * (float)radius / 60 ;
		ubgradeTimer = GetNode<Timer>("upgrade");		
		ubgradeTimer.Timeout += () => CallDeferred("Ubgrade");
		rotationPointCristal = GetNode<Node2D>("cristal_rotation_marker");
		rootNode = GetTree().Root.GetNode<Node2D>("MainScene");
		Target = rootNode.GetNode<CharacterBody2D>("Player");

		NextCristalPosition = new Queue<Vector2>();
		int arraySize = 1 + Tier * 2;//8+
		rotationPoints = new (Node2D, float)[arraySize];
		HP = HpPerTier[Tier - 1];
		ubgradeTimer.Start();
		AddFloatingBullets();
	}
	public override void _PhysicsProcess(double delta)
	{
		for(int i = 0; i < rotationPoints.Length; i++)
			rotationPoints[i].Item1.Rotate(-(float)delta/3 + rotationPoints[i].Item2);
		rotationPointCristal.Rotate((float)delta/3 + rotationPointCritalEXTRA);

		_velocity = newDir(delta);
		_velocity = _velocity.Lerp(Vector2.Zero, 0.1f);
		Position += _velocity;
		        Godot.Collections.Array<Node2D> bodiez = HitBox.GetOverlappingBodies();
        for(int i = 0; i< bodiez.Count; i++)
        {
            if(bodiez[i] is Player player)
                player.Hit();
        }
	}

	Vector2 newDir(double delta)
    {
		Vector2 Dir = new Vector2(0,0);
		if(Position.DistanceTo(Target.Position) > radius)
        	Dir = (Target.Position - Position).Normalized();
        Godot.Collections.Array<Area2D> bodies = GetOverlappingAreas();
        for(int i = 0; i < bodies.Count; i++)
        {
            if(bodies[i] is not MainCristal)
                continue;
            Vector2 awayDir = (Position - bodies[i].GlobalPosition).Normalized();
            Dir += awayDir;
        }
        return Dir.Normalized() * Speed * (float)delta;
    }

	void Ubgrade()
	{
		if(IsInstanceValid(this) == false || cristals.Count >= Tier || Tier == 1)
			return;

		Cristal cris = Prefabs.Cristal.Instantiate<Cristal>();
		cris.Death += (Cristal cristal) => 
		{
			if(IsInstanceValid(this) == false)
				return;
			ubgradeTimer.Start();
			cristals.Remove(cristal);
			NextCristalPosition.Enqueue(cristal.Position);
		};
		cris.parentCristal = this;
		cris.Tier = Tier - 1;
		cristals.Add(cris);

		if(NextCristalPosition.Count != 0)
			cris.Position = NextCristalPosition.Dequeue();
		else
		{
			int offset = Tier * 50;
			float angle = 2 * Mathf.Pi / cristals.Count;
			float vec = rand.Next(0, 5);
			for(int i = 0; i < cristals.Count;i++)
			{
				cristals[i].Position = new Vector2(offset,0).Rotated(vec + angle * i);
			}
		}	
		rotationPointCristal.AddChild(cris);
	}
	void AddFloatingBullets()
	{
		if(IsInstanceValid(this) == false)
			return;

		int ArmCount = 2 + Tier;
		int BulletArmCount = 1 + Tier * 2;//8+
		float offsetFromCenter = 75 * Tier;
		float angle = 2 * Mathf.Pi / ArmCount;
		for(int i = 0; i < BulletArmCount;i++)
		{
			Node2D newRotationPoint = new Node2D();
			for(int k = 0; k < ArmCount; k++)
			{
				FloatingEvilBullet bull = Prefabs.FloatingEvilBullet.Instantiate<FloatingEvilBullet>();
				bull.timerOffset = i + 1;
				var offset = angle/6 * (i + 1);//3 gives fun straight lines // 12 desired 
				bull.Position = new Vector2(offsetFromCenter + 25 * (i + 1), 0).Rotated(-(angle * k + offset));//25->50
				newRotationPoint.AddChild(bull);
			}
			rotationPoints[i] = (newRotationPoint, 0.0f);
			AddChild(newRotationPoint);
		}
	}
	public override void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
    {
		GD.Print($"cristal hit {HP}");
		if(IsInstanceValid(this) == false)
			return;
		if(HP <= 0)
			return;
        HP -= damage;
		
        if(HP <= 0)
            CallDeferred("Die");
    }
	protected override void Die()
    {
		if(IsInstanceValid(this) == false)
			return;
		Death?.Invoke(this);
		QueueFree();
    }
}

