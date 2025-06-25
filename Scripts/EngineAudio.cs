using UnityEngine;

public class EngineAudio : MonoBehaviour
{
    public AudioSource runningSound;
    public float runningMaxVolume = 1f;
    public float runningMaxPitch = 2f;
    public float maxEngineSpeedKmh = 100f;

    private CarAgent carController;
    private Rigidbody rb;

    void Start()
    {
        carController = GetComponent<CarAgent>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (carController == null || runningSound == null || rb == null)
            return;

        float speedKmh = rb.linearVelocity.magnitude * 3.6f;
        float t = Mathf.Clamp01(speedKmh / maxEngineSpeedKmh);

        runningSound.volume = Mathf.Lerp(0f, runningMaxVolume, t);
        runningSound.pitch = Mathf.Lerp(1f, runningMaxPitch, t);
    }
}
