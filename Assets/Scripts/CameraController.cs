using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;    // 카메라가 따라다닐 타겟 (캐릭터)
    public Vector3 offset;      // 타겟과의 거리
    public float sensitivity = 5f; // 마우스 감도
    public bool canMove = true; // 카메라 이동 가능 여부

    private float currentX = 0f;
    private float currentY = 0f;
    public float minY = -85f;  // 카메라의 최소 Y축 각도
    public float maxY = 85f;   // 카메라의 최대 Y축 각도

    void Update()
    {
        if (canMove)
        {
            currentX += Input.GetAxis("Mouse X") * sensitivity;
            currentY -= Input.GetAxis("Mouse Y") * sensitivity;
            currentY = Mathf.Clamp(currentY, minY, maxY);
        }
    }

    void LateUpdate()
    {
        Vector3 direction = new Vector3(0, 0, -offset.magnitude);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        transform.position = target.position + rotation * direction;
        transform.LookAt(target.position + Vector3.up * offset.y);
    }
}
