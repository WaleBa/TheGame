namespace GameE;

public partial class MobFabric : Node
{//death event passes node2D or zombie?
    Node2D _mainScene;//pause mobs

    Queue<Zombie> _zombiePool = new();//at start spawn at least 2000 snakeCells
    Queue<SnakeHead> _snakeHeadPool = new();//template
    Queue<SnakeCell> _snakeCellPool = new();
    public Zombie Zombie()
    {
        Zombie zombie; 

        if(_zombiePool.Count() <= 0) 
        {
            zombie = Prefabs.Zombie.Instantiate<Zombie>();
            
            zombie.Death += (Node2D mob) => _zombiePool.Enqueue((Zombie)mob);//pause mobs
            _mainScene.AddChild(zombie);
        }
        else
        {
            zombie = _zombiePool.Dequeue();
        }        

        return zombie;
    }

    public SnakeHead SnakeHead()
    {
        SnakeHead snakeHead;

        if(_snakeHeadPool.Count() <= 0)
        {
            snakeHead = Prefabs.SnakeHead.Instantiate<SnakeHead>();
            snakeHead.Death += (Node2D mob) => _snakeHeadPool.Enqueue((SnakeHead)mob);
            _mainScene.AddChild(snakeHead);
        }
        else
        {
            snakeHead = _snakeHeadPool.Dequeue();
        }

        return snakeHead;
    }

    public SnakeCell SnakeCell()
    {
        SnakeCell snakeCell;

        if(_snakeCellPool.Count() <= 0)
        {
            snakeCell = Prefabs.SnakeCell.Instantiate<SnakeCell>();
            snakeCell.Death += (Node2D mob) => _snakeCellPool.Enqueue((SnakeCell)mob);
            _mainScene.AddChild(snakeCell);
        }
        else
        {
            snakeCell = _snakeCellPool.Dequeue();
        }

        return snakeCell;
    }

    public override void _Ready()
	{
        _mainScene = GetTree().Root.GetNode<Node2D>("MainScene"); //make mainscene global as player also
    }
}
