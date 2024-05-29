using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;    // ī�޶� ����ٴ� Ÿ�� (ĳ����)
    public Vector3 offset;      // Ÿ�ٰ��� �Ÿ�
    public float sensitivity = 5f; // ���콺 ����
    public bool canMove = true; // ī�޶� �̵� ���� ����

    private float currentX = 0f;
    private float currentY = 0f;
    public float minY = -85f;  // ī�޶��� �ּ� Y�� ����
    public float maxY = 85f;   // ī�޶��� �ִ� Y�� ����

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
