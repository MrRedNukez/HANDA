using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Pause menu for a mobile game: Resume / Options / Exit to Menu.
/// Attach to an empty GameObject (e.g. "PauseManager") in your gameplay scene.
/// </summary>
public class Pausemenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel;     // Holds Resume / Options / Exit buttons
    [SerializeField] private GameObject optionsPanel;   // Holds sliders + Back button

    [Header("Buttons")]
    [SerializeField] private Button pauseButton;        // The small "||" button shown during gameplay
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button exitToMenuButton;
    [SerializeField] private Button optionsBackButton;

    [Header("Options (optional)")]
    [SerializeField] private AudioMixer audioMixer;     // Expose params "MusicVolume" and "SFXVolume"
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private const string MusicKey = "MusicVolume";
    private const string SfxKey = "SFXVolume";

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        if (pauseButton != null) pauseButton.onClick.AddListener(Pause);
        resumeButton.onClick.AddListener(Resume);
        optionsButton.onClick.AddListener(OpenOptions);
        optionsBackButton.onClick.AddListener(CloseOptions);
        exitToMenuButton.onClick.AddListener(ExitToMenu);

        if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSfxVolume);
    }

    private void Start()
    {
        float music = PlayerPrefs.GetFloat(MusicKey, 0.75f);
        float sfx = PlayerPrefs.GetFloat(SfxKey, 0.75f);

        if (musicSlider != null) musicSlider.SetValueWithoutNotify(music);
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(sfx);
        ApplyVolume(MusicKey, music);
        ApplyVolume(SfxKey, sfx);

        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // Android back button/gesture maps to Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsPanel.activeSelf) CloseOptions();
            else if (IsPaused) Resume();
            else Pause();
        }
    }

    // Auto-pause when the player leaves the app (call, home button, etc.)
    private void OnApplicationPause(bool pausedByOS)
    {
        if (pausedByOS && !IsPaused) Pause();
    }

    public void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;
        Time.timeScale = 0f;
        if (pauseButton != null) pauseButton.gameObject.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        optionsPanel.SetActive(false);
        pausePanel.SetActive(false);
        if (pauseButton != null) pauseButton.gameObject.SetActive(true);
    }

    private void OpenOptions()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    private void CloseOptions()
    {
        PlayerPrefs.Save();
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void ExitToMenu()
    {
        // Always restore time before changing scenes
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat(MusicKey, value);
        ApplyVolume(MusicKey, value);
    }

    private void SetSfxVolume(float value)
    {
        PlayerPrefs.SetFloat(SfxKey, value);
        ApplyVolume(SfxKey, value);
    }

    // Slider 0..1 -> decibels (-80..0)
    private void ApplyVolume(string param, float value)
    {
        if (audioMixer == null) return;
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(param, dB);
    }

    private void OnDestroy()
    {
        // Safety net so the next scene never starts frozen
        Time.timeScale = 1f;
    }
}