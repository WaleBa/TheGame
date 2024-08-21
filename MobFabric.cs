namespace GameE;

public partial class MobFabric : Node
{
    //Node2D _mainScene;

    Queue<Zombie> _zombiePool;

    public Zombie Zombie(int tier, Vector2 position)
    {
        Zombie zombie = (_zombiePool.Count() <= 0) 
            ? Prefabs.Zombie.Instantiate<Zombie>() 
            : _zombiePool.Dequeue();        
    
        zombie.Tier = tier;
        zombie.Position = position;//death event passes node2D or zombie?
        zombie.Death += (Node2D mob) => _zombiePool.Enqueue((Zombie)mob);//pause mobs
        //zombie._Ready();//check
        zombie.RequestReady();

        return zombie;
    }

    public override void _Ready()
	{
        _zombiePool = new Queue<Zombie>();
        //make mainscene global as player also
    }
}
