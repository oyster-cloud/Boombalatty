using UnityEngine;

[ExecuteAlways]
public class CameraFitBoard : MonoBehaviour
{
    public Camera targetCamera;
    public float boardWidth = 5f;
    public float padding = 0.3f;

    private void Reset()
    {
        targetCamera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (targetCamera == null || !targetCamera.orthographic) return;

        float aspect = (float)Screen.width / Screen.height;

        float halfWidth = (boardWidth * 0.5f) + padding;

        targetCamera.orthographicSize = halfWidth / aspect;
    }
}
