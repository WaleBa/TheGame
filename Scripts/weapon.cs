namespace GameE;

public partial class weapon : Node2D
{
	PackedScene bullet = ResourceLoader.Load<PackedScene>("res://Scenes/Projectiles/GoodBullet.tscn");

	Marker2D bulletPlace;
	Timer ShootgunCooldown, AutomaticCooldown;
	Node rootNode;//COOLDOWN FOR ONLY SHOTGUN
	Player parent;
	int TimesShoot = 0;

	byte TimeTicks = 0;
	byte ShootgunBulletCount = 10;
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
			if(TimeTicks <= 15) ShootRequest(0);
			TimeTicks = 0;
			return;
		}
		if(Input.IsActionPressed("Shoot"))
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
					Shoot((float)rotation, 20);
        		}

				AutomaticCooldown.WaitTime = 0.3f;
				ShootgunCooldown.WaitTime =1f;
				AutomaticCooldown.Start();
				ShootgunCooldown.Start();
				break;
			}
			case 1: //automatic power:10
			{
				if(AutomaticCooldown.IsStopped() == false)
					return;

				Shoot(Rotation);

				AutomaticCooldown.WaitTime = 0.04f;
				ShootgunCooldown.WaitTime = 0.15f;
				AutomaticCooldown.Start();
				ShootgunCooldown.Start();
				break;
			}
		}
	}
	void Shoot(float rotation, int power = 10)
	{
		TimesShoot++;
		GoodBullet bulle = bullet.Instantiate<GoodBullet>();

		bulle.Position = bulletPlace.GlobalPosition;
        bulle.Rotation = rotation;
		
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
