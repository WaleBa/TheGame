namespace GameE;

public partial class SnakeHead : SnakeCell
{
    public int Tier { get; set; }
    
    static float[] _hpPerTier = { 13, 40.5f, 91.5f, 700, 13425, 40725, 122700, 368700, 1106775, 3321075 };
    static int[] _maxBodySizePerTier = { 30, 60, 200, 270, 810, 2430, 7290, 21870, 65610, 196830 };
    
    const float A_SCALE = 130;

	List<SnakeCell> _body = new();//no add head?

    Timer _regenerationTimer;//dis bet cell smaller
	Node _mainScene;
//sometimes snake overriding can look cool
    Vector2? _hidingSpot = null;

    MobFabric Fabricate;
	float _radius;
    	    Sprite2D _sprite;
        Color _finalColor = new Color(0.5f, 0, 0.25f, 1);
    Color _startColor = new Color(0.5f, 0, 1, 1);
	
    public override void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
    {   
       // Escape();
        
        HP -= damage;
        float offset = 1 - ((float)HP / (float)_hpPerTier[Tier -1]);
        _sprite.Modulate = _startColor.Lerp(_finalColor, offset);
        if(HP <= 0)
            CallDeferred("Die", this, 600);
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
        if(IsInstanceValid(this) == false && _hidingSpot != null)//last cell hp helps in player escaping
            return;

        SnakeCell snakeCell = Prefabs.SnakeCell.Instantiate<SnakeCell>();
        snakeCell.Target = _body.Last();
        snakeCell.Position = _body.Last().Position;
        snakeCell.HP = (_maxBodySizePerTier[Tier -1] - (_body.Count - 1));//-1 -> head

        snakeCell.Death += ManageCut;
        _body.Add(snakeCell);
        _mainScene.AddChild(snakeCell);
		//_mainScene.MoveChild(_mainScene, 0); // ?
    }
    
    void Scaling()
    {
        float A = (A_SCALE + A_SCALE * 0.5f * (Tier - 1)) / 4;

       // float offset = 0.025f * _body.Count;//scakubg iffset 
        foreach(SnakeCell cell in _body)
        {
            cell.DistanceBetweenCells =  3 * A;//97;//4 * _body.Count;
            cell.Scale = new Vector2(1 + 0.3f * (Tier - 1), 1 + 0.3f * (Tier - 1));
            cell.Speed = 700;//do we need A?
        }
    }

    void ManageCut(Node2D cell, int score)//should trigger escaping?
    {
        int index = _body.IndexOf((SnakeCell)cell);
        
        if(index != -1)
            _body.RemoveRange(index, _body.Count - index);
        
        Scaling();//won't be needed if snakes beakes as whole
        SetRadious();
    }
// : 4 most classic one
    void SetRadious() => _radius = _maxBodySizePerTier[Tier] * DistanceBetweenCells / Mathf.Pi;// /2 //-  (A_SCALE + A_SCALE * 0.3f * (Tier - 1)) / 2;// /2 pi r // * 0.75f / 2
    
    float GetRotation(float delta)
    {
        Vector2 targetVector;
        Vector2 currentVector = new Vector2(1, 0).Rotated(Rotation);

        if (_hidingSpot == null)
        {
            //Vector2 targetPos = Target.Position + (Position - Target.Position).Normalized() * _radius; // cool snakey effect and escaping without hit
			Vector2 targetPos = Target.Position + (Position - Target.Position).Normalized().Rotated(Mathf.Pi / 2) * _radius;//most classic
			targetVector = targetPos - Position;
        }
        else
            targetVector = (Vector2)_hidingSpot - Position;

        return currentVector.Angle() + (currentVector.AngleTo(targetVector) * (float)delta);
    }

    protected override void Die(Node2D me, int score)
    {
        if(IsInstanceValid(this) == false)
            return;
        base.RaiseDeathEvent();
        ProcessMode =  ProcessModeEnum.Disabled;
        Visible = false; 
    }


    public void DirtySet(int tier, Vector2 position)
    {
        ProcessMode =  ProcessModeEnum.Pausable;
        Visible = true; 

        Tier = tier; 
        Position = position;
        //Mass = Tier;
        HP = _hpPerTier[Tier - 1];
        //_speed = 150;
        
        _body.Clear();//here bc snakeCell doesn't have body( could override)
        _body.Add(this);
        _regenerationTimer.Start();
        
        CallDeferred("CreateBody");

        //_sprite.Modulate = _startColor;
        
        //_collisionBox.GetNode<CollisionShape2D>("CollisionShape2D").Scale  = new Vector2(1,1) * Tier; //snake collision box
        //GetNode<CollisionShape2D>("CollisionShape2D").Scale  = new Vector2(1,1) * Tier; 
        //GetNode<Sprite2D>("Sprite2D").Scale  = new Vector2(0.2f, 0.2f) * Tier; 
    }

    public override void _Ready()
	{
        AddToGroup("Mobs");
        _sprite = GetNode<Sprite2D>("Sprite2D");
Fabricate = GetTree().Root.GetNode<MobFabric>("MobFabric");
		_mainScene = GetTree().Root.GetNode<Node2D>("MainScene");
        Target = _mainScene.GetNode<Player>("Player");
        _regenerationTimer = GetNode<Timer>("regeneration");
        _body.Add(this);
        _regenerationTimer.Start();
        
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
                player.Hit(15, false);
        };
	}

    public override void _PhysicsProcess(double delta)
    {
        Rotation = GetRotation((float)delta);
        Position += Transform.X * (float)delta * Speed;	
	}
}