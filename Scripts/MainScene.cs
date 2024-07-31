namespace GameE;

public partial class MainScene : Node2D
{
    static readonly Dictionary<MobType, (int Min, int Next)> _multiplicationValuesPerMob = //too long name
                            new Dictionary<MobType, (int Min, int Next)>() {
                                                            {MobType.Zombie, (6, 3)},
                                                            {MobType.SnakeHead, (4, 2)},
                                                            {MobType.MainCristal, (2, 1)} };

    static readonly int[] _zombieScorePerTier = { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };
    static readonly int[] _mainCristalScorePerTier = { 300, 600, 900, 1200, 1500, 1800, 2100, 2400, 2700, 3000 };
    static readonly int[] _snakeHeadScorePerTier = { 150, 300, 450, 600, 750, 900, 1050, 1200, 1350, 1500 };

    Queue<int> _tieredMobsForNextWave = new Queue<int>();//change//tiers for new wave

    Random _random = new Random();
    MobType _currentMob;

    Player _player;//get rid of saying +timer

    Timer _newWaveTimer;
    Timer _tierUpgradeTimer;
    Timer _extraMobTimer;

    Timer _scoreStreakTimer; 
    Timer _scoreStreakMultiplyerTimer;

    Label _scoreStreakLabel; 
    Label _scoreStreakMultiplyerLabel; 
    Label _scoreLabel;

    ulong _score = 0; //START_time
    
    int _scoreStreak = 0; 
    int _scoreStreakMultiplyer = 0;
    int  _mobsKilledStreak = 0;

    int _currentMobTier = 1;
    int _mobKills = 0;
    int _lastMobKills = 0;

    void NewWave()//name: spawn random mob
    {
        while(_tieredMobsForNextWave.Count > 0)
        {
            Vector2 newMobPosition = _player.Position + new Vector2(Global.MAX_DISTANCE_FROM_CENTRE, 0)
                                                                .Rotated(_random.Next(1, 5));//not perfect

            switch(_random.Next(1, 6))
            {
                case 1 | 2 | 3:
                    Zombie zombie = Prefabs.Zombie.Instantiate<Zombie>();
                    
                    zombie.Position = newMobPosition;
                    zombie.Tier = _tieredMobsForNextWave.Dequeue();
                    //zombie.Death += MobKill(MobType.Zombie, zombie.Tier);
                    
                    AddChild(zombie);
                    break;
               case 4 | 5:
                    SnakeHead snake = Prefabs.SnakeHead.Instantiate<SnakeHead>();
                    
                    snake.Position = newMobPosition;
                    snake.Tier = _tieredMobsForNextWave.Dequeue();
                    //snake.Death += (Node2D mob) => MobKill(MobType.SnakeHead, snake.Tier);

                    AddChild(snake);
                    break;
                case 6:
                    MainCristal cristal = Prefabs.MainCristal.Instantiate<MainCristal>();

                    int radius = 200 * cristal.Tier;

                    cristal.Position = _player.Position + new Vector2(Global.MAX_DISTANCE_FROM_CENTRE + radius, 0)
                                                                .Rotated(_random.Next(1, 5)); //cristal.Position = newMobPosition
                    cristal.Tier = _tieredMobsForNextWave.Dequeue();
                    //cristal.Death += (Node2D mob) => MobKill(MobType.MainCristal, cristal.Tier);

                    AddChild(cristal);
                    break;
            }
       }
    }
        
    void MobKill(MobType mobType, int mobTier)//event parameter?
    {
        _tieredMobsForNextWave.Enqueue(mobTier);
        
        Scoring(mobType, mobTier);
        _currentMob = mobType;

        _mobKills++;
        if((_mobKills - _lastMobKills) >= 10)//should not be always 10
        {
            _lastMobKills += 10;
            _player.LevelUp();
        }
    }

    void Scoring(MobType mobKilled, int mobTier)
    {
        switch(mobKilled)
        {
            case MobType.Zombie:
            {   
                MultiplyerSet(MobType.Zombie);
                StreakSet(_zombieScorePerTier[mobTier -1]);
                break;
            }
            case MobType.SnakeHead:
            {   
                MultiplyerSet(MobType.SnakeHead);
                StreakSet(_snakeHeadScorePerTier[mobTier -1]);
                break;
            }
            case MobType.MainCristal:
            {   
                MultiplyerSet(MobType.MainCristal);
                StreakSet(_mainCristalScorePerTier[mobTier -1]);
                break;
            }
        } 
    }

    void MultiplyerSet(MobType mob)
    {
        if(_currentMob != mob)
        {
           _mobsKilledStreak = 1;
            StreakReset();
        }
                
        _mobsKilledStreak++;
                    
        if(_mobsKilledStreak < _multiplicationValuesPerMob[mob].Min)
            return;
        
        int multiplication = 2;        
        for(    
            int m = _mobsKilledStreak - _multiplicationValuesPerMob[mob].Min;
            m >= _multiplicationValuesPerMob[mob].Next; //to long
            m -= _multiplicationValuesPerMob[mob].Next)
        {
            multiplication *= 2;
        }
                
        _scoreStreakMultiplyer = multiplication;
        _scoreStreakMultiplyerLabel.Text = _scoreStreakMultiplyer.ToString();
        _scoreStreakMultiplyerTimer.Start();
    }
    
    void StreakSet(int value)
    {
        _scoreStreak += value;
        _scoreStreakLabel.Text = _scoreStreak.ToString();
        _scoreStreakTimer.Start();
    }
    
    void MultiplyerReset()
    {
        _scoreStreak *= _scoreStreakMultiplyer;
       _scoreStreakMultiplyer = 0;

        _scoreStreakLabel.Text = _scoreStreak.ToString();
       _scoreStreakMultiplyerLabel.Text = "";
    }

    void StreakReset()
    {
        _score += (ulong)_scoreStreak;
        _scoreStreak = 0;

        _scoreLabel.Text = _score.ToString(); 
        _scoreStreakLabel.Text = "";
    }

    public override void _Ready()
    {
        _scoreStreakTimer = GetNode<Timer>("score_streak");
        _scoreStreakMultiplyerTimer = GetNode<Timer>("score_streak_multiplyer");
        _newWaveTimer = GetNode<Timer>("new_wave");
        _tierUpgradeTimer = GetNode<Timer>("tier_upgrade");
        _extraMobTimer = GetNode<Timer>("extra_mob");
        _player = GetNode<Player>("Player");
        _scoreLabel= _player.GetNode<Camera2D>("Camera2D").GetNode<Label>("score");
        _scoreStreakLabel = _player.GetNode<Camera2D>("Camera2D").GetNode<Label>("score_streak");
        _scoreStreakMultiplyerLabel = _player.GetNode<Camera2D>("Camera2D").GetNode<Label>("score_streak_multiplyer");

        _scoreStreakTimer.Timeout += StreakReset;
        _scoreStreakMultiplyerTimer.Timeout += MultiplyerReset;//toolong
        _newWaveTimer.Timeout += NewWave;
        _tierUpgradeTimer.Timeout += () => _currentMobTier++;
        _extraMobTimer.Timeout += () => _tieredMobsForNextWave.Enqueue(_currentMobTier);
        
        _tieredMobsForNextWave.Enqueue(1);
        NewWave();
    }
}