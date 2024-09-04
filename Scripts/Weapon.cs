using System.Runtime.Intrinsics.Arm;

namespace GameE;

public partial class Weapon : Node2D
{
	enum WeaponType
	{
		Shootgun,
		Automatic
	}
	MobFabric Fabricate;
	Marker2D _weaponMarker;
	Marker2D _bulletMarker;
	//add comments
	MainScene _mainScene;
float cooldown = 0.25f;//try make this a property getter
	Timer _shootgunCooldownTimer;
	Timer _automaticCooldownTimer;//cooldown only for shotgun?
	
	public int Level = 1;
	bool nextArm = false;
	byte _timeTicks = 0; //check if even need this field //byte?
	byte _shootgunBulletCount = 5;
	byte _shootgunPower = 10;//need for extra?
	byte _automaticPower = 5;
int arm = 1;
	float _currentJoystickAngle;

	public void LevelUp()
	{
		GD.Print("lvlup");
		Level++;

		cooldown = 0.25f;//small values encourage aiming and make early stages cool
		for(int i = 0; i < Level; i++)
		{
			cooldown *= 0.5f;
		}
	}
		//_automaticCooldownTimer.WaitTime = _automaticCooldownTimer.WaitTime - 0.025f;
		//_shootgunBulletCount += 2;
		//_shootgunPower += 10;
		//_automaticPower += 5;
//

	void PreparingForShoot()
	{
		if(	
			Input.IsActionJustReleased("SHOOT_AXIS_LEFT") ||
			Input.IsActionJustReleased("SHOOT_AXIS_RIGHT") || 
			Input.IsActionJustReleased("SHOOT_AXIS_UP") ||
		 	Input.IsActionJustReleased("SHOOT_AXIS_DOWN") ||
			Input.IsActionJustReleased("SHOOT") )
		{
			if(Input.GetVector(
					"SHOOT_AXIS_LEFT",
					"SHOOT_AXIS_RIGHT", 
					"SHOOT_AXIS_UP", 
					"SHOOT_AXIS_DOWN" ) != new Vector2(0,0)) return;

			if(_timeTicks <= 15) 
				Shoot(WeaponType.Shootgun);

			_timeTicks = 0;
		}
		else if(
			Input.IsActionPressed("SHOOT_AXIS_LEFT") ||
			Input.IsActionPressed("SHOOT_AXIS_RIGHT") || 
		 	Input.IsActionPressed("SHOOT_AXIS_UP") || 
		 	Input.IsActionPressed("SHOOT_AXIS_DOWN") ||
			Input.IsActionPressed("SHOOT") )
		{
			if(_timeTicks > 15) 
				Shoot(WeaponType.Automatic);

			_timeTicks++;
		}
	}

	void Shoot(WeaponType weaponType)
	{
		switch(weaponType)
		{
			case WeaponType.Shootgun: 
			{
				if(_shootgunCooldownTimer.IsStopped() == false)
					return;
				double angle = 0.4 / (2 + 1);
        		for (int i = 1; i <= 2; i++)
        		{
					double rotation = GlobalRotation + (-0.2 + (i * angle));
					SpawnBullet((float)rotation, _shootgunPower);
        		}
				//SetCooldowns(1f, 0.1f);
				break;
			}
			case WeaponType.Automatic:
			{
				if(_automaticCooldownTimer.IsStopped() == false)
					return;
									double angle = 0.1 * arm / (Level + 1);
        		for (int i = 1; i <= arm; i++)
        		{
					double rotation = GlobalRotation + (-0.1 * arm + (i * angle));
				SpawnBullet((float)rotation, _automaticPower);// * Level);        		
				 }
				SetCooldowns(0.5f, cooldown);//0.05f);			
				break;
			}
		}
	}

	void SpawnBullet(float rotation, int power)
	{
		GoodBullet bullet = Fabricate.GoodBullet();

		bullet.Position = _bulletMarker.GlobalPosition;
        bullet.Rotation = rotation;
		bullet.Damage = power; 
        bullet.Speed = 3000;
        bullet.Range =3000;// 1500;//can be changable
		bullet.Activate();
		//_mainScene.AddChild(bullet);
	}

	float GetRotation()
	{
		switch(Global.CONTROLLER)
		{
			case true:

				Vector2 input = Input.GetVector(
								"SHOOT_AXIS_LEFT",
								"SHOOT_AXIS_RIGHT",
								"SHOOT_AXIS_UP", 
								"SHOOT_AXIS_DOWN" );
				if(input == Vector2.Zero)
					return Rotation;// ?:
				else 
				{
					Shoot(WeaponType.Automatic);

					return input.Angle();
				}
			case false:
				return (GetGlobalMousePosition() - GlobalPosition).Angle();
		}
	}
	
	void SetCooldowns(float shotgunWaitTime, float automaticWaitTime)
	{
		_shootgunCooldownTimer.WaitTime = shotgunWaitTime;
		_automaticCooldownTimer.WaitTime = automaticWaitTime;

		_automaticCooldownTimer.Start();
		_shootgunCooldownTimer.Start();
	}
	
	public override void _Ready()
	{
		Fabricate = GetTree().Root.GetNode<MobFabric>("MobFabric");
		_mainScene = GetTree().Root.GetNode<MainScene>("MainScene");
		_bulletMarker = GetNode<Marker2D>("bullet_marker");
		_shootgunCooldownTimer = GetNode<Timer>("shootgun_cooldown");
		_automaticCooldownTimer = GetNode<Timer>("automatic_cooldown");
		_automaticCooldownTimer.WaitTime = 0.25f;
	}

	public override void _PhysicsProcess(double delta)
	{
		Rotation = GetRotation(); //player(outside source) could decide on where to shoot

		//ZIndex = Rotation < new Vector2(1,0).Angle() ? -1 : 0;
		
		//PreparingForShoot();
	}

}
