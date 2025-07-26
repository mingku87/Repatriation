using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    private Transform Target => Player.Instance.transform;
    public Vector2 center;
    public Vector2 size;

    private float orthographicSize;
    private float horizontalGraphicSize;

    private void Start()
    {
        orthographicSize = Camera.main.orthographicSize;
        horizontalGraphicSize = orthographicSize * Screen.width / Screen.height;
    }

    private void LateUpdate()
    {
        cameraMovement();
    }

    private void cameraMovement()
    {
        if (Target == null) return;

        Vector3 targetPosition;

        targetPosition = new Vector3(Target.position.x, Target.position.y + InGameConstant.cameraOffsetY, InGameConstant.cameraOffsetZ);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * InGameConstant.cameraSpeed);
    }

    private void maxCameraMovement()
    {
        float lx = size.x * 0.5f - horizontalGraphicSize;
        float clampX = Mathf.Clamp(transform.position.x, -lx + center.x, lx + center.x);

        float ly = size.y * 0.5f - orthographicSize;
        float clampY = Mathf.Clamp(transform.position.y, -ly + center.y, ly + center.y);

        transform.position = new Vector3(clampX, clampY, InGameConstant.cameraOffsetZ);
    }
}