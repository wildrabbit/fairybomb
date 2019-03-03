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

    // Time control / contexts
    float _timeScale;
    float _elapsedUnits;
    int _turns;

    List<IScheduledEntity> _scheduledEntities;
    List<IScheduledEntity> _scheduledToAdd;

    PlayContext _playContext;

    Dictionary<PlayContext, IPlayContext> _playContexts;
    Dictionary<PlayContext, BaseContextData> _playContextData;

    GameInput _input;
    GameResult _result;
    int[] _sampleMap;

    //----------------------- Shortcuts --------------------/

    Player _player;
    FairyBombMap _map;

    void Awake()
    {
        _input = new GameInput(_inputDelay);
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
        _cameraController.Cleanup();

        _map.Cleanup();
        GameObject.Destroy(_map.gameObject);
        _map = null;

        _player.Cleanup();
        GameObject.Destroy(_player.gameObject);
        _player = null;

        _scheduledEntities.Clear();
        _scheduledToAdd.Clear();
    }

    void StartGame()
    {
        _map = Instantiate<FairyBombMap>(_mapPrefab);
        _map.InitFromArray(new Vector2Int(10, 8), System.Array.ConvertAll(_sampleMap, (value) => (TileType)value), new Vector2Int(2, 2));

        _player = Instantiate<Player>(_playerPrefab);
        _player.transform.position = _map.WorldFromCoords(_map.PlayerStart);
        _scheduledEntities.Add(_player);

        _cameraController.SetBounds(_map.GetBounds());
        _cameraController.SetTarget(_player.transform);

        _elapsedUnits = 0;
        _turns = 0;

        _result = GameResult.Running;

        var contextData = ((ActionPhaseData)_playContextData[PlayContext.Action]);
        contextData.Player = _player;
        contextData.Map = _map;
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
            float units = _defaultTimeScale * (1 / _player.Speed);
            foreach (var scheduled in _scheduledEntities)
            {
                scheduled.AddTime(units, ref _playContext);
            }
            _elapsedUnits += units;
            _turns++;
            Debug.Log($"Game time: {_elapsedUnits}, turns: {_turns}");

        //    foreach (var toRemove in _monstersToRemove)
        //    {
        //        _scheduledEntities.Remove(toRemove);
        //        Destroy(toRemove.gameObject);
        //        _monsters.Remove(toRemove);
        //    }
        //    _monstersToRemove.Clear();

        //    foreach (var toAdd in _scheduledToAdd)
        //    {
        //        _scheduledEntities.Add(toAdd);
        //    }
        //    _scheduledToAdd.Clear();
        }

        _result = EvaluateGameResult();
    }

    GameResult EvaluateGameResult()
    {
        Vector2Int playerCoords = _map.CoordsFromWorld(_player.transform.position);
        if (_map.IsGoal(playerCoords))
        {
            Debug.Log($"YAY WON");
            return GameResult.Won;
        }
        //if (Mathf.Approximately(_player.HP, 0.0f))
        //{
        //    Destroy(_player.gameObject);
        //    _player = null;
        //    _scheduledEntities.Remove(_player);
        //    _gameResult = GameResult.Lost;
        //    GameFinished?.Invoke(_gameResult);
        //}
        return GameResult.Running;
    }

    private IEnumerator RestartGame()
    {
        ResetGame();
        yield return new WaitForSeconds(0.1f);
        StartGame(); // TODO: Change level or smthing.
    }
}
