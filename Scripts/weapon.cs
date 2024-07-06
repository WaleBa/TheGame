namespace GameE;

public partial class weapon : Node2D
{
	Marker2D bulletPlace;
	public Timer ShootgunCooldown, AutomaticCooldown;
	Node rootNode;//COOLDOWN FOR ONLY SHOTGUN
	Player parent;
	int TimesShoot = 0;

	byte TimeTicks = 0;
	public byte ShootgunBulletCount = 5;
	public byte ShootgunPower = 10;
	public byte AutomaticPower = 10; 
	float oldVec;
	public override void _Ready()
	{
		bulletPlace = GetNode<Marker2D>("bulletPlace");
		ShootgunCooldown = GetNode<Timer>("ShootgunCooldown");
		AutomaticCooldown = GetNode<Timer>("AutomaticCooldown");
		rootNode = GetTree().Root.GetNode<Node2D>("MainScene");
		parent = rootNode.GetNode<Player>("Player");
		LoadSave();	
	}

	public override void _PhysicsProcess(double delta)
	{
		Rotation = GetAxis();//(GetGlobalMousePosition() - GlobalPosition).Angle();
		if(Rotation < new Vector2(1,0).Angle()) 
				ZIndex = -1;
			else	
				ZIndex = 0;
		PreparingForShoot();
	}
	float GetAxis()
	{
		Vector2 zer = new Vector2(0,0);
		Vector2 newVec = Input.GetVector("shoot_axis_left", "shoot_axis_right", "shoot_axis_up", "shoot_axis_down");
		if(newVec == zer) 
		{
			ShootugnAction();
			return oldVec;
		}
		else
		{
			oldVec = newVec.Angle();
			return newVec.Angle();
		}
	}
	void ShootugnAction()
	{
				if(
			Input.IsActionJustReleased("shoot_axis_left") ||
			Input.IsActionJustReleased("shoot_axis_right") || 
		 	Input.IsActionJustReleased("shoot_axis_up") || 
		 	Input.IsActionJustReleased("shoot_axis_down")
		 )
		{
		if(TimeTicks <= 15) ShootRequest(0);
		TimeTicks = 0;
		}
	}
	void PreparingForShoot()
	{
		if(
			Input.IsActionPressed("shoot_axis_left") ||
			Input.IsActionPressed("shoot_axis_right") || 
		 	Input.IsActionPressed("shoot_axis_up") || 
		 	Input.IsActionPressed("shoot_axis_down")
		 )
		{
			if(TimeTicks > 15) ShootRequest(1);
			TimeTicks++;
			return;
		}
	}
	void ShootRequest(int type)
	{
		switch(type)
		{
			case 0: //shootgun power:20
			{
				if(ShootgunCooldown.IsStopped() == false)
					return;

				double angle = 0.4 / (ShootgunBulletCount + 1);
        		for (int i = 1; i <= ShootgunBulletCount; i++)
        		{
					double rotation = GlobalRotation + (-0.2 + (i * angle));
					Shoot((float)rotation, ShootgunPower);
        		}

				AutomaticCooldown.WaitTime = 0.1f;
				ShootgunCooldown.WaitTime =1f;
				AutomaticCooldown.Start();
				ShootgunCooldown.Start();
				break;
			}
			case 1: //automatic power:10
			{
				if(AutomaticCooldown.IsStopped() == false)
					return;

				Shoot(Rotation, AutomaticPower);

				AutomaticCooldown.WaitTime = 0.1f;
				ShootgunCooldown.WaitTime = 0.5f;
				AutomaticCooldown.Start();
				ShootgunCooldown.Start();
				break;
			}
		}
	}
	void Shoot(float rotation, int power)
	{
		TimesShoot++;
		GoodBullet bulle = Prefabs.GoodBullet.Instantiate<GoodBullet>();

		bulle.Position = bulletPlace.GlobalPosition;
        bulle.Rotation = rotation;
		bulle.Damage = power;
		rootNode.AddChild(bulle);
	}

	public Godot.Collections.Dictionary<string, Variant> Save()
	{
		return new Godot.Collections.Dictionary<string, Variant>()
		{
			{"TimesShoot", TimesShoot}	
		};
	} 

	public override void _ExitTree()
	{
		using var SaveGame = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Write);
		Godot.Collections.Dictionary<string, Variant> weaponData = Save();
		string jsonString = Json.Stringify(weaponData);
		SaveGame.StoreLine(jsonString);
		GD.Print("saved");
	}
	public void LoadSave()
	{
		if(!FileAccess.FileExists("user://savegame.save"))
			return;
		using var saveGame = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Read);

		while(saveGame.GetPosition() < saveGame.GetLength())
		{
			string jsonString = saveGame.GetLine();
			Json json = new Json();
			var ParseResult = json.Parse(jsonString);
			if(ParseResult != Error.Ok)
				continue;
			Godot.Collections.Dictionary<string, Variant> nodeData = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);
			foreach(var (key, value) in nodeData)
			{
				if(key == "TimesShoot")	
					GD.Print($"LOAD times shoot: {value}");
			}
		}
	} 
}
