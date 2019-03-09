using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public enum CameraType
{
    Fixed,
    Tracking
}

public class CameraController : MonoBehaviour
{
    public Camera Camera => _camera;
    public AnimationCurve EaseCurve;
    [FormerlySerializedAs("_type")] public CameraType CameraType;
    Camera _camera;
    Transform _target;
    Rect _worldSize;
    
    void Awake()
    {
        CameraType = CameraType.Fixed;
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
        CameraType = CameraType.Tracking;
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
        CameraType = CameraType.Fixed;
        SetCamPos2D(cameraCenter);
        _target = null;
    }

    public void Update()
    {
    }

    public void PlayerMoved(Vector2Int newCoords, Vector2 worldPos, BaseEntity whoMoved)
    {
        SetCamPos2D(worldPos);
    }

    internal void Cleanup()
    {
        _target = null;
    }
}
