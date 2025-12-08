using UnityEngine;

public class Player : MonoBehaviour
{
    private Camera cam;

    [Header("카메라 오프셋")]
    public Vector3 cameraOffset = new Vector3(0f, 1.5f, -10f);

    private void Start()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        cam.transform.position = transform.position + cameraOffset;
    }
}
