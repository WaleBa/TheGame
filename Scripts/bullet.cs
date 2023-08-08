using Godot;
using System;

public partial class bullet : Area2D
{
	int Speed = 500;//const
	int Power = 10;
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += Transform.X * Speed * (float)delta;
	}
}
