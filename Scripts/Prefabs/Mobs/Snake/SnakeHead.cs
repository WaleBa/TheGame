namespace GameE;

public partial class SnakeHead : SnakeCell
{
    static int[] _hpPerTier = { 75, 375, 1350, 4350, 13425, 40725, 122700, 368700, 1106775, 3321075 };
    static int[] _maxBodySizePerTier = { 10, 30, 90, 270, 810, 2430, 7290, 21870, 65610, 196830 };

	List<SnakeCell> _body = new();//no add head?

    Timer _regenerationTimer;
	Node _mainScene;

	Vector2? _hidingSpot = null;

	float _radius;
	
    public override void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
    {   
        Escape();
        
        HP -= damage;
        
        if(HP <= 0)
            Die();
    }

    async void Escape()
    {
        _hidingSpot = Position + (Position - Target.Position).Normalized() * _body.Count * DistanceBetweenCells;//CHECK

        foreach(SnakeCell cell in _body)
            cell.Speed = 600;

        await Task.Delay(3000);//how logn
        
        foreach(SnakeCell cell in _body)
            cell.Speed = 300;
        
        _hidingSpot = null;
    }

	void CreateBody()//check if spawn cell should be called deferred
    {
        for (int i = 1; i < _maxBodySizePerTier[Tier -1]; i++)
            SpawnCell();

        Scaling();
        SetRadious();
        
        foreach(SnakeCell cell in _body)//where should this be?
            cell.Speed = 300;
    }

    void SpawnCell()//could check if this is valid (same for zombies and cristals)
    {
        if(IsInstanceValid(this) == false && _hidingSpot != null)
            return;

        SnakeCell snakeCell = Prefabs.SnakeCell.Instantiate<SnakeCell>();
        
        snakeCell.Target = _body.Last();
        snakeCell.Position = _body.Last().Position;
        snakeCell.Death += ManageCut;
        _body.Add(snakeCell);
        snakeCell.HP = (_maxBodySizePerTier[Tier -1] - (_body.Count - 1)) * 10;//-1 -> head
        _mainScene.AddChild(snakeCell);
		//_mainScene.MoveChild(_mainScene, 0); // ?
    }
    
    void Scaling()
    {
        float offset = 0.0025f * _body.Count;//scakubg iffset 
        foreach(SnakeCell cell in _body)
        {
            cell.DistanceBetweenCells = 30 + _body.Count;
            cell.Scale = new Vector2(0.5f + offset,0.5f + offset);
        }
    }

    void ManageCut(Node2D cell)//should trigger escaping?
    {
        int index = _body.IndexOf((SnakeCell)cell);
        
        if(index != -1)
            _body.RemoveRange(index, _body.Count - index);
        
        Scaling();
        SetRadious();
    }

    void SetRadious() => _radius = _body.Count * DistanceBetweenCells / 2 / 3.14f;// /2 pi r
    
    float FinalRotation()
    {
        if(_hidingSpot != null)
            return ((Vector2)_hidingSpot - Position).Angle();

        Vector2 currentVector = new Vector2(1,0).Rotated(Rotation);
        Vector2 targetPosition = (Position - Target.Position).Normalized() * _radius;//need bc: can have negative value
        Vector2 vectorToTargetPosition = (targetPosition - Position).Normalized();
        Vector2 finalVector = (vectorToTargetPosition + currentVector).Normalized();
        //return finalVector.Angle();//(Target.Position - Position).Angle();
        return (Target.Position + (Position - Target.Position).Normalized().Rotated(_radius) * _radius).Angle();
    }

    public override void _Ready()
	{
        AddToGroup("Mobs");
        Tier = 1;

		_mainScene = GetTree().Root.GetNode<Node2D>("MainScene");
        Target = _mainScene.GetNode<Player>("Player");
        _regenerationTimer = GetNode<Timer>("regeneration");
        _body.Add(this);

        _regenerationTimer.Timeout += () => 
        {            
            if(_body.Count >= _maxBodySizePerTier[Tier -1])
                return;

            SpawnCell();
            Scaling();
            SetRadious();
        };
        
        HP =_hpPerTier[Tier -1];

        CallDeferred("CreateBody");

        BodyEntered += (Node2D body) =>
        {
            if(body is Player player)
                player.Hit();
        };
	}

    public override void _PhysicsProcess(double delta)
    {
        Vector2 targetVector;
        Vector2 currentVector = new Vector2(1, 0).Rotated(Rotation);
        if (_hidingSpot == null)
        {
			float dys = Position.DistanceTo(Target.Position);
			Vector2 targetPos = Target.Position + (Position - Target.Position).Normalized().Rotated(_radius / dys) * _radius;
			targetVector = targetPos - Position;
        }
        else
            targetVector = (Vector2)_hidingSpot - Position;

        Rotation = currentVector.Angle() + (currentVector.AngleTo(targetVector) * (float)delta);
        Position += Transform.X * (float)delta * Speed;	
	}
    /*
    public override void _PhysicsProcess(double delta)
    {
        Rotation += FinalRotation() * (float)delta;
        Position += Transform.X * (float)delta * Speed;	
	}*/
}