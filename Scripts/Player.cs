namespace GameE;

public partial class Player : CharacterBody2D
{
	public delegate void DeathEventHandler(Node2D me);
	public event DeathEventHandler Death;

	const float SPEED = 100.0f;//not changable?
	int _hp = 20;
	Weapon _weapon;
	Timer timer;
	bool floa = false;
	public void LevelUp() => _weapon.LevelUp(); //need?

	public void Hit(int damage, bool flo)
	{
		if(floa == true)
			timer.WaitTime = 2.5f;
		if(timer.IsStopped() == false)
			return;
		_hp -= damage;
		if(flo == true)
		{
			floa = true;
			timer.WaitTime = 0.25f;
		}
		timer.Start();
		GetNode<ProgressBar>("ProgressBar").Value -= damage;
        if(_hp <= 0)
            CallDeferred("Die");
	}
	
	void Die()
	{
		Death?.Invoke(this);
		GetTree().ReloadCurrentScene();//future: calls event and mainscene reloads itself
	}

	Vector2 GetMovementInput()
	{
		switch(Global.CONTROLLER)
		{
			case true:
				return Input.GetVector(
								"MOVE_AXIS_LEFT", 
								"MOVE_AXIS_RIGHT", 
								"MOVE_AXIS_UP", 
								"MOVE_AXIS_DOWN");
			case false:
				return Input.GetVector(
								"MOVE_LEFT",
								"MOVE_RIGHT",
								"MOVE_UP",
								"MOVE_DOWN");
		}
	}

    public override void _Ready()
    {
		timer = GetNode<Timer>("Timer");
		_weapon = GetNode<Weapon>("Weapon");
		GetNode<ProgressBar>("ProgressBar").Value = 20;
    }

    public override void _PhysicsProcess(double delta)
	{
		Velocity += GetMovementInput() * SPEED;
		Velocity = Velocity.Lerp(Vector2.Zero, 0.1f);

		if(Position.DistanceTo(new Vector2(0,0)) > Global.MAX_DISTANCE_FROM_CENTRE * 3)
			Position = (Position - new Vector2(0,0)).Normalized() * Global.MAX_DISTANCE_FROM_CENTRE;
		
		MoveAndSlide();
	}
}
