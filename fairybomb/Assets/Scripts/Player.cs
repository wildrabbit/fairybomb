using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour, IScheduledEntity
{
    [SerializeField] SpriteRenderer _view;
    public float Speed = 1.0f;
    //...whatever

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Cleanup()
    {

    }

    public void AddTime(float timeUnits, ref PlayContext playContext)
    {
    }
}
