using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace GameE;

public partial class SnakeHead : SnakeBody
{
	PackedScene snakecell = ResourceLoader.Load<PackedScene>("res://Scenes/Mobs/Snake/SnakeCell.tscn");

	List<SnakeBody> body = new();
	Node rootNode;
	Vector2? hidingSpot = null;
	int length, bodySize = 100;
	float radius;


	
	public override void _Ready()
	{
        DistanceBetweenCells = 30;
        body.Add(this);
        Timer timer = new()
        {
            Autostart = true,
            WaitTime = 1.0
        };
        timer.Timeout += () => 
        {
            SnakeBody target = body.Last() as SnakeBody;
            AddCell(target);
            Sizing();
        };
        AddChild(timer);
        
        hp = 100;
		rootNode = GetTree().Root.GetNode<Node2D>("MainScene");
		Target = rootNode.GetNode<CharacterBody2D>("Player");
		length = bodySize * (int)DistanceBetweenCells;
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
        var count = body.Count;
        hidingSpot = Position + (Position - Target.Position).Normalized() * (200 + count/2);
        hp -= damage;
        if(hp <= 0)
            Die();
    }
	void CreateBody()
    {
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
        SnakeBody sc = snakecell.Instantiate<SnakeBody>();
        sc.Target = target;
        sc.Position = target.Position;
        body.Add(sc);
        sc.Death += ManageCut;
        rootNode.AddChild(sc);
		rootNode.MoveChild(sc, 0);
    }
    
    void Sizing()
    {
        var count = body.Count;
        float offset = 0.001f * count;
        for(int i = 0; i < count; i++)//calls from head to last
        {
            body[i].Scale = new Vector2(0.5f + offset,0.5f + offset);
            body[i].DistanceBetweenCells = 30 + offset;
        }
    }
    void ManageCut(SnakeBody cell)
    {
        int index = body.IndexOf(cell);
        if(index != -1)
            body.RemoveRange(index, body.Count - index);
        Sizing();
        length = body.Count * (int)DistanceBetweenCells;
        radius = length /2 / 3.14f;
    }
}

