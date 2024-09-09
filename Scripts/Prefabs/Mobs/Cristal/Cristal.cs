namespace GameE;

public partial class Cristal : Area2D
{
	public delegate void DeathEventHandler(Cristal me);
	public event DeathEventHandler Death;

    public int Tier { get; set; }

	protected static int[] _hpPerTier = { 27, 81, 243,  8700, 26850, 81450, 245400, 737400, 2213550, 6642150 };

    protected int _hp;
	    Sprite2D _sprite;
	RigidBody2D _parentCristal;

    Color _finalColor = new Color(1, 1, 1, 1);
    Color _startColor = new Color(1, 0, 1, 1);
	public virtual void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
    {
        _hp -= damage;
				float offset = 1 - ((float)_hp / (float)_hpPerTier[Tier -1]);
        _sprite.Modulate = _startColor.Lerp(_finalColor, offset);

        if(_hp <= 0)
            CallDeferred("Die");
    }
	
	protected virtual void Die()
    {
		if(IsInstanceValid(this) == false)
			return;

		Death?.Invoke(this);
        ProcessMode =  ProcessModeEnum.Disabled;
        Visible = false;         
    }

	public override void _Ready()
	{
		Tier++;
		AddToGroup("Mobs");
        _sprite = GetNode<Sprite2D>("Sprite2D");
		_parentCristal = GetParent().GetParent<RigidBody2D>();//already tier -1
		
		GetNode<Sprite2D>("Sprite2D").Scale *= 1 + 0.5f * (Tier -1); //= new Vector2(1,1) * (float)Tier /2;
		GetNode<CollisionShape2D>("CollisionShape2D").Scale = new Vector2(1 + 0.5f * (Tier - 1), 1 + 0.5f * (Tier -1));//= new Vector2(1,1) * (float)Tier/2;

		_hp = _hpPerTier[Tier - 1];

		BodyEntered += (Node2D body) =>
        {
            if(body is Player player)
                player.Hit(20, false);
        };
	}

	public override void _PhysicsProcess(double delta)
	{
		GlobalRotation = 0;
		//ZIndex = GlobalPosition.Y < _parentCristal.GlobalPosition.Y ? -1 : 0;
	}
}
