using UnityEngine;
using UnityEngine.AI;

public class MovingPlatform : MonoBehaviour
{
    public float moveDistance = 10f;  // 이동할 거리
    public float waitTime = 1f;       // 목표 지점에 도착한 후 대기 시간
    public BoxCollider boundary;      // 구역 경계를 설정하는 Box Collider

    private Vector3 startPosition;    // 시작 위치
    private Vector3 targetPosition;   // 목표 위치
    private NavMeshAgent agent;
    private bool waiting = false;     // 대기 상태를 확인하기 위한 플래그
    private float waitTimer = 0f;     // 대기 시간 타이머
    private int maxRetries = 10;      // 목표 설정 시도 횟수 제한

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = true;     // 경로의 끝에서 천천히 멈춤
        agent.updateRotation = false; // 보트가 자연스럽게 회전하도록 하기 위해 설정

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
        return true; // boundary가 설정되지 않은 경우 제한 없음
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
