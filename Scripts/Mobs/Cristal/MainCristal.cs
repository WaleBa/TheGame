namespace GameE;
public partial class MainCristal : Cristal
{
	public new delegate void DeathEventHandler(int Tier);
	public new event DeathEventHandler Death;
	Timer ubgradeTimer;
	Node2D Target, rootNode, rotationPointCristal;

	Random rand = new Random();
	Vector2 _velocity = new Vector2();
	List<Cristal> cristals = new List<Cristal>();
	(Node2D rotationPoint, float extra)[] rotationPoints;	
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
		AddToGroup("Mobs");

		GetNode<Sprite2D>("Sprite2D").Scale = new Vector2(1,1) * (float)Tier /2;
		GetNode<CollisionShape2D>("CollisionShape2D").Scale = new Vector2(1,1) * (float)Tier/2;
		
		ubgradeTimer = GetNode<Timer>("UbgradeTimer");		ubgradeTimer.Timeout += () => CallDeferred("Ubgrade");
		rotationPointCristal = GetNode<Node2D>("rotationPointCristal");
		rootNode = GetTree().Root.GetNode<Node2D>("MainScene");
		Target = rootNode.GetNode<CharacterBody2D>("Player");

		NextCristalPosition = new Queue<Vector2>();
		int arraySize = 1 + Tier * 2;
		rotationPoints = new (Node2D, float)[arraySize];
		radius = 200 * Tier;
		HP = HpPerTier[Tier - 1];
		ubgradeTimer.Start();
		AddFloatingBullets();
	}
	public override void _PhysicsProcess(double delta)
	{
		for(int i = 0; i < rotationPoints.Length; i++)
			rotationPoints[i].Item1.Rotate(-(float)delta/3 + rotationPoints[i].Item2);
		rotationPointCristal.Rotate((float)delta/3 + rotationPointCritalEXTRA);

		if(Position.DistanceTo(Target.Position) > radius)
			_velocity = (Target.Position - Position).Normalized() * Speed * (float)delta;
		_velocity = _velocity.Lerp(Vector2.Zero, 0.1f);
		Position += _velocity;
	}

	void Ubgrade()
	{
		if(IsInstanceValid(this) == false || cristals.Count >= Tier)
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
		int BulletArmCount = 1 + Tier * 2;
		float offsetFromCenter = 75 * Tier;
		float angle = 2 * Mathf.Pi / ArmCount;
		for(int i = 0; i < BulletArmCount;i++)
		{
			Node2D newRotationPoint = new Node2D();
			for(int k = 0; k < ArmCount; k++)
			{
				FloatingEvilBullet bull = Prefabs.FloatingEvilBullet.Instantiate<FloatingEvilBullet>();
				bull.timerOffset = i + 1;
				var offset = angle/6 * (i + 1);
				bull.Position = new Vector2(offsetFromCenter + 50 * (i + 1), 0).Rotated(-(angle * k + offset));
				newRotationPoint.AddChild(bull);
			}
			rotationPoints[i] = (newRotationPoint, 0.0f);
			AddChild(newRotationPoint);
		}
	}

	protected override void Die()
    {
		if(IsInstanceValid(this) == false)
			return;
		Death?.Invoke(Tier);
		QueueFree();
    }
}

