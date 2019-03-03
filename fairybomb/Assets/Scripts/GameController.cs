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

    [SerializeField] FairyBombMap _mapPrefab;
    [SerializeField] Player _playerPrefab;
    [SerializeField] float _inputDelay = 0.25f;
    [SerializeField] float _defaultTimeScale = 1.0f;

    IEntityController _entityController;


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

    //----------------------- Shortcuts --------------------/

    FairyBombMap _map;

    void Awake()
    {
        _input = new GameInput(_inputDelay);
        _entityController = new EntityController();

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

        StartGame();
    }

    void ResetGame()
    {
        _entityController.OnEntitiesAdded -= RegisterScheduledEntities;
        _entityController.OnEntitiesRemoved -= UnregisterScheduledEntities;
        _entityController.OnBombExploded -= _map.BombExploded;
        _entityController.OnPlayerKilled -= PlayerKilled;

        _cameraController.Cleanup();

        _map.Cleanup();
        GameObject.Destroy(_map.gameObject);
        _map = null;

        _entityController.Cleanup();

        _scheduledEntities.Clear();
        _scheduledToAdd.Clear();
    }

    private void PlayerKilled()
    {
        _result = GameResult.Lost;
        // TODO: Lost event
        Debug.Log("Booo, Lost");
    }

    void StartGame()
    {
        _map = Instantiate<FairyBombMap>(_mapPrefab);
        _map.InitFromArray(new Vector2Int(10, 8), System.Array.ConvertAll(_sampleMap, (value) => (TileType)value), new Vector2Int(2, 2));

        _entityController.Init(_map);
        _entityController.OnEntitiesAdded += RegisterScheduledEntities;
        _entityController.OnEntitiesRemoved += UnregisterScheduledEntities;
        _entityController.CreatePlayer(_playerPrefab, _map.PlayerStart);
        _entityController.OnPlayerKilled += PlayerKilled;

        _entityController.OnBombExploded += _map.BombExploded;

        Rect mapBounds = _map.GetBounds();
        _cameraController.SetBounds(mapBounds);
        _cameraController.SetFixed(new Vector3(mapBounds.width/2, mapBounds.height/2, _cameraController.transform.position.z));

        _elapsedUnits = 0;
        _turns = 0;

        _result = GameResult.Running;

        var contextData = ((ActionPhaseData)_playContextData[PlayContext.Action]);
        contextData.EntityController = _entityController;
        contextData.Player = _entityController.Player;
        contextData.Map = _map;

        _entityController.AddNewEntities();
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

        _input.Read();
        bool timeWillPass;
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
            Debug.Log($"Game time: {_elapsedUnits}, turns: {_turns}");
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
            Debug.Log($"YAY WON");
            // TODO: Won event
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
