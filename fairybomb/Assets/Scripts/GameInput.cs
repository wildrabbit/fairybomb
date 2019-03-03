using UnityEngine;

public class InputEntry
{
    public bool Value;
    float _last;

    string _buttonKey;
    float _delay;

    public InputEntry(string key, float delay)
    {
        _buttonKey = key;
        _delay = delay;
        _last = -1;
        Value = false;
    }

    public bool Read()
    {
        if ((_last < 0 || Time.time - _last >= _delay) && Input.GetButton(_buttonKey))
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
}

public enum MoveDirection
{
    None = 0,
    NW,
    N,
    NE,
    SW,
    S,
    SE
}

public class GameInput
{
    const int kNumInputs = 3;
    float _moveInputDelay;

    public bool IdleTurn => idle.Value;
    public bool BombPlaced => placeBomb.Value;
    public bool BombDetonated => detonateBomb.Value;

    public bool Any => Input.anyKeyDown;

    public MoveDirection MoveDir;

   public bool[] numbersPressed;

    InputEntry dirNW;
    InputEntry dirN;
    InputEntry dirNE;
    InputEntry dirSW;
    InputEntry dirS;
    InputEntry dirSE;

    InputEntry idle;
    InputEntry placeBomb;
    InputEntry detonateBomb;

    InputEntry[] numInputs;

    public GameInput(float inputDelay)
    {
        _moveInputDelay = inputDelay;
        dirNW = new InputEntry("dirNW", _moveInputDelay);
        dirN = new InputEntry("dirN", _moveInputDelay);
        dirNE = new InputEntry("dirNE", _moveInputDelay);
        dirSW = new InputEntry("dirSW", _moveInputDelay);
        dirS = new InputEntry("dirS", _moveInputDelay);
        dirSE = new InputEntry("dirSE", _moveInputDelay);

        idle = new InputEntry("idle", _moveInputDelay);
        placeBomb = new InputEntry("place", _moveInputDelay);
        detonateBomb = new InputEntry("detonate", _moveInputDelay);
    }

    public void Read()
    {
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
    }
}
