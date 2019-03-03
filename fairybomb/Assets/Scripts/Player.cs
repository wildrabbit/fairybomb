using UnityEngine;
using System.Collections;

public class Player : BaseEntity
{
    [SerializeField] SpriteRenderer _view;
    public Bomb _bombPrefab;
    public float Speed = 1.0f;
    //...whatever

    public override void AddTime(float timeUnits, ref PlayContext playContext)
    {

    }
}
