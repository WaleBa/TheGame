using Godot;
using System;

public partial class ScoreNode : Node2D
{
	int Range = 80;
	Vector2 _startingPosition;
	public string Score;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_startingPosition = Position;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	    public override void _PhysicsProcess(double delta)
    { 
        Position += Transform.Y * 100 * (float)delta;

        if(Position.DistanceTo(_startingPosition) > Range)
            QueueFree();
    }
}
