namespace GameE;

public partial class MobFabric : Node
{
    //Node2D _mainScene;

    Queue<Zombie> _zombiePool;

    Zombie dek()
    {
        GD.Print("dek");
        Zombie z =_zombiePool.Dequeue();
        //z.DirtySet(tier, position);
               // z.Tier = tier;
       // z.Position = position;//death event passes node2D or zombie?
        z.Death += (Node2D mob) => _zombiePool.Enqueue((Zombie)mob);//pause mobs
        return z;
    }
    Zombie zap()
    {
        GD.Print("zap");
        Zombie z = Prefabs.Zombie.Instantiate<Zombie>();
        //z.Position = position;//death event passes node2D or zombie?
        z.Death += (Node2D mob) => _zombiePool.Enqueue((Zombie)mob);//pause mobs
         GetTree().Root.GetNode<Node2D>("MainScene").AddChild(z);
         return z;
    }

    public Zombie Zombie()
    {
        Zombie zombie = (_zombiePool.Count() <= 0) 
            ? zap()
            : dek();        
    
       // zombie.Tier = tier;
       // zombie.Position = position;//death event passes node2D or zombie?
       // zombie.Death += (Node2D mob) => _zombiePool.Enqueue((Zombie)mob);//pause mobs
        //zombie._Ready();//check
        //zombie.RequestReady();

        return zombie;
    }

    public override void _Ready()
	{
        _zombiePool = new Queue<Zombie>();
        //make mainscene global as player also
    }
}
