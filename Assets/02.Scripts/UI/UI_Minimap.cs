using UnityEngine;

public class UI_Minimap : MonoBehaviour
{
    [SerializeField] private Camera _minimapCamera;

    [SerializeField] private float _zoomStep = 1f;
    [SerializeField] private float _minSize = 4f;
    [SerializeField] private float _maxSize = 16f;

    public void ZoomOut()
    {
        float size = _minimapCamera.orthographicSize + _zoomStep;
        _minimapCamera.orthographicSize = Mathf.Min(size, _maxSize);
    }

    public void ZoomIn()
    {
        float size = _minimapCamera.orthographicSize - _zoomStep;
        _minimapCamera.orthographicSize = Mathf.Max(size, _minSize);
    }
}
