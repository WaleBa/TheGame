using System.Runtime;

namespace GameE;
public partial class EvilBullet : Projectile
{
    public bool Alive = true;
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
            player.Hit(Damage, 3, (player.Position - Position).Normalized());
            if(IsInstanceValid(this) == true)
                QueueFree();
        }    
    }
}
