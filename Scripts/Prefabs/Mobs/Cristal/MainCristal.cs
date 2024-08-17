namespace GameE;

public partial class MainCristal : Cristal
{
	public new delegate void DeathEventHandler(Node2D me);
	public new event DeathEventHandler Death;

	List<Cristal> _cristals = new List<Cristal>();	

	Queue<Vector2> _nextCristalPosition = new Queue<Vector2>();

	Area2D _cristalCollisionBox;
	
	Marker2D _bulletRotationMarker;//for future use
	Marker2D _cristalRotationMarker;

	Timer _ubgradeTimer;

	Node2D _mainScene;
	
	Node2D _target;

	Vector2 _velocity = new Vector2();
	
	public float Radius;
	float _radius;

	int _speed = 50;

	float _cristalOffsetFromCentre;
	float _bulletOffsetFromCentre;
	float _bulletOffsetFromEachOther;
	int _armCount;
	int _bulletsPerArmCount;
	float _bulletOffsetFromEachOtherRotation;

	public override void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
    {
        _hp -= damage;

        if(_hp <= 0)
            CallDeferred("Die");
    }

	protected override void Die()
    {
		if(IsInstanceValid(this) == false)
			return;

		Death?.Invoke(this);
        ProcessMode =  ProcessModeEnum.Disabled;
        Visible = false;         
    }
	
	void SpawnCristal()
	{
		if(IsInstanceValid(this) == false || _cristals.Count >= (Tier + 3) || Tier == 1)
			return;

		Cristal cristal = Prefabs.Cristal.Instantiate<Cristal>();

		cristal.Tier = Tier - 1;

		cristal.Death += (Cristal cristal) => 
		{
			if(IsInstanceValid(this) == false)
				return;

			_nextCristalPosition.Enqueue(cristal.Position);
			_cristals.Remove(cristal);

			_ubgradeTimer.Start();
		};
		
		_cristals.Add(cristal);	

		if(_nextCristalPosition.Count != 0)
			cristal.Position = _nextCristalPosition.Dequeue();
		else
		{
			float angle = 2 * Mathf.Pi / _cristals.Count;
			
			for(int i = 0; i < _cristals.Count; i++)
				_cristals[i].Position = new Vector2(_cristalOffsetFromCentre, 0).Rotated(angle * i);
		}

		_cristalRotationMarker.AddChild(cristal);
	}

/*
	var offset = angle/6 * (i + 1);//3 gives fun straight lines // 12 desired 
	bullet.Position = new Vector2(_bulletOffsetFromCentre + _bulletOffsetFromEachOther * (i + 1), 0).Rotated(-(angle * k + offset));//25->50
*/
 	void SpawnFloatingBullets()
	{	
		float angle = 2 * Mathf.Pi / _armCount;
	
		for(int i = 0; i < _bulletsPerArmCount; i++)
		{
			for(int k = 0; k < _armCount; k++)
			{
				FloatingEvilBullet bullet = Prefabs.FloatingEvilBullet.Instantiate<FloatingEvilBullet>();

				bullet.SpawnTimerOffset = i + 1;
				bullet.Position = new Vector2(
								_bulletOffsetFromCentre + _bulletOffsetFromEachOther * (i + 1), 0)
								.Rotated(-(angle * k + _bulletOffsetFromEachOtherRotation  * i));//shouldn't propably be again offset from eachother
				
				_bulletRotationMarker.AddChild(bullet);
			}
		}
	}
	
	int  caseArena()
	{
		return (Position.DistanceTo(new Vector2(0,0)) >= Global.MAX_DISTANCE_FROM_CENTRE) ? 4 : 1;
	}

	Vector2 NewDirection()
    {
		Vector2 newDirection = new Vector2(0,0);

		if(Position.DistanceTo(_target.Position) > _radius)
        	newDirection = (_target.Position - Position).Normalized();

        Godot.Collections.Array<Area2D> bodies = _cristalCollisionBox.GetOverlappingAreas();
        for(int i = 0; i < bodies.Count; i++)
        {
            if(bodies[i].GetParent() is not MainCristal)
                continue;
        
            newDirection += (Position - bodies[i].GlobalPosition).Normalized();
        }
        return newDirection.Normalized() * caseArena();
    }

	public override void _Ready()
	{
		AddToGroup("Mobs");

		 _radius = Radius; //all those should be calculated properly

		_mainScene = GetTree().Root.GetNode<Node2D>("MainScene");
		_target = _mainScene.GetNode<CharacterBody2D>("Player");		
		_ubgradeTimer = GetNode<Timer>("ubgrade");		
		_bulletRotationMarker = GetNode<Marker2D>("bullet_rotation_marker");
		_cristalRotationMarker = GetNode<Marker2D>("cristal_rotation_marker");
		_cristalCollisionBox = GetNode<Area2D>("cristal_collision_box");

		GetNode<Sprite2D>("Sprite2D").Scale = new Vector2(2+ 0.5f * (Tier - 2), 2 + 0.5f * (Tier - 2));// * (float)Tier;// /2?
		GetNode<CollisionShape2D>("CollisionShape2D").Scale = new Vector2(2+ 0.5f * (Tier - 2), 2 + 0.5f * (Tier - 2));// = new Vector2(1, 1) * (float)Tier;
		//_cristalCollisionBox.Scale = new Vector2(1,1) * (float)Tier * 4;
		_cristalCollisionBox.Scale = new Vector2(1 + 0.5f * (Tier -1), 1 + 0.5f * (Tier - 1));

		_ubgradeTimer.Timeout += SpawnCristal;
		
		_hp = _hpPerTier[Tier - 1];
		//_bulletRotationPoints = new Marker2D[1 + Tier * 2];//8+
		float MCrad = 65 * (2 + 0.5f * (Tier -2));
		float Crad = 65 * (1 + 0.5f * (Tier - 1));
		//_radius = 200 * Tier;//all those should be calculated properly
		_cristalOffsetFromCentre = MCrad + Crad;//= 80 * Tier;
		_bulletOffsetFromCentre = MCrad + Crad * 2;// + 32.5f;//130 * Tier;//too long names
		_bulletOffsetFromEachOther = 96.5f;
		_bulletOffsetFromEachOtherRotation = 0.05f;
		_armCount = Tier + 5;
		_bulletsPerArmCount =  (int)((_radius - MCrad - 2 * Crad + 65 ) / 97.5f);//= 9 + Tier * 2;

		SpawnFloatingBullets();

		BodyEntered += (Node2D body) =>
        {
            if(body is Player player)
                player.Hit();
        };
	}

	public override void _PhysicsProcess(double delta)
	{	
		_bulletRotationMarker.Rotate(-(float)delta/3);// /3 good speed?
		_cristalRotationMarker.Rotate((float)delta/3);

		_velocity = NewDirection() * _speed * (float)delta;
		_velocity = _velocity.Lerp(Vector2.Zero, 0.1f);

		Position += _velocity;
	}

}

