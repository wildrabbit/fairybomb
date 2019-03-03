using System.Collections;
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
        Vector2 camPos = _camera.transform.position;
        camPos.x = Mathf.Clamp(camPos.x, _worldSize.xMin + halfWidth, _worldSize.xMax - halfWidth);
        camPos.y = Mathf.Clamp(camPos.y, _worldSize.yMin + halfHeight, _worldSize.yMax - halfHeight);
        SetCamPos2D(camPos);
    }

    public void SetTarget(Transform target)
    {
        _type = CameraType.Tracking;
        _target = target;
        SetCamPos2D(_target.position);
    }

    IEnumerator MoveCamera(float time)
    {
        float elapsed = 0;
        Vector2 startPos = _camera.transform.position;
        Vector2 targetPos = new Vector2(_target.position.x, _target.position.y);
        while (elapsed < time)
        {
            SetCamPos2D(Vector2.Lerp(startPos, targetPos, EaseCurve.Evaluate(elapsed)));
            yield return null;
            elapsed += Time.deltaTime;
        }
    }

    public void SetCamPos2D(Vector2 position)
    {
        Vector3 newPos = new Vector3(position.x, position.y, _camera.transform.position.z);
        _camera.transform.position = newPos;
    }

    public void SetFixed(Vector2 cameraCenter)
    {
        _type = CameraType.Fixed;
        SetCamPos2D(cameraCenter);
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

    internal void Cleanup()
    {
        _target = null;
    }
}
