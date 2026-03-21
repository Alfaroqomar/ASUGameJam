using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 baseOffset = new Vector2(0, 0);
    [SerializeField] private Vector2 offsetWhenDownIsHeld = new Vector2(0, -5);
    [SerializeField] private Vector2 deadzone = new Vector2(10, 10);
    [SerializeField] private Vector2 softzone = new Vector2(500, 500);

    [Header("Movement")]
    [SerializeField] private float softZoneSpeed = 0.05f;
    [SerializeField] private float hardZoneSpeed = 0.075f;

    /*
     * CAMERA TRACKING
     */

    void FixedUpdate()
    {
        transform.position = CalculateCameraPosition();
    }

    Vector3 CalculateCameraPosition() {
        Vector2 offset = (Input.GetAxis("Vertical") < -0.5) ? offsetWhenDownIsHeld : baseOffset;
        Vector3 goalPosition = target.position + new Vector3(offset.x, offset.y, 0);
        Vector3 currentPosition = transform.position;

        float deltaX = Mathf.Abs(goalPosition.x - currentPosition.x);
        float deltaY = Mathf.Abs(goalPosition.y - currentPosition.y);

        // If player is outside deadzone, move camera slowly, if outside softzone, move camera faster
        float newX = (deltaX > deadzone.x) ? Mathf.Lerp(currentPosition.x, goalPosition.x, softZoneSpeed) : currentPosition.x;
        if (deltaX > softzone.x) {
            newX = Mathf.Lerp(currentPosition.x, goalPosition.x, hardZoneSpeed);
        }
        float newY = (deltaY > deadzone.y) ? Mathf.Lerp(currentPosition.y, goalPosition.y, softZoneSpeed) : currentPosition.y;
        if (deltaY > softzone.y) {
            newY = Mathf.Lerp(currentPosition.y, goalPosition.y, hardZoneSpeed);
        }

        return new Vector3(newX, newY, currentPosition.z);
    }

    /*
     * CAMERA SHAKE
     */

    // Use this function to trigger a camera shake with the desired strength and duration (seconds)
    public void StartShake(float strength = 0.5f, float duration = 0.2f) {
        StartCoroutine(Shake(strength, duration));
    }

    // Handles shaking the camera over a set period of time
    private IEnumerator Shake(float strength, float duration)
    {
        Quaternion originalRotation = transform.rotation;
        float timeLeft = duration;

        while (timeLeft > 0)
        {
            // Gradually reduce shake strength
            float calculatedStrength = timeLeft / duration * strength;
            float x = Random.Range(-1f, 1f) * calculatedStrength;
            float y = Random.Range(-1f, 1f) * calculatedStrength;
            float z = Random.Range(-1f, 1f) * calculatedStrength * 0;

            transform.position = CalculateCameraPosition() + new Vector3(x, y, 0);
            transform.rotation = Quaternion.Euler(0, 0, z);

            timeLeft -= Time.fixedDeltaTime;
            yield return new WaitForSecondsRealtime(Time.fixedDeltaTime);
        }

        transform.rotation = originalRotation;
    }
}
