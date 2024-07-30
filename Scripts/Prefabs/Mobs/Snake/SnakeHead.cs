namespace GameE;

public partial class SnakeHead : SnakeBody
{
	List<SnakeBody> body = new();
	Node rootNode;
	Vector2? hidingSpot = null;
	int length, bodySize;//niker
	float radius;
    Timer timer;
    public int Tier;

    int LastCellHP;
    public int[] HpPerTier = {
        75,
        375,
        1350,
        4350,
        13425,
        40725,
        122700,
        368700,
        1106775,
        3321075
    };

    public int[] BodySizePerTier = {
        10,
        30,
        90,
        270,
        810,
        2430,
        7290,
        21870,
        65610,
        196830
    };
	
	public override void _Ready()
	{
        DistanceBetweenCells = 30;
        body.Add(this);
        timer = new()
        {
            Autostart = true,
            WaitTime = 1.0
        };
        timer.Timeout += () => 
        {
            SnakeBody target = body.Last() as SnakeBody;
            if(body.Count < bodySize) //not growing over body limit
                AddCell(target);
            Sizing();
            timer.WaitTime = 1.0f;
        };
        AddChild(timer);
        
        HP = HpPerTier[Tier -1];
        bodySize = BodySizePerTier[Tier -1] + 1;
		rootNode = GetTree().Root.GetNode<Node2D>("MainScene");
		Target = rootNode.GetNode<Player>("Player");
		length = bodySize * (int)DistanceBetweenCells;
        radius = length / 3.14f;// /2
        CallDeferred("CreateBody");
	}


    public override void _PhysicsProcess(double delta)
    {
        Vector2 targetVector;
        Vector2 currentVector = new Vector2(1, 0).Rotated(Rotation);
        if (hidingSpot == null)
        {
			float dys = Position.DistanceTo(Target.Position);
			Vector2 targetPos = Target.Position + (Position - Target.Position).Normalized().Rotated(radius / dys) * radius;
			targetVector = targetPos - Position;
        }
        else
            targetVector = (Vector2)hidingSpot - Position;

        Rotation = currentVector.Angle() + (currentVector.AngleTo(targetVector) * (float)delta);
        Position += Transform.X * (float)delta * Speed;	
        if (hidingSpot != null && Position.DistanceTo((Vector2)hidingSpot) <= 80) 
        {
            hidingSpot = null;
        }
        Godot.Collections.Array<Node2D> bodiez = GetOverlappingBodies();
        for(int i = 0; i< bodiez.Count; i++)
        {
            if(bodiez[i] is Player player)
                player.Hit();
        }
	}

    protected override async void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
    {
        var count = body.Count;
        hidingSpot = Position + (Position - Target.Position).Normalized() * (600 + count/2);
        foreach(SnakeBody cell in body)
            cell.Speed = 600;
        RemoveEscape();
        HP -= damage;
        if(HP <= 0)
            Die();
    }

    async void RemoveEscape()
    {
        await Task.Delay(5000);
        foreach(SnakeBody cell in body)
            cell.Speed = 300;
        hidingSpot = null;
    }

	void CreateBody()
    {
        LastCellHP = bodySize * 10;
		SnakeBody target = this;
        for (int i = 1; i < bodySize; i++)
        {
            AddCell(target);
            target = body.Last() as SnakeBody;
        }
        Sizing();
    }
    void AddCell(SnakeBody target)
    {
        SnakeBody sc = Prefabs.SnakeCell.Instantiate<SnakeBody>();
        sc.Target = target;
        sc.Position = target.Position;
        sc.Speed = Speed;
        body.Add(sc);
        sc.Death += ManageCut;
        sc.HP = LastCellHP;
        LastCellHP -= 10;
        rootNode.AddChild(sc);
		rootNode.MoveChild(sc, 0);
    }
    
    void Sizing()
    {
        var count = body.Count;
        float offset = 0.0025f * count;
        for(int i = 0; i < count; i++)//calls from head to last
        {
            body[i].Scale = new Vector2(0.5f + offset,0.5f + offset);
            body[i].DistanceBetweenCells = 30 + offset;
        }
    }
    void ManageCut(Node2D cell)
    {
        LastCellHP = ((SnakeBody)cell).HP;
        int index = body.IndexOf((SnakeBody)cell);
        if(index != -1)
            body.RemoveRange(index, body.Count - index);
        Sizing();
        length = body.Count * (int)DistanceBetweenCells;
        radius = length /2 / 3.14f;
        timer.WaitTime = 5.0f;
        timer.Start();
    }
}

