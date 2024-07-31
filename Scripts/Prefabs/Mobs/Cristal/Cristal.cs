namespace GameE;

public partial class Cristal : Area2D
{
	public delegate void DeathEventHandler(Cristal me);
	public event DeathEventHandler Death;

	public int HP { get; set; }
	public int Tier { get; set; }

	Cristal _parentCristal;
	
	public virtual void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
    {
        HP -= damage;
		
        if(HP <= 0)
            CallDeferred("Die");
    }
	
	protected virtual void Die()
    {
		if(IsInstanceValid(this) == false)
			return;

		Death?.Invoke(this);
		// QueueFree();
    }

	public override void _Ready()
	{
		AddToGroup("Mobs");

		_parentCristal = GetParent().GetParent<Cristal>();
		
		GetNode<Sprite2D>("Sprite2D").Scale = new Vector2(1,1) * (float)Tier /2;
		GetNode<CollisionShape2D>("CollisionShape2D").Scale = new Vector2(1,1) * (float)Tier/2;

		BodyEntered += (Node2D body) =>
        {
            if(body is Player player)
                player.Hit();
        };
	}

	public override void _PhysicsProcess(double delta)
	{
		ZIndex = GlobalPosition.Y < _parentCristal.GlobalPosition.Y ? -1 : 0;
	}
}
