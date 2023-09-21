namespace GameE;

public partial class weapon : Node2D
{
	PackedScene bullet = ResourceLoader.Load<PackedScene>("res://Scenes/Projectiles/GoodBullet.tscn");

	Marker2D bulletPlace;
	Timer cooldown;
	Node rootNode;//COOLDOWN FOR ONLY SHOTGUN
	Player parent;

	byte TimeTicks = 0;
	byte ShootgunBulletCount = 10;
	public override void _Ready()
	{
		bulletPlace = GetNode<Marker2D>("bulletPlace");
		cooldown = GetNode<Timer>("cooldown");
		rootNode = GetTree().Root.GetNode<Node2D>("MainScene");
		parent = rootNode.GetNode<Player>("Player");
	}

	public override void _PhysicsProcess(double delta)
	{
		Rotation = (GetGlobalMousePosition() - GlobalPosition).Angle();
		if(Rotation < new Vector2(1,0).Angle())
				ZIndex = -1;
			else	
				ZIndex = 0;
		PreparingForShoot();
	}
	void PreparingForShoot()
	{
		if(Input.IsActionJustReleased("Shoot"))
		{
			if(TimeTicks <= 90) ShootRequest(0);
			TimeTicks = 0;
			return;
		}
		if(Input.IsActionPressed("Shoot") && TimeTicks > 90)
		{
			ShootRequest(1);
			return;
		}
		TimeTicks++;
	}
	void ShootRequest(int type)
	{
		if(cooldown.IsStopped() == false)
			return;

		switch(type)
		{
			case 0: //shootgun power:20
			{
				double angle = 0.4 / (ShootgunBulletCount + 1);
        		for (int i = 1; i <= ShootgunBulletCount; i++)
        		{
					double rotation = GlobalRotation + (-0.2 + (i * angle));
					Shoot((float)rotation, 20);
        		}

				cooldown.WaitTime = 2f;
				cooldown.Start();

				break;
			}
			case 1: //automatic power:10
			{
				Shoot(Rotation);

				cooldown.WaitTime = 0.1f;
				cooldown.Start();

				break;
			}
		}
	}
	void Shoot(float rotation, int power = 10)
	{
		GoodBullet bulle = bullet.Instantiate<GoodBullet>();

		bulle.Position = bulletPlace.GlobalPosition;
        bulle.Rotation = rotation;
		
		rootNode.AddChild(bulle);
	}
}
