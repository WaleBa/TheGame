namespace GameE;
public partial class Player : CharacterBody2D
{
	const float Speed = 50.0f;

	public override void _PhysicsProcess(double delta)
	{
		Velocity += GetMovementInput() * Speed;
		Velocity = Velocity.Lerp(Vector2.Zero, 0.1f);
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
