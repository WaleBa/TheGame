namespace GameE;
public partial class Cristal : Area2D
{
	public delegate void DeathEventHandler(Cristal me);
	public event DeathEventHandler Death;


	public Cristal parentCristal;
	
	public int HP;
	public int Tier = 3;

	public override void _Ready()
	{
		AddToGroup("Mobs");

		GetNode<Sprite2D>("Sprite2D").Scale = new Vector2(1,1) * (float)Tier /2;
		GetNode<CollisionShape2D>("CollisionShape2D").Scale = new Vector2(1,1) * (float)Tier/2;

		HP = 100 * Tier;
	}
	public override void _PhysicsProcess(double delta)
	{
		if(GlobalPosition.Y < parentCristal.GlobalPosition.Y)
			ZIndex = -1;
		else 
			ZIndex = 0;
	}
	
	public void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
    {
		if(IsInstanceValid(this) == false)
			return;
		if(HP <= 0)
			return;
        HP -= damage;
		
        if(HP <= 0)
            CallDeferred("Die");
    }

	protected virtual void Die()
    {
		if(IsInstanceValid(this) == false)
			return;
		Death?.Invoke(this);
		QueueFree();
    }
}
