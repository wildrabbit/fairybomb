using UnityEngine;

public class InputEntry
{
    public bool Value;
    float _last;

    KeyCode _key;
    float _delay;

    public InputEntry(KeyCode key, float delay)
    {
        _key = key;
        _delay = delay;
        _last = -1;
        Value = false;
    }

    public bool Read()
    {
        if ((_last < 0 || Time.time - _last >= _delay) && Input.GetKey(_key))
        {
            _last = Time.time;
            Value = true;
        }
        else
        {
            Value = false;
        }
        return Value;
    }

    public void UpdateKey(KeyCode newKey)
    {
        _key = newKey;
        _last = -1;
        Value = false;
    }
}

public enum MoveDirection
{
    None = 0,
    N,
    NE,
    SE,
    S,
    SW,
    NW
}

public class GameInput
{
    public const int kLayoutQwerty = 0;
    public const int kLayoutAzerty = 1;
    
    const int kNumInputs = 3;
    float _moveInputDelay;
    int _currentLayout = 0;

    KeyCode[][] _directionLayouts = new KeyCode[][]
    {
        new KeyCode[]{KeyCode.W, KeyCode.E, KeyCode.D, KeyCode.S, KeyCode.A, KeyCode.Q},
        new KeyCode[]{KeyCode.Z, KeyCode.E, KeyCode.D, KeyCode.S, KeyCode.Q, KeyCode.A},
    };

    public event System.Action<int> OnLayoutChanged;

    public bool IdleTurn => idle.Value;
    public bool BombPlaced => placeBomb.Value;
    public bool BombDetonated => detonateBomb.Value;

    public bool Any => Input.anyKeyDown;

    public MoveDirection MoveDir;

    public bool ShiftPressed => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

    public bool[] NumbersPressed;
    public KeyCode StartKeyCode;

    InputEntry dirNW;
    InputEntry dirN;
    InputEntry dirNE;
    InputEntry dirSW;
    InputEntry dirS;
    InputEntry dirSE;

    InputEntry idle;
    InputEntry placeBomb;
    InputEntry detonateBomb;

    public GameInput()
    {

    }

    public void ChangeLayout(int layoutIdx)
    {
        _currentLayout = layoutIdx;
        KeyCode[] keycodes = _directionLayouts[_currentLayout];
        dirNW.UpdateKey(keycodes[(int)MoveDirection.NW - 1]);
        dirN.UpdateKey(keycodes[(int)MoveDirection.N - 1]);
        dirNE.UpdateKey(keycodes[(int)MoveDirection.NE - 1]);
        dirSW.UpdateKey(keycodes[(int)MoveDirection.SW - 1]);
        dirS.UpdateKey(keycodes[(int)MoveDirection.S - 1]);
        dirSE.UpdateKey(keycodes[(int)MoveDirection.SE - 1]);
        OnLayoutChanged?.Invoke(_currentLayout);

    }

    public void Init(float inputDelay, int layout = kLayoutQwerty)
    {
        _currentLayout = layout;
        KeyCode[] keycodes = _directionLayouts[_currentLayout];

        _moveInputDelay = inputDelay;
        dirNW = new InputEntry(keycodes[(int)MoveDirection.NW - 1], _moveInputDelay);
        dirN = new InputEntry(keycodes[(int)MoveDirection.N - 1], _moveInputDelay);
        dirNE = new InputEntry(keycodes[(int)MoveDirection.NE - 1], _moveInputDelay);
        dirSW = new InputEntry(keycodes[(int)MoveDirection.SW - 1], _moveInputDelay);
        dirS = new InputEntry(keycodes[(int)MoveDirection.S - 1], _moveInputDelay);
        dirSE = new InputEntry(keycodes[(int)MoveDirection.SE - 1], _moveInputDelay);

        idle = new InputEntry(KeyCode.Space, _moveInputDelay);
        placeBomb = new InputEntry(KeyCode.J, _moveInputDelay);
        detonateBomb = new InputEntry(KeyCode.K, _moveInputDelay);

        NumbersPressed = new bool[kNumInputs];
        NumbersPressed.Fill<bool>(false);
        StartKeyCode = KeyCode.Alpha1;
    }

    public void Read()
    {
        if(Input.GetKeyUp(KeyCode.Tab))
        {
            ChangeLayout((_currentLayout + 1) % 2);
        }

        detonateBomb.Read();
        placeBomb.Read();
        idle.Read();

        MoveDir = MoveDirection.None;
        if(dirNW.Read())
        {
            MoveDir = MoveDirection.NW;
        }
        else if(dirN.Read())
        {
            MoveDir = MoveDirection.N;
        }
        else if(dirNE.Read())
        {
            MoveDir = MoveDirection.NE;
        }
        else if(dirSW.Read())
        {
            MoveDir = MoveDirection.SW;
        }
        else if(dirS.Read())
        {
            MoveDir = MoveDirection.S;
        }
        else if(dirSE.Read())
        {
            MoveDir = MoveDirection.SE;
        }

        for(int i = 0; i < kNumInputs; ++i)
        {
            NumbersPressed[i] = Input.GetKeyUp(i + StartKeyCode);
        }
    }
}
