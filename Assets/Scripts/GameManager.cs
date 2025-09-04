using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI References (TextMeshPro)")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text livesText;

    [Header("Timers per Level (build indices)")]
    [SerializeField] private float level2Time = 60f;
    [SerializeField] private float level3Time = 90f;
    [SerializeField] private float level4Time = 120f;

    [Header("Lives per Level")]
    [SerializeField] private int defaultLives = 3;

    [Header("Optional behaviour")]
    [Tooltip("If true, the GameManager will call player.KillByTimer() when the level timer hits 0.")]
    [SerializeField] private bool killPlayerOnTimeUp = true;

    [Tooltip("If true, GameManager will load a Game Over scene after the player dies.")]
    [SerializeField] private bool loadGameOverSceneOnDeath = false;
    [SerializeField] private string gameOverSceneName = "GameOver";
    [SerializeField] private float delayBeforeGameOver = 1f;

    [Header("Music per Level")]
    [Tooltip("Assign 1 music track per level build index (same order as in Build Settings). Leave empty if no music.")]
    [SerializeField] private AudioClip level1Music;
    [SerializeField] private AudioClip level2Music;
    [SerializeField] private AudioClip level3Music;
    [SerializeField] private AudioClip level4Music;

    private AudioSource musicSource;

    // Event other systems can subscribe to (optional)
    public event Action OnPlayerDiedEvent;

    private float currentTimer;
    private bool timerRunning;

    private PlayerControls playerControls;
    private PlayerControls wormPlayer;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Setup music source
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        FindPlayerRefs();
        if (playerControls != null)
            UpdateLivesUI(playerControls.Lives);
        else if (wormPlayer != null)
            UpdateLivesUI(wormPlayer.Lives);
        else
            UpdateLivesUI(defaultLives);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayerRefs();

        // Handle music
        PlayLevelMusic(scene.buildIndex);

        // Handle timers
        switch (scene.buildIndex)
        {
            case 2:
                StartLevel(level2Time);
                break;
            case 3:
                StartLevel(level3Time);
                break;
            case 4:
                StartLevel(level4Time);
                break;
            default:
                timerRunning = false;
                currentTimer = 0f;
                UpdateTimerUI();
                break;
        }
    }

    private void FindPlayerRefs()
    {
        playerControls = FindObjectOfType<PlayerControls>();
    }

    private void StartLevel(float timeLimit)
    {
        currentTimer = timeLimit;
        timerRunning = true;

        FindPlayerRefs();

        if (playerControls != null)
        {
            playerControls.Lives = defaultLives;
            UpdateLivesUI(playerControls.Lives);
        }
        else if (wormPlayer != null)
        {
            wormPlayer.Lives = defaultLives;
            UpdateLivesUI(wormPlayer.Lives);
        }
        else
        {
            UpdateLivesUI(defaultLives);
        }

        UpdateTimerUI();
    }

    private void Update()
    {
        if (!timerRunning) return;

        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0f)
        {
            currentTimer = 0f;
            timerRunning = false;
            TimeUp();
        }

        UpdateTimerUI();
    }

    private void TimeUp()
    {
        Debug.Log("GameManager: Level timer expired.");
        if (killPlayerOnTimeUp)
        {
            if (playerControls != null)
            {
                playerControls.KillByTimer();
            }
            else if (wormPlayer != null)
            {
                wormPlayer.KillByTimer();
            }
            else
            {
                Debug.LogWarning("GameManager: No player found to kill when timer expired.");
            }
        }
    }

    public void OnPlayerDied()
    {
        Debug.Log("GameManager: Player died notification received.");
        timerRunning = false;
        OnPlayerDiedEvent?.Invoke();

        if (loadGameOverSceneOnDeath)
            StartCoroutine(LoadGameOverDelayed());
    }

    private IEnumerator LoadGameOverDelayed()
    {
        yield return new WaitForSeconds(delayBeforeGameOver);

        if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            Debug.LogWarning("GameManager: gameOverSceneName is empty, cannot load Game Over scene.");
        }
    }

    public void UpdateLivesUI(int lives)
    {
        if (livesText != null)
            livesText.text = $"Lives: {lives}";
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = $"Time: {FormatTime(currentTimer)}";
    }

    private string FormatTime(float seconds)
    {
        seconds = Mathf.Max(0f, seconds);
        int mins = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{mins}:{secs:00}";
    }

    private void PlayLevelMusic(int buildIndex)
    {
        AudioClip clipToPlay = null;

        switch (buildIndex)
        {
            case 1: clipToPlay = level1Music; break;
            case 2: clipToPlay = level2Music; break;
            case 3: clipToPlay = level3Music; break;
            case 4: clipToPlay = level4Music; break;
        }

        if (clipToPlay != null && musicSource.clip != clipToPlay)
        {
            musicSource.clip = clipToPlay;
            musicSource.Play();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
