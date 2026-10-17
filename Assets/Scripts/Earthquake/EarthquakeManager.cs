using UnityEngine;

public class EarthquakeManager : MonoBehaviour
{
    private enum Phase { Idle, Waiting, Rumble, Quake, FadeOut, AftershockWait }

    [Header("Hub Link")]
    public SimulationManager simManager;

    [Header("Target")]
    public Transform house;

    [Header("UI Alerts")]
    public GameObject redAlertPanel;
    public float alertDuration = 3f;

    [Header("Debris Spawner")]
    public GameObject debrisPrefab;
    public Transform player;
    public EarthquakeTimer timerSystem;
    [Tooltip("Random time between debris drops.")]
    public Vector2 spawnIntervalRange = new Vector2(5f, 10f);
    public float spawnHeight = 5f;
    private float currentDebrisTimer;

    [Header("Timing")]
    public bool autoStart = true;
    public float startDelay = 3f;
    public Vector2 rumbleDurationRange = new Vector2(7f, 10f);
    public float quakeDuration = 30f;
    public float fadeOutDuration = 3f;

    [Header("Aftershocks")]
    public bool enableAftershocks = true;
    public int aftershocksToTrigger = 1;
    public Vector2 aftershockDelayRange = new Vector2(5f, 15f);
    [Range(0.1f, 1f)] public float aftershockIntensityMultiplier = 0.5f;

    [Header("Intensity")]
    [Range(0f, 2f)] public float rumbleStartIntensity = 0.02f;
    [Range(0f, 2f)] public float rumbleEndIntensity = 0.1f;
    public Vector2 peakIntensityRange = new Vector2(0.4f, 1.5f);
    public Vector2 spikeIntervalRange = new Vector2(0.5f, 2f);
    public float spikeSharpness = 4f;

    [Header("Shake Feel")]
    public float frequency = 15f;
    public float tiltDegrees = 0.5f;

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

    private int completedAftershocks = 0;
    private float currentAftershockDelay = 0f;
    private bool isCurrentlyAftershock = false;

    private Rigidbody rb;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Awake()
    {
        if (house == null) return;

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
        noiseSeed = Random.Range(0f, 1000f);
        if (autoStart) StartEarthquake();
    }

    void Update()
    {
        if (rb == null) Tick(Time.deltaTime); 
    }

    void FixedUpdate()
    {
        if (rb != null) Tick(Time.fixedDeltaTime); 
    }

    void Tick(float dt)
    {
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
                
                if (phaseTimer >= alertDuration && redAlertPanel != null)
                {
                    redAlertPanel.SetActive(false);
                }
                
                if (phaseTimer >= rumbleDuration) BeginQuake(false);
                break;

            case Phase.Quake:
                spikeTimer -= dt;
                if (spikeTimer <= 0f)
                {
                    spikeTarget = Random.Range(rumbleEndIntensity, PeakIntensity);
                    if (Random.value < 0.2f) spikeTarget = PeakIntensity;
                    spikeTimer = Random.Range(spikeIntervalRange.x, spikeIntervalRange.y);
                }

                CurrentIntensity = Mathf.Lerp(CurrentIntensity, spikeTarget, 1f - Mathf.Exp(-spikeSharpness * dt));

                // DEBRIS SPAWNING LOOP
                currentDebrisTimer -= dt;
                if (currentDebrisTimer <= 0f)
                {
                    SpawnDebris();
                    currentDebrisTimer = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
                }

                float activeDuration = isCurrentlyAftershock ? (quakeDuration * 0.5f) : quakeDuration;
                if (activeDuration > 0f && phaseTimer >= activeDuration) BeginFadeOut();
                break;

            case Phase.FadeOut:
                float f = fadeOutDuration > 0f ? Mathf.Clamp01(phaseTimer / fadeOutDuration) : 1f;
                CurrentIntensity = Mathf.Lerp(fadeStartIntensity, 0f, f);
                
                if (f >= 1f) 
                {
                    if (enableAftershocks && completedAftershocks < aftershocksToTrigger)
                    {
                        phase = Phase.AftershockWait;
                        phaseTimer = 0f;
                        currentAftershockDelay = Random.Range(aftershockDelayRange.x, aftershockDelayRange.y);
                    }
                    else
                    {
                        phase = Phase.Idle;
                    }
                }
                break;

            case Phase.AftershockWait:
                CurrentIntensity = 0f;
                if (phaseTimer >= currentAftershockDelay)
                {
                    completedAftershocks++;
                    BeginQuake(true); 
                }
                break;
        }
    }

    void ApplyShake()
    {
        if (house == null) return;

        Vector3 pos = originalPosition;
        Quaternion rot = originalRotation;

        if (CurrentIntensity > 0.0001f)
        {
            float time = Time.time * frequency;
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

    private void SpawnDebris()
    {
        if (debrisPrefab == null || player == null) return;
        
        Vector3 spawnPos = player.position + (Vector3.up * spawnHeight);
        GameObject debris = Instantiate(debrisPrefab, spawnPos, Random.rotation);
        
        DebrisHazard hazard = debris.GetComponent<DebrisHazard>();
        if (hazard != null)
        {
            hazard.Setup(timerSystem);
        }
    }

    // ---------- Public API ----------
    void BeginRumble()
    {
        phase = Phase.Rumble;
        phaseTimer = 0f;
        isCurrentlyAftershock = false;
        rumbleDuration = Random.Range(rumbleDurationRange.x, rumbleDurationRange.y);

        if (redAlertPanel != null) redAlertPanel.SetActive(true);
    }

    void BeginQuake(bool isAftershock)
    {
        phase = Phase.Quake;
        phaseTimer = 0f;
        isCurrentlyAftershock = isAftershock;
        
        PeakIntensity = Random.Range(peakIntensityRange.x, peakIntensityRange.y);
        
        if (isAftershock) PeakIntensity *= aftershockIntensityMultiplier;
        
        spikeTarget = rumbleEndIntensity;
        
        // Reset debris timer at the start of a quake
        currentDebrisTimer = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y); 
        
        // Start the UI Timer if it's the main quake
        if (!isAftershock)
        {
            if (redAlertPanel != null) redAlertPanel.SetActive(false);
            if (timerSystem != null) timerSystem.StartTimer();
        }
    }

    void BeginFadeOut()
    {
        phase = Phase.FadeOut;
        phaseTimer = 0f;
        fadeStartIntensity = CurrentIntensity;
    }

    public void StartEarthquake()
    {
        phase = Phase.Waiting;
        phaseTimer = 0f;
        CurrentIntensity = 0f;
        completedAftershocks = 0;

        if (redAlertPanel != null) redAlertPanel.SetActive(false);
    }
}