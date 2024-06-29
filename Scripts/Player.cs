namespace GameE;
public partial class Player : CharacterBody2D
{
	const float Speed = 50.0f;

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
		return Input.GetVector("move_left", "move_right", "move_up", "move_down");
	}
	
	public void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
	{
		GD.Print("got damage");
	}
}
