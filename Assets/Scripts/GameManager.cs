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

    // Event other systems can subscribe to (optional)
    public event Action OnPlayerDiedEvent;

    private float currentTimer;
    private bool timerRunning;

    // Support both player script types: PlayerControls (your new type) or WormSlingshot3D (older player)
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
        // show initial lives value
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
                // Not a timed level
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

        // Give the player their starting lives (if found)
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
            // Prefer PlayerControls, fallback to WormSlingshot3D
            if (playerControls != null)
            {
                playerControls.KillByTimer(); // kills player (Lives = 0)
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
        // Additional behaviors on time up could be added here
    }

    /// <summary>
    /// Called by the player when they die (or by other systems). Stops timer and optionally loads GameOver.
    /// Players should call GameManager.Instance?.OnPlayerDied() when they die.
    /// </summary>
    public void OnPlayerDied()
    {
        Debug.Log("GameManager: Player died notification received.");
        // Stop timer so it doesn't keep counting negative etc.
        timerRunning = false;

        // Broadcast to any listeners
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

    // Formats seconds to MM:SS
    private string FormatTime(float seconds)
    {
        seconds = Mathf.Max(0f, seconds);
        int mins = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{mins}:{secs:00}";
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
