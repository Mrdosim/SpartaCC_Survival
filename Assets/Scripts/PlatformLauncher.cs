using System.Collections;
using UnityEngine;

public class PlatformLauncher : MonoBehaviour
{
    public float launchForce = 10f;
    public Vector3 launchDirection = new Vector3(0, 1, 1);
    public float launchDelay = 0.5f;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartCoroutine(LaunchPlayerAfterDelay(other.gameObject.GetComponent<Rigidbody>()));
        }
    }

    private IEnumerator LaunchPlayerAfterDelay(Rigidbody playerRigidbody)
    {
        yield return new WaitForSeconds(launchDelay);
        Vector3 normalizedLaunchDirection = launchDirection.normalized;
        playerRigidbody.AddForce(normalizedLaunchDirection * launchForce, ForceMode.Impulse);
        Debug.Log("Player launched with force: " + normalizedLaunchDirection * launchForce); // 디버그 메시지 추가
    }
}
