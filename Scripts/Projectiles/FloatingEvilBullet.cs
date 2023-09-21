namespace GameE;
public partial class FloatingEvilBullet : EvilBullet
{
    Timer timer;
    Sprite2D sprite;
    public override void _Ready()
    {
        base._Ready();
        timer = GetNode<Timer>("Timer");
        sprite = GetNode<Sprite2D>("Sprite2D");
        timer.Timeout += () => 
        {
            Alive = true;
            sprite.Visible = true;
        };
    }
    public override void Contact(Node2D body)
    {
        if(Alive == true)
        {
            if(body is Player player)
            {
                player.Hit(Damage, 3, (body.Position - Position).Normalized());
                Vanish();
            }
            else if(body is GoodBullet bullet)
                Vanish();
        }
    }
    public override void _PhysicsProcess(double delta)
    {

    }
    
    private void Vanish()
    {
        Alive = false;
        sprite.Visible = false;
        timer.Start();
    }
}
