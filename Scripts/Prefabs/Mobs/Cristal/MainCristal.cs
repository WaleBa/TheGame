namespace GameE;

public partial class MainCristal : RigidBody2D
{
	public delegate void DeathEventHandler(Node2D me);
	public event DeathEventHandler Death;

    public int Tier { get; set; }

	protected static int[] _hpPerTier = { 150, 750, 2700, 8700, 26850, 81450, 245400, 737400, 2213550, 6642150 };

    protected int _hp;

	List<Cristal> _cristals = new List<Cristal>();	//bullet at edge overllapoign
//no need for next crista pos -> as with floatings vanish()
	Queue<Vector2> _nextCristalPosition = new Queue<Vector2>();//too brutal rigid bosies

	CollisionShape2D _cristalCollisionBox;//check if scaling should only work on whole node
	
	Marker2D _bulletRotationMarker;//for future use
	Marker2D _cristalRotationMarker;

	Timer _ubgradeTimer;

	Node2D _mainScene;
	
	Node2D _target;

	Vector2 _velocity = new Vector2();
	    Sprite2D _sprite;
	public float Radius;
	float _radius;

	int _speed = 50;

	float _cristalOffsetFromCentre;
	float _bulletOffsetFromCentre;
	float _bulletOffsetFromEachOther;
	int _armCount;
	int _bulletsPerArmCount;
	float _bulletOffsetFromEachOtherRotation;



    Color _finalColor = new Color(1, 1, 1, 1);
    Color _startColor = new Color(1, 0, 1, 1);

	public void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
    {
        _hp -= damage;

		float offset = 1 - ((float)_hp / (float)_hpPerTier[Tier -1]);
        _sprite.Modulate = _startColor.Lerp(_finalColor, offset);

        if(_hp <= 0)
            CallDeferred("Die");
    }

	protected void Die()
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

	Vector2 NewDirection()
    {
		Vector2 newDirection = new Vector2(0,0);

		if(Position.DistanceTo(_target.Position) > _radius)
        	newDirection = (_target.Position - Position).Normalized();

        return newDirection.Normalized();
    }

	public override void _Ready()
	{
		GD.Print("ready");
		AddToGroup("Mobs");
		
		 _radius = Radius; //all those should be calculated properly
        _sprite = GetNode<Sprite2D>("Sprite2D");
		_mainScene = GetTree().Root.GetNode<Node2D>("MainScene");
		_target = _mainScene.GetNode<CharacterBody2D>("Player");		
		_ubgradeTimer = GetNode<Timer>("ubgrade");		
		_bulletRotationMarker = GetNode<Marker2D>("bullet_rotation_marker");
		_cristalRotationMarker = GetNode<Marker2D>("cristal_rotation_marker");
		_cristalCollisionBox = GetNode<CollisionShape2D>("CollisionShape2D");

		GetNode<Sprite2D>("Sprite2D").Scale *=  2 + 0.5f * (Tier - 2);// * (float)Tier;// /2?
		GetNode<Area2D>("hit_box").GetNode<CollisionShape2D>("CollisionShape2D").Scale *= 2+ 0.5f * (Tier - 2);// = new Vector2(1, 1) * (float)Tier;
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
		GetNode<Sprite2D>("Sprite2D").Scale *=  2 + 0.5f * (Tier - 2);// * (float)Tier;// /2?
		GetNode<Area2D>("hit_box").GetNode<CollisionShape2D>("CollisionShape2D").Scale *= 2+ 0.5f * (Tier - 2);// = new Vector2(1, 1) * (float)Tier;
		//_cristalCollisionBox.Scale = new Vector2(1,1) * (float)Tier * 4;
		_cristalCollisionBox.Scale = new Vector2(1 + 0.5f * (Tier -1), 1 + 0.5f * (Tier - 1));

		SpawnFloatingBullets();

		GetNode<Area2D>("hit_box").BodyEntered += (Node2D body) =>
        {
            if(body is Player player)
                player.Hit(20, false);
        };
	}

	public override void _PhysicsProcess(double delta)
	{	
		_bulletRotationMarker.Rotate(-(float)delta/3);// /3 good speed?
		_cristalRotationMarker.Rotate((float)delta/3);
	}

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)//change place
    {
        ApplyCentralForce(NewDirection() * _speed * 10);
    }
}

