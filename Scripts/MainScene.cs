using System.Collections;

namespace GameE;
public partial class MainScene : Node2D
{
    Random rand = new Random();
    Timer NewWaveTimer, TierUpgradeTimer, ExtraMobTimer;
    int currentTier = 1;
    Queue<int> TieredMobsForNextWave = new Queue<int>();
    public override void _Ready()
    {
        NewWaveTimer = GetNode<Timer>("NewWaveTimer");
        TierUpgradeTimer = GetNode<Timer>("TierUpgradeTimer");
        ExtraMobTimer = GetNode<Timer>("ExtraMobTimer");
        NewWaveTimer.Timeout += NewWave;
        TierUpgradeTimer.Timeout += () => currentTier++;
        ExtraMobTimer.Timeout += () => TieredMobsForNextWave.Enqueue(currentTier);
        NewWaveTimer.Start();
        TierUpgradeTimer.Start();
        ExtraMobTimer.Start();
        TieredMobsForNextWave.Enqueue(1);
    }

    private void NewWave()
    {
        GD.Print(TieredMobsForNextWave.Count);
        foreach(int tier in TieredMobsForNextWave)
        {
            Vector2 FromPlayerPosition = new Vector2(0,0) - GetNode<Player>("Player").Position).Normalized() * 1250;

            int value = rand.Next(1, 6);
            switch(value)
            {
                case 6:
                    MainCristal cristal = Prefabs.MainCristal.Instantiate<MainCristal>();
                    cristal.Death += (int Tier) => TieredMobsForNextWave.Enqueue(Tier);
                    cristal.Position = FromPlayerPosition;
                    cristal.Tier = tier;
                    AddChild(cristal);
                    break;
                case 5:
                    SnakeHead snake = Prefabs.SnakeHead.Instantiate<SnakeHead>();
                    snake.Death += (int Tier) => TieredMobsForNextWave.Enqueue(Tier);
                    snake.Position = FromPlayerPosition;
                    snake.Tier = tier;
                    AddChild(snake);
                    break;
                case 4:
                    SnakeHead snake2 = Prefabs.SnakeHead.Instantiate<SnakeHead>();
                    snake2.Death += (int Tier) => TieredMobsForNextWave.Enqueue(Tier);
                    snake2.Position = FromPlayerPosition;
                    snake2.Tier = tier;
                    AddChild(snake2);
                    break;
                default:
                    Zombie zombie = Prefabs.Zombie.Instantiate<Zombie>();
                    zombie.Death += (int Tier) => TieredMobsForNextWave.Enqueue(Tier);;
                    zombie.Position = FromPlayerPosition;
                    zombie.Tier = tier;
                    AddChild(zombie);
                    break;
            }
        }
    }
}
