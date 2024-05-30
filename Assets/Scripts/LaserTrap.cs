using System.Collections;
using UnityEngine;

public class LaserTrap : MonoBehaviour
{
    public Transform rayOrigin;
    public Transform rayTarget;
    public LayerMask detectionLayer;
    public GameObject warningMessage;
    public float trapDelay = 1.0f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;

    private bool trapActivated = false;

    void Update()
    {
        DetectPlayer();
    }

    void DetectPlayer()
    {
        Vector3 direction = rayTarget.position - rayOrigin.position;
        float distance = Vector3.Distance(rayOrigin.position, rayTarget.position);

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin.position, direction, out hit, distance, detectionLayer))
        {
            if (hit.collider.CompareTag("Player") && !trapActivated)
            {
                Debug.Log("Player detected by laser trap!");
                trapActivated = true;
                StartCoroutine(ActivateTrap());
            }
        }

    }


    IEnumerator ActivateTrap()
    {
        if (warningMessage != null)
        {
            warningMessage.SetActive(true);
        }

        yield return new WaitForSeconds(trapDelay);

        if (warningMessage != null)
        {
            warningMessage.SetActive(false);
        }

        trapActivated = false;
    }
}
