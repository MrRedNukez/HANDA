using UnityEngine;

public class EarthquakeManager : MonoBehaviour
{
    private enum Phase { Idle, Waiting, Rumble, Quake, FadeOut }

    [Header("Target")]
    [Tooltip("The house (or any object) that will shake. Can have a Rigidbody or not.")]
    public Transform house;

    [Header("Timing")]
    public bool autoStart = true;
    [Tooltip("Seconds after the game loads before the earthquake begins.")]
    public float startDelay = 3f;
    [Tooltip("Random length (min, max) of the slow, gentle rumble phase.")]
    public Vector2 rumbleDurationRange = new Vector2(7f, 10f);
    [Tooltip("How long the strong quake lasts after the rumble. 0 = never ends.")]
    public float quakeDuration = 30f;
    [Tooltip("Seconds to calm down when the quake ends.")]
    public float fadeOutDuration = 3f;

    [Header("Intensity")]
    [Range(0f, 2f)] public float rumbleStartIntensity = 0.02f;
    [Range(0f, 2f)] public float rumbleEndIntensity = 0.1f;
    [Tooltip("The max intensity is randomly picked in this range each time the game loads.")]
    public Vector2 peakIntensityRange = new Vector2(0.4f, 1.5f);
    [Tooltip("Random time (min, max) between intensity spikes.")]
    public Vector2 spikeIntervalRange = new Vector2(0.5f, 2f);
    [Tooltip("How fast intensity moves toward each new spike. Higher = snappier.")]
    public float spikeSharpness = 4f;

    [Header("Shake Feel")]
    public float frequency = 15f;
    [Tooltip("Small tilt in degrees at intensity 1. Set 0 for position-only shake.")]
    public float tiltDegrees = 0.5f;

    // Read these from other scripts (camera shake, audio, etc.)
    public float CurrentIntensity { get; private set; }
    public float PeakIntensity { get; private set; }
    public bool IsShaking => phase == Phase.Rumble || phase == Phase.Quake || phase == Phase.FadeOut;

    private Phase phase = Phase.Idle;
    private float phaseTimer;
    private float rumbleDuration;
    private float spikeTarget;
    private float spikeTimer;
    private float fadeStartIntensity;
    private float noiseSeed;

    private Rigidbody rb;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Awake()
    {
        if (house == null)
        {
            Debug.LogWarning("EarthquakeManager: No house assigned!", this);
            enabled = false;
            return;
        }

        rb = house.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            originalPosition = rb.position;
            originalRotation = rb.rotation;
        }
        else
        {
            originalPosition = house.position;
            originalRotation = house.rotation;
        }
    }

    void Start()
    {
        // Different random values every time the game loads
        noiseSeed = Random.Range(0f, 1000f);

        if (autoStart) StartEarthquake();
    }

    void Update()
    {
        if (rb == null) Tick();
    }

    void FixedUpdate()
    {
        if (rb != null) Tick();
    }

    void Tick()
    {
        float dt = Time.deltaTime;
        UpdatePhase(dt);
        ApplyShake();
    }

    void UpdatePhase(float dt)
    {
        phaseTimer += dt;

        switch (phase)
        {
            case Phase.Waiting:
                CurrentIntensity = 0f;
                if (phaseTimer >= startDelay) BeginRumble();
                break;

            case Phase.Rumble:
                float t = Mathf.Clamp01(phaseTimer / rumbleDuration);
                CurrentIntensity = Mathf.Lerp(rumbleStartIntensity, rumbleEndIntensity, t);
                if (phaseTimer >= rumbleDuration) BeginQuake();
                break;

            case Phase.Quake:
                spikeTimer -= dt;
                if (spikeTimer <= 0f)
                {
                    spikeTarget = Random.Range(rumbleEndIntensity, PeakIntensity);
                    // Occasionally hit the full peak
                    if (Random.value < 0.2f) spikeTarget = PeakIntensity;
                    spikeTimer = Random.Range(spikeIntervalRange.x, spikeIntervalRange.y);
                }

                CurrentIntensity = Mathf.Lerp(CurrentIntensity, spikeTarget, 1f - Mathf.Exp(-spikeSharpness * dt));

                if (quakeDuration > 0f && phaseTimer >= quakeDuration) BeginFadeOut();
                break;

            case Phase.FadeOut:
                float f = fadeOutDuration > 0f ? Mathf.Clamp01(phaseTimer / fadeOutDuration) : 1f;
                CurrentIntensity = Mathf.Lerp(fadeStartIntensity, 0f, f);
                if (f >= 1f) phase = Phase.Idle;
                break;

            case Phase.Idle:
                CurrentIntensity = 0f;
                break;
        }
    }

    void ApplyShake()
    {
        Vector3 pos = originalPosition;
        Quaternion rot = originalRotation;

        if (CurrentIntensity > 0.0001f)
        {
            float time = phaseTimerAbsolute() * frequency;

            float shakeX = (Mathf.PerlinNoise(noiseSeed + time, 0f) - 0.5f) * 2f * CurrentIntensity;
            float shakeZ = (Mathf.PerlinNoise(0f, noiseSeed + time) - 0.5f) * 2f * CurrentIntensity;
            pos += new Vector3(shakeX, 0f, shakeZ);

            if (tiltDegrees > 0f)
            {
                float tiltX = (Mathf.PerlinNoise(noiseSeed + 50f + time, 10f) - 0.5f) * 2f * tiltDegrees * CurrentIntensity;
                float tiltZ = (Mathf.PerlinNoise(10f, noiseSeed + 90f + time) - 0.5f) * 2f * tiltDegrees * CurrentIntensity;
                rot = originalRotation * Quaternion.Euler(tiltX, 0f, tiltZ);
            }
        }

        if (rb != null)
        {
            rb.MovePosition(pos);
            rb.MoveRotation(rot);
        }
        else
        {
            house.SetPositionAndRotation(pos, rot);
        }
    }

    float phaseTimerAbsolute() => Time.time;

    void BeginRumble()
    {
        phase = Phase.Rumble;
        phaseTimer = 0f;
        rumbleDuration = Random.Range(rumbleDurationRange.x, rumbleDurationRange.y);
    }

    void BeginQuake()
    {
        phase = Phase.Quake;
        phaseTimer = 0f;
        PeakIntensity = Random.Range(peakIntensityRange.x, peakIntensityRange.y);
        spikeTarget = rumbleEndIntensity;
        spikeTimer = 0f;
    }

    void BeginFadeOut()
    {
        phase = Phase.FadeOut;
        phaseTimer = 0f;
        fadeStartIntensity = CurrentIntensity;
    }

    // ---------- Public API ----------

    public void StartEarthquake()
    {
        phase = Phase.Waiting;
        phaseTimer = 0f;
        CurrentIntensity = 0f;
    }

    public void StopEarthquake()
    {
        if (IsShaking) BeginFadeOut();
        else phase = Phase.Idle;
    }
}