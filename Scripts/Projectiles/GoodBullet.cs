namespace GameE;
public partial class GoodBullet : Projectile
{   
    public override void _Ready()
    {
        base._Ready();
        Speed = 700;
        Range = 800;
    }

    public override void Contact(Node2D body)
    {
        if(body.IsInGroup("Mobs"))
        {
            if(IsInstanceValid(body))
            {
                body.Call("Hit",Damage, 3, (body.Position - Position).Normalized());
                if(IsInstanceValid(this))
                    QueueFree();
            }
        }
        else if(body is EvilBullet bullett)
        {      
            if(IsInstanceValid(this) == true)
                QueueFree();
        }
        else if(body is HitBox hitBox)
        {
            hitBox.Call("Hit", Damage, 3, Position);//pos is not vector of recoil
            if(IsInstanceValid(this))
                QueueFree();
        }
    }
}
