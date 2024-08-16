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

    MobType? _currentMob;

    Player _player;//get rid of saying +timer

    int bariera = 10;

    Timer _newWaveTimer;
    Timer _tierUpgradeTimer;
    Timer _extraMobTimer;
//all timers in-editor got waittime /2
    Timer _scoreStreakTimer; 
    Timer _scoreStreakMultiplyerTimer;

    Label _scoreStreakLabel; 
    Label _scoreStreakMultiplyerLabel; 
    Label _scoreLabel;

    int lastSS = 0;

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
            
            switch(_random.Next(1, 7))
            {
                case 1 | 2 | 3:
                    Zombie zombie = Prefabs.Zombie.Instantiate<Zombie>();
                    
                    zombie.Position = newMobPosition;
                    zombie.Tier = _tieredMobsForNextWave.Dequeue();
                    zombie.Death += (Node2D mob) => MobKill(MobType.Zombie, zombie.Tier, zombie.Position);
                    
                    AddChild(zombie);
                    break;

                case 4 | 5:
                    //GD.Print("snake pre");
                    if(_tieredMobsForNextWave.Peek() == 1)
                        break;
                    //GD.Print("snake po");
                    SnakeHead snake = Prefabs.SnakeHead.Instantiate<SnakeHead>();
                    
                    snake.Position = newMobPosition;
                    snake.Tier = _tieredMobsForNextWave.Dequeue();
                    snake.Death += (Node2D mob) => MobKill(MobType.SnakeHead, snake.Tier, snake.Position);

                    AddChild(snake);
                    break;

               default://bad luck ? or not working?
                    //GD.Print("cristsl pre");                    
                    if(_tieredMobsForNextWave.Peek() == 1 || _tieredMobsForNextWave.Peek() == 2)
                        break;
                    //GD.Print("cristal po");
                    MainCristal cristal = Prefabs.MainCristal.Instantiate<MainCristal>();

                    cristal.Tier = _tieredMobsForNextWave.Dequeue();
                    cristal.Radius = 1250 * (1 + 0.5f * (cristal.Tier - 1));

                    cristal.Position = new Vector2(Global.MAX_DISTANCE_FROM_CENTRE + cristal.Radius, 0)
                                                                .Rotated(_random.Next(1, 5)); //cristal.Position = newMobPosition
                    cristal.Death += (Node2D mob) => MobKill(MobType.MainCristal, cristal.Tier, cristal.Position);
                    
                    AddChild(cristal);
                    break;
            }
       }
    }
        
    void MobKill(MobType mobType, int mobTier, Vector2 pos)//event parameter?
    {
        _tieredMobsForNextWave.Enqueue(mobTier);
        
        Scoring(mobType, mobTier, pos);
        _currentMob = mobType;

        _mobKills++;
        if((_mobKills - _lastMobKills) >= bariera)//should not be always 10
        {
            _lastMobKills += bariera;//looks goofy
            _player.LevelUp();
            bariera *= 2;
        }
    }

    void Scoring(MobType mobKilled, int mobTier, Vector2 pos)
    {
        switch(mobKilled)
        {
            case MobType.Zombie:
            {   
                ScoreNew(MobType.Zombie, mobTier, pos);
                //MultiplyerSet(MobType.Zombie);
                //StreakSet(_zombieScorePerTier[mobTier -1]);
                break;
            }
            case MobType.SnakeHead:
            {   
                ScoreNew(MobType.SnakeHead, mobTier, pos);
                //MultiplyerSet(MobType.SnakeHead);
                //StreakSet(_snakeHeadScorePerTier[mobTier -1]);
                break;
            }
            case MobType.MainCristal:
            {   
                ScoreNew(MobType.MainCristal, mobTier, pos);
                //MultiplyerSet(MobType.MainCristal);
                //StreakSet(_mainCristalScorePerTier[mobTier -1]);
                break;
            }
        } 
    }

    void MultiplyerSet(MobType mob)
    {   
        if(_currentMob != mob)
        {
            MultiplyerReset();
            return;
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

    void ScoreNew(MobType mobType, int mobTier, Vector2 pos)
    {
                Node2D sl = Prefabs.ScoreLabel.Instantiate<Node2D>();
        if(mobType != _currentMob)
        {
            lastSS = 0;
            _currentMob = mobType;
            switch(mobType)
            {
                case MobType.Zombie:
                    sl.GetNode<Label>("Label").Text = (_zombieScorePerTier[mobTier -1]).ToString();
                    _score += (ulong)_zombieScorePerTier[mobTier -1];
                    _scoreLabel.Text = _score.ToString(); 
                    break;
                case MobType.SnakeHead:
                    sl.GetNode<Label>("Label").Text = (_snakeHeadScorePerTier[mobTier - 1]).ToString();
                    _score += (ulong)_snakeHeadScorePerTier[mobTier -1];
                    _scoreLabel.Text = _score.ToString(); 
                    break;
                case MobType.MainCristal:
                    sl.GetNode<Label>("Label").Text = (_mainCristalScorePerTier[mobTier - 1]).ToString();
                    _score += (ulong)_mainCristalScorePerTier[mobTier -1];
                    _scoreLabel.Text = _score.ToString(); 
                    break;
            }
        }
        else
        {
            switch(mobType)
            {
                case MobType.Zombie:
                    sl.GetNode<Label>("Label").Text = (_zombieScorePerTier[mobTier -1] * lastSS).ToString();
                    _score += (ulong)_zombieScorePerTier[mobTier -1];
                    _scoreLabel.Text = _score.ToString(); 
                    break;
                case MobType.SnakeHead:
                    sl.GetNode<Label>("Label").Text = (_snakeHeadScorePerTier[mobTier - 1] * lastSS).ToString();
                    _score += (ulong)_snakeHeadScorePerTier[mobTier -1];
                    _scoreLabel.Text = _score.ToString(); 
                    break;
                case MobType.MainCristal:
                    sl.GetNode<Label>("Label").Text = (_mainCristalScorePerTier[mobTier - 1] * lastSS).ToString();
                   _score += (ulong)_mainCristalScorePerTier[mobTier -1];
                    _scoreLabel.Text = _score.ToString(); 
                    break;
            }
            lastSS++;
        }
        //sl.GetNode<Label>("Label").Text = 
        sl.Position = pos;
        AddChild(sl);
    }
    
    void MultiplyerReset()
    {
        _currentMob = null;
        _mobsKilledStreak = 0;
        
        if(_scoreStreakMultiplyer != 0)
            _scoreStreak *= _scoreStreakMultiplyer;
        _scoreStreakMultiplyer = 0;

        _scoreStreakLabel.Text = _scoreStreak.ToString();
        _scoreStreakMultiplyerLabel.Text = _scoreStreakMultiplyer.ToString();
    }

    void StreakReset()
    {
        _currentMob = null;
        _mobsKilledStreak = 0;

        _score += (ulong)_scoreStreak;
        _scoreStreak = 0;

        _scoreLabel.Text = _score.ToString(); 
        _scoreStreakLabel.Text = _scoreStreakMultiplyer.ToString();
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

        //_scoreStreakTimer.Timeout += StreakReset;
        //_scoreStreakMultiplyerTimer.Timeout += MultiplyerReset;//toolong
        _newWaveTimer.Timeout += NewWave;
        _tierUpgradeTimer.Timeout += () => _currentMobTier++;
        _extraMobTimer.Timeout += () => _tieredMobsForNextWave.Enqueue(_currentMobTier);
        
        _tieredMobsForNextWave.Enqueue(1);
        NewWave();
    }
}