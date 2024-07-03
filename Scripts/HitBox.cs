namespace GameE;
public partial class HitBox : Area2D
{
	Node2D parent;
	public override void _Ready()
	{
		parent = GetParent<Node2D>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
    {
		parent.Call("Hit", damage, recoilPower, recoilVectorGiven);
    }
}
