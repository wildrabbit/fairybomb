using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameResult
{
    None,
    Running,
    Won,
    Lost
}

public class GameController : MonoBehaviour
{
    [SerializeField] CameraController _cameraController;

    [SerializeField] HUD _hudPrefab;
    [SerializeField] FairyBombMap _mapPrefab;
    [SerializeField] Player _playerPrefab;
    [SerializeField] GameObject _explosionPrefab;
    [SerializeField] Monster _monster1Prefab;
    [SerializeField] Monster _monster2Prefab;
    [SerializeField] float _inputDelay = 0.25f;
    [SerializeField] float _defaultTimeScale = 1.0f;

    public int Turns => _turns;
    public float TimeUnits => _elapsedUnits;

    IEntityController _entityController;
    MonsterCreator _monsterCreator;
    AIController _aiController;
    GameEventLog _eventLog;
    HUD _hud;
    List<GameObject> _explosionItems;


    // Time control / contexts
    float _timeScale;
    float _elapsedUnits;
    int _turns;

    List<IScheduledEntity> _scheduledEntities;
    List<IScheduledEntity> _scheduledToAdd;

    PlayContext _playContext;
    
    // :thinking: Does the behaviour/data make sense for this? Should context also include the data?
    Dictionary<PlayContext, IPlayContext> _playContexts;
    Dictionary<PlayContext, BaseContextData> _playContextData;

    GameInput _input;
    GameResult _result;
    int[] _sampleMap;

    Vector2Int _sampleDimensions;
    Vector2Int _samplePlayerStart;
    List<MonsterSpawn> _sampleMonsters;

    //----------------------- Shortcuts --------------------/

    FairyBombMap _map;

    void Awake()
    {
        _input = new GameInput(_inputDelay);
        _entityController = new EntityController();

        _eventLog = new GameEventLog();
        _aiController = new AIController();
        _monsterCreator = new MonsterCreator();

        _explosionItems = new List<GameObject>();
        _scheduledEntities = new List<IScheduledEntity>();
        _scheduledToAdd = new List<IScheduledEntity>();
        _result = GameResult.None;
        InitializePlayContexts();
        InitializePlayContextsData();
    }

    void InitializePlayContexts()
    {
        _playContexts = new Dictionary<PlayContext, IPlayContext>();
        _playContexts[PlayContext.Action] = new ActionPhaseContext();
    }

    void InitializePlayContextsData()
    {
        _playContextData = new Dictionary<PlayContext, BaseContextData>();

        ActionPhaseData actionCtxtData = new ActionPhaseData();
        actionCtxtData.input = _input;
        actionCtxtData.BumpingWallsWillSpendMoves = false;
        actionCtxtData.Log = _eventLog;
        _playContextData[PlayContext.Action] = actionCtxtData;
    }

    void Start()
    {
        _sampleMap = new int[]
        {
            0,0,0,0,0,0,0,0,0,0,
            0,1,1,1,1,0,1,1,3,0,
            0,1,1,1,1,0,1,1,1,0,
            0,1,1,1,1,1,1,1,1,0,
            0,0,1,0,0,2,0,2,0,0,
            0,1,1,1,1,2,1,1,1,0,
            0,1,1,1,1,0,1,1,1,0,
            0,0,0,0,0,0,0,0,0,0,
        };
        _sampleDimensions = new Vector2Int(8, 10);
        _samplePlayerStart = new Vector2Int(2, 2);

        _sampleMonsters = new List<MonsterSpawn>();
        _sampleMonsters.Add(new MonsterSpawn()
        {
            Prefab = _monster1Prefab,
            Coords = new Vector2Int(6, 2)
        });
        _sampleMonsters.Add(new MonsterSpawn()
        {
            Prefab = _monster2Prefab,
            Coords = new Vector2Int(2, 7)
        });
        _eventLog.Init();

        StartGame();
    }

    void ResetGame()
    {
        _aiController.Cleanup();

        _entityController.OnEntitiesAdded -= RegisterScheduledEntities;
        _entityController.OnEntitiesRemoved -= UnregisterScheduledEntities;
        _entityController.OnBombExploded -= BombExploded;
        _entityController.OnBombSpawned -= BombSpawned;
        _entityController.OnBombExploded -= _map.BombExploded;
        _entityController.OnPlayerKilled -= PlayerKilled;

        _cameraController.Cleanup();

        _hud.Cleanup();
        Destroy(_hud.gameObject);

        foreach(var explosion in _explosionItems)
        {
            Destroy(explosion);
        }
        _explosionItems.Clear();

        _map.Cleanup();
        GameObject.Destroy(_map.gameObject);
        _map = null;

        _entityController.Cleanup();

        _scheduledEntities.Clear();
        _scheduledToAdd.Clear();
    }

    private void BombSpawned(Bomb bomb)
    {
        BombActionEvent bombEvt = new BombActionEvent(_turns, _elapsedUnits);
        bombEvt.SetSpawned(bomb);
        _eventLog.AddEvent(bombEvt);
    }

