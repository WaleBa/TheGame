namespace GameE;

public partial class SnakeHead : SnakeBody
{
	PackedScene snakecell = ResourceLoader.Load<PackedScene>("res://Scenes/Mobs/Snake/SnakeCell.tscn");

	List<SnakeBody> body = new();
	Node rootNode;

	Vector2? hidingSpot = null;
	int length, size = 200;
	float radius;

	
	public override void _Ready()
	{
        body.Add(this);
        Timer timer = new();
        timer.Autostart = true;
        timer.WaitTime = 5.0;
        timer.Timeout += () => 
        {
            SnakeBody target = body.Last() as SnakeBody;
            AddCell(target);
        };
        AddChild(timer);
        
        hp = 100;
		rootNode = GetTree().Root.GetNode<Node2D>("MainScene");
		Target = rootNode.GetNode<CharacterBody2D>("Player");
		length = size * DistanceBetweenCells;
        radius = length /2 / 3.14f;
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
	}

    protected override void Hit(int damage, int recoilPower, Vector2 recoilVectorGiven)
    {
        hidingSpot = Position + (Position - Target.Position).Normalized() * 200;
        hp -= damage;
        if(hp <= 0)
            Die();
    }
	void CreateBody()
    {
		SnakeBody target = this;
        for (int i = 1; i < size; i++)
        {
            AddCell(target);
            target = body.Last() as SnakeBody;
        }
    }
    void AddCell(SnakeBody target)
    {
        SnakeBody sc = snakecell.Instantiate<SnakeBody>();
        sc.Target = target;
        sc.Position = target.Position;
        body.Add(sc);
        sc.Death += ManageCut;
        rootNode.AddChild(sc);
		rootNode.MoveChild(sc, 0);
    }
    void ManageCut(SnakeBody cell)
    {
        int index = body.IndexOf(cell);
        if(index != -1)
            body.RemoveRange(index, body.Count - index);
    }
}

