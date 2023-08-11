using Godot;
using System;

public partial class Zombie : CharacterBody2D
{
	PackedScene zombie = ResourceLoader.Load<PackedScene>("res://Scenes/Zombie.tscn");
	Vector2[] Dir = {new Vector2(0,1), new Vector2(1,1), new Vector2(-1,-1)}; 
	CharacterBody2D Target;
	Node2D rootNode;
	int Speed = 100;
	int hp = 50;
	int Tier = 3;
	public override void _Ready()
	{
		Timer timer = new();
        timer.Autostart = true;
        timer.OneShot = true;
		timer.WaitTime = 0.3;
		AddChild(timer);
        timer.Timeout += () => 
        {
Visible = true;
        };
		Scale = new Vector2(1,1) * (float)(Tier * 0.5);
		rootNode = GetTree().Root.GetNode<Node2D>("MainScene");
		Target = GetTree().Root.GetNode<Node2D>("MainScene").GetNode<CharacterBody2D>("Player");
	}
	public override void _PhysicsProcess(double delta)
	{
		if(Position.DistanceTo(Target.Position) > 100)
		{
			Vector2 vel = (Target.Position - Position).Normalized() * Speed * (float)delta;
			MoveAndCollide(vel);
		}
	}
	public void Hit(int damage)
    {
		if(hp <= 0)
			return;
        hp -= damage;
        if(hp <= 0)
            CallDeferred("Die");
    }

	void Die()
    {
        if(IsInstanceValid(this) == true)
        {
			if(Tier > 1)
			{
				for(int i = 0; i < 3;i++)
				{
					Zombie z = zombie.Instantiate<Zombie>();
					z.Position = Position + Dir[i];
					z.Visible= false;
					z.Tier = Tier - 1;
					rootNode.AddChild(z);
				}
			}
            QueueFree();
        }
    }
}
