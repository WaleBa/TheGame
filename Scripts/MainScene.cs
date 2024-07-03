using System.Collections;

namespace GameE;
public partial class MainScene : Node2D
{
    Random rand = new Random();
    Timer NewWaveTimer, TierUpgradeTimer, ExtraMobTimer;
    int currentTier = 1;
    Queue<int> TieredMobsForNextWave = new Queue<int>();
    ulong StartTime;
    int kills, lastKills;
    public override void _Ready()
    {
        StartTime = Time.GetTicksMsec();
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
        GD.Print($"next wave mobs count: {TieredMobsForNextWave.Count},time passed: {(Time.GetTicksMsec() - StartTime) / 1000}, and kills:{kills}");
        while(TieredMobsForNextWave.Count > 0)
        {
            Vector2 FromPlayerPosition = GetNode<Player>("Player").Position + new Vector2(1250, 0).Rotated(rand.Next(1, 5));//not perfect

            int value = rand.Next(1 ,6);
            switch(value)
            {
                case 1:
                    MainCristal cristal = Prefabs.MainCristal.Instantiate<MainCristal>();
                    cristal.Death += (int Tier) => {TieredMobsForNextWave.Enqueue(Tier); kills++;};
                    cristal.Tier = TieredMobsForNextWave.Dequeue();
                    //cristal.Position = FromPlayerPosition;
                    int radius = 200 * cristal.Tier;
                    cristal.Position = GetNode<Player>("Player").Position + new Vector2(1250 + radius, 0).Rotated(rand.Next(1, 5));
                    AddChild(cristal);
                    break;
                case 2:
                    SnakeHead snake = Prefabs.SnakeHead.Instantiate<SnakeHead>();
                    snake.Death += (int Tier) => {TieredMobsForNextWave.Enqueue(Tier); kills++; check();};
                    snake.Position = FromPlayerPosition;
                    snake.Tier = TieredMobsForNextWave.Dequeue();
                    AddChild(snake);
                    break;
                case 3:
                    SnakeHead snake2 = Prefabs.SnakeHead.Instantiate<SnakeHead>();
                    snake2.Death += (int Tier) => {TieredMobsForNextWave.Enqueue(Tier); kills++; check();};
                    snake2.Position = FromPlayerPosition;
                    snake2.Tier = TieredMobsForNextWave.Dequeue();
                    AddChild(snake2);
                    break;
                default:
                    Zombie zombie = Prefabs.Zombie.Instantiate<Zombie>();
                    zombie.Death += (int Tier) => {TieredMobsForNextWave.Enqueue(Tier); kills++; check();};
                    zombie.Position = FromPlayerPosition;
                    zombie.Tier = TieredMobsForNextWave.Dequeue();
                    AddChild(zombie);
                    break;
            }
        }
    }

    private void check()
    {
        if((kills - lastKills) >= 10)
        {
            GetNode<Player>("Player").Call("lvl");
            lastKills = lastKills + 10;
        }
    }
}
