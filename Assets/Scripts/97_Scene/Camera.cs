using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform Target;
    public float speed;
    public float z = -10f;
    public float yOffest = 0f;
    public Vector2 center;
    public Vector2 size;

    private float orthographicSize;
    private float horizontalGraphicSize;

    public bool isCamera = true;

    private void Start()
    {
        orthographicSize = Camera.main.orthographicSize;
        horizontalGraphicSize = orthographicSize * Screen.width / Screen.height;

        if (Player.Instance != null)
        {
            Target = Player.Instance.transform;
        }
    }

    private void LateUpdate()
    {
        if (Player.Instance != null)
        {
            Target = Player.Instance.transform;
            cameraMovement();
            //maxCameraMovement();
        }
    }

    private void cameraMovement()
    {
        if (Target == null) return;

        Vector3 targetPosition;

        if (isCamera)
        {
            targetPosition = new Vector3(Target.position.x, Target.position.y + yOffest, z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);
        }
        else
        {
            targetPosition = new Vector3(Target.position.x, transform.position.y, z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);
        }
    }

    private void maxCameraMovement()
    {
        float lx = size.x * 0.5f - horizontalGraphicSize;
        float clampX = Mathf.Clamp(transform.position.x, -lx + center.x, lx + center.x);

        float ly = size.y * 0.5f - orthographicSize;
        float clampY = Mathf.Clamp(transform.position.y, -ly + center.y, ly + center.y);

        transform.position = new Vector3(clampX, clampY, z);
    }
}