    private void BombExploded(Bomb bomb, List<Vector2Int> coords, BaseEntity triggerEntity)
    {
        BombActionEvent bombEvt = new BombActionEvent(_turns, _elapsedUnits);
        if (triggerEntity == null)
        {
            bombEvt.SetTimedOut(bomb);
        }
        else if (triggerEntity is Bomb)
        {
            bombEvt.SetChainExplosion(bomb, ((Bomb)triggerEntity));
        }
        _eventLog.AddEvent(bombEvt);

        foreach (var coord in coords)
        {
            var explosion = Instantiate(_explosionPrefab);
            explosion.transform.position = _map.WorldFromCoords(coord);
            _explosionItems.Add(explosion);
            StartCoroutine(DelayedKillExplosion(explosion, 0.25f));
        }
    }

    private IEnumerator DelayedKillExplosion(GameObject explosion, float delay)
    {
        yield return new WaitForSeconds(delay);
        _explosionItems.Remove(explosion);
        Destroy(explosion);
    }

    private void PlayerKilled()
    {
        _result = GameResult.Lost;
        GameFinishedEvent evt = new GameFinishedEvent(_turns, _elapsedUnits, GameResult.Lost);
        _eventLog.EndSession(evt);

        StartCoroutine(DelayedPurge(0.25f));
    }

    IEnumerator DelayedPurge(float delay)
    {
        yield return new WaitForSeconds(delay);
        _entityController.PurgeEntities();
    }
    void StartGame()
    {
        _map = Instantiate<FairyBombMap>(_mapPrefab);
        _map.InitFromArray(_sampleDimensions, System.Array.ConvertAll(_sampleMap, (value) => (TileType)value), _samplePlayerStart, arrayOriginTopLeft:true);

        _entityController.Init(_map);
        _entityController.OnEntitiesAdded += RegisterScheduledEntities;
        _entityController.OnEntitiesRemoved += UnregisterScheduledEntities;
        _entityController.CreatePlayer(_playerPrefab, _map.PlayerStart);
        _entityController.OnPlayerKilled += PlayerKilled;
        _entityController.OnBombExploded += BombExploded;
        _entityController.OnBombSpawned += BombSpawned;
        _entityController.OnBombExploded += _map.BombExploded;

        _aiController.Init(_entityController, _map, _eventLog);

        // Inic camera
        Rect mapBounds = _map.GetBounds();
        _cameraController.SetBounds(mapBounds);
        _cameraController.SetFixed(new Vector3(mapBounds.width/2, mapBounds.height/2, _cameraController.transform.position.z));

        // Init default context
        var contextData = ((ActionPhaseData)_playContextData[PlayContext.Action]);
        contextData.EntityController = _entityController;
        contextData.Player = _entityController.Player;
        contextData.Map = _map;

        // le hud
        _hud = Instantiate<HUD>(_hudPrefab);
        _hud.Init(_eventLog, _entityController.Player, () => Turns, () => TimeUnits);

        // populate the level
        _monsterCreator.Init(_entityController, _aiController, _map, _eventLog);
        _monsterCreator.AddInitialMonsters(_sampleMonsters);

        _entityController.AddNewEntities();

        // Init game control / time vars
        _elapsedUnits = 0;
        _turns = 0;
        _result = GameResult.Running;

        // Starting event!
        var setupEvt = new GameSetupEvent();
        setupEvt.MapSize = _sampleDimensions;
        setupEvt.MapTiles = _sampleMap;
        setupEvt.PlayerCoords = _samplePlayerStart;
        setupEvt.HP = _entityController.Player.HP;
        setupEvt.MaxHP = _entityController.Player.MaxHP;
        _eventLog.StartSession(setupEvt);
    }

    void RegisterScheduledEntities(List<BaseEntity> entities)
    {
        foreach(var e in entities)
        {
            _scheduledEntities.Add(e);
        }
    }

    void UnregisterScheduledEntities(List<BaseEntity> entities)
    {
        foreach(var e in entities)
        {
            _scheduledEntities.Remove(e);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_result != GameResult.Running)
        {
            if (_input.Any)
            {
                StartCoroutine(RestartGame());
            }
            return;
        }

        if(Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log(_eventLog.Flush());
        }

        _input.Read();
        bool timeWillPass;

        _playContextData[_playContext].Refresh(this);
        
        _playContext = _playContexts[_playContext].Update(_playContextData[_playContext], out timeWillPass);

        if (timeWillPass)
        {
            float units = _defaultTimeScale * (1 / _entityController.Player.Speed);
            foreach (var scheduled in _scheduledEntities)
            {
                scheduled.AddTime(units, ref _playContext);
            }
            _elapsedUnits += units;
            _turns++;
        }

        _entityController.RemovePendingEntities();
        _entityController.AddNewEntities();

        if(_result == GameResult.Running)
        {
            _result = EvaluateVictory();
        }
    }

    GameResult EvaluateVictory()
    {
        if (_map.IsGoal(_entityController.Player.Coords))
        {
            // TODO: Won event
            GameFinishedEvent evt = new GameFinishedEvent(_turns, _elapsedUnits, GameResult.Won);
            _eventLog.EndSession(evt);
            return GameResult.Won;
        }
        return GameResult.Running;
    }

    private IEnumerator RestartGame()
    {
        ResetGame();
        yield return new WaitForSeconds(0.1f);
        StartGame(); // TODO: Change level or smthing.
    }
}
