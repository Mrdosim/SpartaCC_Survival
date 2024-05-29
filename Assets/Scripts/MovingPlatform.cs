using UnityEngine;
using UnityEngine.AI;

public class MovingPlatform : MonoBehaviour
{
    public float moveDistance = 10f;  // �̵��� �Ÿ�
    public float waitTime = 1f;       // ��ǥ ������ ������ �� ��� �ð�
    public BoxCollider boundary;      // ���� ��踦 �����ϴ� Box Collider

    private Vector3 startPosition;    // ���� ��ġ
    private Vector3 targetPosition;   // ��ǥ ��ġ
    private NavMeshAgent agent;
    private bool waiting = false;     // ��� ���¸� Ȯ���ϱ� ���� �÷���
    private float waitTimer = 0f;     // ��� �ð� Ÿ�̸�
    private int maxRetries = 10;      // ��ǥ ���� �õ� Ƚ�� ����

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = true;     // ����� ������ õõ�� ����
        agent.updateRotation = false; // ��Ʈ�� �ڿ������� ȸ���ϵ��� �ϱ� ���� ����

        startPosition = transform.position;
        SetNextTarget();
    }

    void SetNextTarget()
    {
        for (int i = 0; i < maxRetries; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * moveDistance;
            randomDirection += startPosition;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomDirection, out hit, moveDistance, NavMesh.AllAreas))
            {
                Vector3 potentialTarget = hit.position;
                if (IsWithinBoundary(potentialTarget))
                {
                    targetPosition = potentialTarget;
                    agent.destination = targetPosition;
                    Debug.Log("New target set: " + targetPosition);
                    return;
                }
            }
        }
        Debug.LogWarning("Failed to find a valid target within boundary.");
    }

    bool IsWithinBoundary(Vector3 position)
    {
        if (boundary != null)
        {
            return boundary.bounds.Contains(position);
        }
        return true; // boundary�� �������� ���� ��� ���� ����
    }

    void Update()
    {
        if (waiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                waiting = false;
                waitTimer = 0f;
                SetNextTarget();
            }
        }
        else
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                waiting = true;
            }
        }
    }
}
