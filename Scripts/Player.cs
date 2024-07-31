namespace GameE;

public partial class Player : CharacterBody2D
{
	const float SPEED = 50.0f;//not changable?
	
	Weapon _weapon;

	public void LevelUp() => _weapon.LevelUp(); //need?

	public void Hit() => Die();
	
	void Die() => GetTree().ReloadCurrentScene();//future: calls event and mainscene reloads itself
	
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
		_weapon = GetNode<Weapon>("Weapon");
    }

    public override void _PhysicsProcess(double delta)
	{
		Velocity += GetMovementInput() * SPEED;
		Velocity = Velocity.Lerp(Vector2.Zero, 0.1f);

		if(Position.DistanceTo(new Vector2(0,0)) > Global.MAX_DISTANCE_FROM_CENTRE)
			Position = (Position - new Vector2(0,0)).Normalized() * Global.MAX_DISTANCE_FROM_CENTRE;
		
		MoveAndSlide();
	}
}
