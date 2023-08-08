using Godot;
using System;

public partial class player : CharacterBody2D
{
	public const float Speed = 50.0f;

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
}
