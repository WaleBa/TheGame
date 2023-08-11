namespace GameE;
public partial class bullet : Area2D
{
	int Speed = 500;
	Vector2 StartingPos;
	public int Range = 700;
	public override void _Ready()
	{
		StartingPos = Position;

		AreaEntered += Contact;
		BodyEntered += Contact;
	}

	void Contact(Node2D body)
	{
		if(body.IsInGroup("Mobs"))
		{
			body.Call("Hit", 10);
			if(IsInstanceValid(this) == true)
				QueueFree();
		}
	}

	public override void _PhysicsProcess(double delta)
	{ 
		GlobalPosition += Transform.X * Speed * (float)delta;
		if(Position.DistanceTo(StartingPos) > Range)
			QueueFree();
	}
}
