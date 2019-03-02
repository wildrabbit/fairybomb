using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraType
{
    Fixed,
    Tracking
}

public class CameraController : MonoBehaviour
{
    public AnimationCurve EaseCurve;
    public CameraType _type;
    Camera _camera;
    Transform _target;
    Rect _worldSize;
    
    void Awake()
    {
        _type = CameraType.Fixed;
        _camera = GetComponent<Camera>();
    }

    public void SetBounds(Rect bounds)
    {
        _worldSize = bounds;
        //FitToBounds();
    }

    public void FitToBounds()
    {
        float halfHeight = _camera.orthographicSize;
        float halfWidth = _camera.aspect * _camera.orthographicSize;
        Vector3 camPos = _camera.transform.position;
        camPos.x = Mathf.Clamp(camPos.x, _worldSize.xMin + halfWidth, _worldSize.xMax - halfWidth);
        camPos.y = Mathf.Clamp(camPos.y, _worldSize.yMin + halfHeight, _worldSize.yMax - halfHeight);
        _camera.transform.position = camPos;
    }

    public void SetTarget(Transform target)
    {
        _type = CameraType.Tracking;
        _target = target;
        _camera.transform.position = new Vector3(_target.position.x, _target.position.y, _camera.transform.position.z);
        //FitToBounds();
    }

    IEnumerator MoveCamera(float time)
    {
        float elapsed = 0;
        Vector3 startPos = _camera.transform.position;
        Vector3 targetPos = new Vector3(_target.position.x, _target.position.y, _camera.transform.position.z);
        while (elapsed < time)
        {
            _camera.transform.position = Vector3.Lerp(startPos, targetPos, EaseCurve.Evaluate(elapsed));
            yield return null;
            elapsed += Time.deltaTime;
        }
    }

    public void SetFixed(Vector3 cameraCenter)
    {
        _type = CameraType.Fixed;
        _target = null;
    }

    public void Update()
    {
        if(_type == CameraType.Tracking && _target != null)
        {
            if(!Mathf.Approximately(Vector2.Distance(_camera.transform.position, _target.position), 0.0f))
            {
                StartCoroutine(MoveCamera(0.25f));
            }

            //_camera.transform.position = new Vector3(_target.position.x, _target.position.y, _camera.transform.position.z);
            //FitToBounds();
        }
    }
}
