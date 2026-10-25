
using System.Collections;
using UnityEngine;

public class HouseWeatherController : MonoBehaviour
{
    [Header("Weather Toggle")]
    public bool weatherEnabled = true;

    [Header("Rain")]
    [Tooltip("Particle System that creates the outdoor rain.")]
    public ParticleSystem rainParticleSystem;

    [Tooltip("Optional rain sound.")]
    public AudioSource rainAudio;

    [Range(0f, 2f)]
    public float rainVolume = 0.7f;

    [Header("Lightning")]
    [Tooltip("Directional or point light used for lightning flashes.")]
    public Light lightningLight;

    [Tooltip("Enable occasional lightning flashes.")]
    public bool lightningEnabled = true;

    [Tooltip("Minimum seconds between lightning flashes.")]
    public float minLightningInterval = 12f;

    [Tooltip("Maximum seconds between lightning flashes.")]
    public float maxLightningInterval = 25f;

    [Tooltip("Brightness of the lightning flash.")]
    [Range(0f, 8f)]
    public float lightningIntensity = 4f;

    [Tooltip("Duration of each flash.")]
    [Range(0.02f, 0.5f)]
    public float flashDuration = 0.12f;

    [Tooltip("Chance for a second, weaker flash.")]
    [Range(0f, 1f)]
    public float doubleFlashChance = 0.3f;

    [Tooltip("Optional lightning or thunder sound.")]
    public AudioSource thunderAudio;

    [Header("Optional Ambient Light Flash")]
    [Tooltip("Optional light that illuminates the outdoor environment.")]
    public Light ambientLightningLight;

    [Range(0f, 8f)]
    public float ambientFlashIntensity = 2f;

    private Coroutine weatherRoutine;
    private float originalLightningIntensity;
    private float originalAmbientIntensity;
    private float originalRainVolume;
    private bool originalLightningEnabled;
    private bool originalAmbientEnabled;
    private bool originalRainLoop;
    private bool originalThunderLoop;
    private bool settingsSaved;

    private void Awake()
    {
        SaveOriginalSettings();
        ApplyWeatherState();
    }

    private void OnEnable()
    {
        ApplyWeatherState();
    }

    private void OnDisable()
    {
        StopWeather();
    }

    private void OnValidate()
    {
        minLightningInterval = Mathf.Max(1f, minLightningInterval);
        maxLightningInterval = Mathf.Max(
            minLightningInterval,
            maxLightningInterval);
    }

    private void Update()
    {
        // Allows the Inspector toggle to be changed during Play mode.
        ApplyWeatherState();
    }

    private void SaveOriginalSettings()
    {
        if (settingsSaved) return;

        if (lightningLight != null)
        {
            originalLightningIntensity = lightningLight.intensity;
            originalLightningEnabled = lightningLight.enabled;
        }

        if (ambientLightningLight != null)
        {
            originalAmbientIntensity = ambientLightningLight.intensity;
            originalAmbientEnabled = ambientLightningLight.enabled;
        }

        if (rainAudio != null)
        {
            originalRainVolume = rainAudio.volume;
            originalRainLoop = rainAudio.loop;
        }

        if (thunderAudio != null)
        {
            originalThunderLoop = thunderAudio.loop;
        }

        settingsSaved = true;
    }

    private void ApplyWeatherState()
    {
        if (weatherEnabled)
        {
            if (rainParticleSystem != null &&
                !rainParticleSystem.isPlaying)
            {
                rainParticleSystem.Play();
            }

            if (rainAudio != null)
            {
                rainAudio.volume = rainVolume;

                if (!rainAudio.isPlaying)
                    rainAudio.Play();
            }

            if (weatherRoutine == null && isActiveAndEnabled)
            {
                weatherRoutine = StartCoroutine(WeatherLoop());
            }
        }
        else
        {
            StopWeather();
        }
    }

    private IEnumerator WeatherLoop()
    {
        while (weatherEnabled)
        {
            if (!lightningEnabled)
            {
                yield return null;
                continue;
            }

            float waitTime = Random.Range(
                minLightningInterval,
                maxLightningInterval);

            yield return new WaitForSeconds(waitTime);

            if (!weatherEnabled) break;
            if (!lightningEnabled) continue;

            yield return StartCoroutine(LightningFlash());

            if (weatherEnabled &&
                thunderAudio != null &&
                Random.value < 0.8f)
            {
                // Small delay between the flash and thunder.
                yield return new WaitForSeconds(
                    Random.Range(0.3f, 1.5f));

                if (weatherEnabled && thunderAudio != null)
                {
                    thunderAudio.Play();
                }
            }
        }

        weatherRoutine = null;
    }

    private IEnumerator LightningFlash()
    {
        if (lightningLight == null &&
            ambientLightningLight == null)
        {
            yield break;
        }

        int flashCount = Random.value < doubleFlashChance ? 2 : 1;

        for (int i = 0; i < flashCount; i++)
        {
            if (!weatherEnabled) yield break;

            SetLightning(true);

            yield return new WaitForSeconds(flashDuration);

            SetLightning(false);

            if (i < flashCount - 1)
            {
                yield return new WaitForSeconds(
                    Random.Range(0.08f, 0.2f));
            }
        }
    }

    private void SetLightning(bool on)
    {
        if (lightningLight != null)
        {
            lightningLight.enabled = on;

            lightningLight.intensity = on
                ? lightningIntensity
                : originalLightningIntensity;
        }

        if (ambientLightningLight != null)
        {
            ambientLightningLight.enabled = on;

            ambientLightningLight.intensity = on
                ? ambientFlashIntensity
                : originalAmbientIntensity;
        }
    }

    private void StopWeather()
    {
        if (weatherRoutine != null)
        {
            StopCoroutine(weatherRoutine);
            weatherRoutine = null;
        }

        if (rainParticleSystem != null)
        {
            rainParticleSystem.Stop(
                true,
                ParticleSystemStopBehavior.StopEmitting);
        }

        if (rainAudio != null)
        {
            rainAudio.Stop();
        }

        if (thunderAudio != null)
        {
            thunderAudio.Stop();
        }

        RestoreLightning();
    }

    private void RestoreLightning()
    {
        if (lightningLight != null)
        {
            lightningLight.intensity = originalLightningIntensity;
            lightningLight.enabled = originalLightningEnabled;
        }

        if (ambientLightningLight != null)
        {
            ambientLightningLight.intensity = originalAmbientIntensity;
            ambientLightningLight.enabled = originalAmbientEnabled;
        }
    }
}
