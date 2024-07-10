namespace GameE;
public partial class Player : CharacterBody2D
{
	const float Speed = 50.0f;
    public bool Controller = false;
	public override void _PhysicsProcess(double delta)
	{
		Velocity += GetMovementInput() * Speed;
		Velocity = Velocity.Lerp(Vector2.Zero, 0.1f);

		if(Position.DistanceTo(new Vector2(0,0)) > 1250)
			Position = (Position - new Vector2(0,0)).Normalized() * 1250;
		
		MoveAndSlide();
	}

	Vector2 GetMovementInput()
	{
		if(Controller == true)
			return Input.GetVector("move_left_joypad", "move_right_joypad", "move_up_joypad", "move_down_joypad");
		return Input.GetVector("move_left", "move_right", "move_up", "move_down");
	}
	
	public void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
	{
		GD.Print("death!");
		GetTree().ReloadCurrentScene();
	}
	public void lvl()
	{
		GD.Print("lvl up");
		GetNode<weapon>("weapon").AutomaticCooldown.WaitTime =GetNode<weapon>("weapon").AutomaticCooldown.WaitTime /2;
		GetNode<weapon>("weapon").ShootgunBulletCount = (byte)(GetNode<weapon>("weapon").ShootgunBulletCount + 2);
		GetNode<weapon>("weapon").ShootgunPower = (byte)(GetNode<weapon>("weapon").ShootgunPower * 2);
		GetNode<weapon>("weapon").AutomaticPower = (byte)(GetNode<weapon>("weapon").AutomaticPower * 2);
	}	
}
