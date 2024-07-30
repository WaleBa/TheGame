using System.Runtime;

namespace GameE;
public partial class EvilBullet : Projectile
{
    public override void _Ready()
    {
        base._Ready();
        Speed = 700;
        Range = 800;
        Damage = 10;
    }

    public override void Contact(Node2D body)
    {
        if(body is GoodBullet bullet)
        {
            if(IsInstanceValid(this) == true)
                QueueFree();
        }
        if(body is Player player)
        {
            player.Hit();
            if(IsInstanceValid(this) == true)
                QueueFree();
        }    
    }
}
