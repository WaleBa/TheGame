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
            cell.Speed = 300;
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
    
    float GetRotation(float delta)
    {
        Vector2 targetVector;
        Vector2 currentVector = new Vector2(1, 0).Rotated(Rotation);

        if (_hidingSpot == null)
        {
			Vector2 targetPos = Target.Position + (Position - Target.Position).Normalized().Rotated(_radius) * _radius;
			targetVector = targetPos - Position;
        }
        else
            targetVector = (Vector2)_hidingSpot - Position;

        return currentVector.Angle() + (currentVector.AngleTo(targetVector) * (float)delta);
    }

    public override void _Ready()
	{
        AddToGroup("Mobs");
        
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
        Rotation = GetRotation((float)delta);
        Position += Transform.X * (float)delta * Speed;	
	}
}