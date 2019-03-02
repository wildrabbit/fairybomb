using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] CameraController _cameraController;

    [SerializeField] FairyBombMap _mapPrefab;
    [SerializeField] Player _playerPrefab;
    [SerializeField] float _inputDelay = 0.25f;

    Player _player;
    FairyBombMap _map;

    GameInput _input;

    void Awake()
    {
        _input = new GameInput(_inputDelay);
    }
    void Start()
    {
        int[] sampleMap = new int[]
        {
            0,0,0,0,0,0,0,0,0,0,
            0,1,1,1,1,0,1,1,1,0,
            0,1,1,1,1,0,1,1,1,0,
            0,1,1,1,1,2,1,1,1,0,
            0,0,1,0,0,2,0,2,0,0,
            0,1,1,1,1,2,1,1,1,0,
            0,1,1,1,1,0,1,1,1,0,
            0,0,0,0,0,0,0,0,0,0,
        };
        _map = Instantiate<FairyBombMap>(_mapPrefab);
        _map.InitFromArray(new Vector2Int(10, 8), System.Array.ConvertAll(sampleMap, (value) => (TileType)value), new Vector2Int(2,2));

        _player = Instantiate<Player>(_playerPrefab);
        _player.transform.position = _map.WorldFromCoords(_map.PlayerStart);

        _cameraController.SetBounds(_map.GetBounds());
        _cameraController.SetTarget(_player.transform);
    }

    // Update is called once per frame
    void Update()
    {
        //if (_gameResult != GameResult.Running)
        //{
        //    if (Input.anyKeyDown)
        //    {
        //        StartCoroutine(RestartGame());
        //    }
        //    return;
        //}

        _input.Read();
        Vector2Int _playerCoords = _map.CoordsFromWorld(_player.transform.position);
        Vector2Int offset = Vector2Int.zero;
        bool evenColumn = _playerCoords.y % 2 == 0;
        MoveDirection moveDir = _input.MoveDir;
        switch (moveDir)
        {
            case MoveDirection.None:
            {
                return;
            }
            case MoveDirection.NW:
            {
                offset.Set(evenColumn ? 0 : 1, -1);
                break;
            }
            case MoveDirection.N:
            {
                offset.Set(1, 0);
                break;
            }
            case MoveDirection.NE:
            {
                offset.Set(evenColumn ? 0 : 1, 1);
                break;
            }
            case MoveDirection.SW:
            {
                offset.Set(evenColumn ? -1 : 0 , - 1);
                break;
            }
            case MoveDirection.SE:
            {
                offset.Set(evenColumn ? -1 : 0 , 1);
                break;
            }
            case MoveDirection.S:
            {
                offset.Set(-1, 0);
                break;
            }
        }

        _playerCoords += offset;
        _map.ConstrainCoords(ref _playerCoords);
        Vector2 _playerPos = _map.WorldFromCoords(_playerCoords);
        _player.transform.position = _playerPos;

        //bool willSpendTime;
        //_playContext = _playContexts[_playContext].Update(_playContextData[_playContext], out willSpendTime);

        //if (willSpendTime)
        //{
        //    float units = _gameConfig.DefaultTimescale * (1 / _player.Speed);
        //    foreach (var scheduled in _scheduledEntities)
        //    {
        //        scheduled.AddTime(units, ref _playContext);
        //    }
        //    _elapsedUnits += units;
        //    _turns++;
        //    Debug.Log($"Game time: {_elapsedUnits}, turns: {_turns}");

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
        //}

        //if (Mathf.Approximately(_player.HP, 0.0f))
        //{
        //    Destroy(_player.gameObject);
        //    _player = null;
        //    _scheduledEntities.Remove(_player);
        //    _gameResult = GameResult.Lost;
        //    GameFinished?.Invoke(_gameResult);
        //}
    }
}
