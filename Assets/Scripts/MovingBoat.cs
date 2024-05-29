using UnityEngine;
using System.Collections;

public class MovingBoat : MonoBehaviour
{
    public Transform[] points;
    public float speed = 2f;
    public float waitTime = 1f;

    private int currentPointIndex = 0;
    private bool waiting = false;
    private bool forward = true;

    void Update()
    {
        if (!waiting)
        {
            Transform targetPoint = points[currentPointIndex];
            transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
            {
                StartCoroutine(WaitAtPoint());
            }
        }
    }

    private IEnumerator WaitAtPoint()
    {
        waiting = true;
        yield return new WaitForSeconds(waitTime);

        if (forward)
        {
            currentPointIndex++;
            if (currentPointIndex >= points.Length)
            {
                currentPointIndex = points.Length - 1;
                forward = false;
            }
        }
        else
        {
            currentPointIndex--;
            if (currentPointIndex < 0)
            {
                currentPointIndex = 0;
                forward = true;
            }
        }

        waiting = false;
    }
}
