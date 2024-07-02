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
        while(TieredMobsForNextWave.Count > 0)
        {
            Vector2 FromPlayerPosition = GetNode<Player>("Player").Position + new Vector2(1250, 0).Rotated(rand.Next(1, 5));//not perfect

            int value = rand.Next(1 ,6);
            switch(value)
            {
                case 1:
                    MainCristal cristal = Prefabs.MainCristal.Instantiate<MainCristal>();
                    cristal.Death += (int Tier) => TieredMobsForNextWave.Enqueue(Tier);
                    cristal.Position = FromPlayerPosition;
                    cristal.Tier = TieredMobsForNextWave.Dequeue();
                    AddChild(cristal);
                    break;
                case 2:
                    SnakeHead snake = Prefabs.SnakeHead.Instantiate<SnakeHead>();
                    snake.Death += (int Tier) => TieredMobsForNextWave.Enqueue(Tier);
                    snake.Position = FromPlayerPosition;
                    snake.Tier = TieredMobsForNextWave.Dequeue();
                    AddChild(snake);
                    break;
                case 3:
                    SnakeHead snake2 = Prefabs.SnakeHead.Instantiate<SnakeHead>();
                    snake2.Death += (int Tier) => TieredMobsForNextWave.Enqueue(Tier);
                    snake2.Position = FromPlayerPosition;
                    snake2.Tier = TieredMobsForNextWave.Dequeue();
                    AddChild(snake2);
                    break;
                default:
                    Zombie zombie = Prefabs.Zombie.Instantiate<Zombie>();
                    zombie.Death += (int Tier) => TieredMobsForNextWave.Enqueue(Tier);;
                    zombie.Position = FromPlayerPosition;
                    zombie.Tier = TieredMobsForNextWave.Dequeue();
                    AddChild(zombie);
                    break;
            }
        }
    }
}
