using System.Collections;

namespace GameE;
public partial class MainScene : Node2D
{
    Random rand = new Random();
    Timer NewWaveTimer, TierUpgradeTimer, ExtraMobTimer, ScoreStreakTimer, ScoreMultiplicationTimer;
    int currentTier = 1;
    Queue<int> TieredMobsForNextWave = new Queue<int>();
    ulong StartTime;
    int kills, lastKills;

    ulong Score = 0;
    int ScoreStreak = 0;
    int ScoreMultiplication = 0;
    int mobsKilledStreak = 0;
    string currMob;
    Label ScoreStreakLabel, ScoreMultiLabel, ScoreLabel;


    public int[] ZombieScorePerTier = {
        100,
        200,
        300,
        400,
        500,
        600,
        700,
        800,
        900,
        1000
    };
    public int[] MainCristalScorePerTier = {
        300,
        600,
        900,
        1200,
        1500,
        1800,
        2100,
        2400,
        2700,
        3000
    };
    public int[] SnakeHeadScorePerTier = {
        150,
        300,
        450,
        600,
        750,
        900,
        1050,
        1200,
        1350,
        1500
    };
    public override void _Ready()
    {
        ScoreStreakTimer = GetNode<Timer>("ScoreStreakTimer");
        ScoreMultiplicationTimer = GetNode<Timer>("ScoreMultiplicationTimer");
        ScoreStreakTimer.Timeout += StreakTimeout;
        ScoreMultiplicationTimer.Timeout += MultiTimeout;

        ScoreLabel = GetNode<Player>("Player").GetNode<Camera2D>("Camera2D").GetNode<Label>("Score");
        ScoreStreakLabel = GetNode<Player>("Player").GetNode<Camera2D>("Camera2D").GetNode<Label>("Score Streak");
        ScoreMultiLabel = GetNode<Player>("Player").GetNode<Camera2D>("Camera2D").GetNode<Label>("Score Combo Multiplyer");


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

    void StreakTimeout()
    {
        Score += (ulong)ScoreStreak;
        ScoreLabel.Text = Score.ToString(); 
        ScoreStreakLabel.Text = "0";
        ScoreStreak = 0;
    }
    void MultiTimeout()
    {
        ScoreStreak *= ScoreMultiplication;
        ScoreStreakLabel.Text = ScoreStreak.ToString();
        ScoreMultiplication = 0;
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
                    cristal.Death += (Node2D mob) => 
                    {
                        TieredMobsForNextWave.Enqueue(((MainCristal)mob).Tier); 
                        kills++;
                        check();
                        Scoring(mob);
                    };
                    cristal.Tier = TieredMobsForNextWave.Dequeue();
                    //cristal.Position = FromPlayerPosition;
                    int radius = 200 * cristal.Tier;
                    cristal.Position = GetNode<Player>("Player").Position + new Vector2(1250 + radius, 0).Rotated(rand.Next(1, 5));
                    AddChild(cristal);
                    break;
                case 2:
                    SnakeHead snake = Prefabs.SnakeHead.Instantiate<SnakeHead>();
                    snake.Death += (Node2D mob) => 
                    {
                        TieredMobsForNextWave.Enqueue(((SnakeHead)mob).Tier); 
                        kills++; 
                        check();
                        Scoring(mob);
                    };
                    snake.Position = FromPlayerPosition;
                    snake.Tier = TieredMobsForNextWave.Dequeue();
                    AddChild(snake);
                    break;
                case 3:
                    SnakeHead snake2 = Prefabs.SnakeHead.Instantiate<SnakeHead>();
                    snake2.Death += (Node2D mob) => 
                    {
                        TieredMobsForNextWave.Enqueue(((SnakeHead)mob).Tier); 
                        kills++; 
                        check();
                        Scoring(mob);
                    };
                    snake2.Position = FromPlayerPosition;
                    snake2.Tier = TieredMobsForNextWave.Dequeue();
                    AddChild(snake2);
                    break;
                default:
                    Zombie zombie = Prefabs.Zombie.Instantiate<Zombie>();
                    zombie.Death += (Node2D mob) => 
                    {
                        TieredMobsForNextWave.Enqueue(((Zombie)mob).Tier); 
                        kills++; 
                        check();
                        Scoring(mob);
                    };
                    zombie.Position = FromPlayerPosition;
                    zombie.Tier = TieredMobsForNextWave.Dequeue();
                    AddChild(zombie);
                    break;
            }
        }
    }

    public void Scoring(Node2D mob)
    {
        currMob = mob.GetType().ToString();
        if(mob is Zombie zombie)
        {
            if(currMob == "zombie")
            {
                mobsKilledStreak++;
                if(mobsKilledStreak >= 6)
                {
                    ScoreMultiplicationTimer.Start();
                    if(mobsKilledStreak == 6) ScoreMultiplication = 2;
                    else if(mobsKilledStreak - 6 >= 3)
                    {
                        mobsKilledStreak = 6;
                        ScoreMultiplication *= 2;
                        ScoreMultiLabel.Text = ScoreMultiplication.ToString();
                    }
                }
            }
            else
            {
                MultiTimeout();
                mobsKilledStreak = 0;
            }
            ScoreStreak += ZombieScorePerTier[zombie.Tier -1];
            ScoreStreakLabel.Text = ScoreStreak.ToString();
            ScoreStreakTimer.Start();
        }
        else if(mob is MainCristal mainCristal)
        {
            if(currMob == "MainCristal")
            {
                mobsKilledStreak++;
                if(mobsKilledStreak >= 4)
                {
                    ScoreMultiplicationTimer.Start();
                    if(mobsKilledStreak == 4) ScoreMultiplication = 2;
                    else if(mobsKilledStreak - 4 >= 2)
                    {
                        mobsKilledStreak = 4;
                        ScoreMultiplication *= 2;
                        ScoreMultiLabel.Text = ScoreMultiplication.ToString();
                    }
                }
            }
            else
            {
                MultiTimeout();
                mobsKilledStreak = 0;
            }
            ScoreStreak += MainCristalScorePerTier[mainCristal.Tier -1];
            ScoreStreakLabel.Text = ScoreStreak.ToString();
            ScoreStreakTimer.Start();
        }
        else if(mob is SnakeHead snakeHead)
        {
            if(currMob == "SnakeHead")
            {
                mobsKilledStreak++;
                if(mobsKilledStreak >= 2)
                {
                    ScoreMultiplicationTimer.Start();
                    if(mobsKilledStreak == 2) ScoreMultiplication = 2;
                    else if(mobsKilledStreak - 2 >= 1)
                    {
                        mobsKilledStreak = 2;
                        ScoreMultiplication *= 2;
                        ScoreMultiLabel.Text = ScoreMultiplication.ToString();
                    }
                }
            }
            else
            {
                MultiTimeout();
                mobsKilledStreak = 0;
            }
            ScoreStreak += SnakeHeadScorePerTier[snakeHead.Tier -1];
            ScoreStreakLabel.Text = ScoreStreak.ToString();
            ScoreStreakTimer.Start();
        }
    }
    private void check()
    {
        if((kills - lastKills) >= 10)//should not be always 10
        {
            GetNode<Player>("Player").Call("lvl");
            lastKills = lastKills + 10;
        }
    }
}
