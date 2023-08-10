namespace GameE;

public partial class weapon : Node2D
{
	PackedScene bullet = ResourceLoader.Load<PackedScene>("res://Scenes/Bullet.tscn");

	Marker2D bulletPlace;
	Timer cooldown;
	Node rootNode;//COOLDOWN FOR ONLY SHOTGUN

	byte TimeTicks = 0;
	byte ShootgunBulletCount = 10;
	public override void _Ready()
	{
		bulletPlace = GetNode<Marker2D>("bulletPlace");
		cooldown = GetNode<Timer>("cooldown");
		rootNode = GetTree().Root.GetNode<Node2D>("MainScene");
	}

	public override void _PhysicsProcess(double delta)
	{	
		LookAt(GetGlobalMousePosition());
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
		bullet bulle = bullet.Instantiate<bullet>();

		bulle.Set("position", bulletPlace.GlobalPosition);
        bulle.Set("rotation", rotation);

		rootNode.AddChild(bulle);

	}
}
