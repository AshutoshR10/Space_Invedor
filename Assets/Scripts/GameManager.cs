using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Runtime.CompilerServices;
using System.Collections;
using TMPro;
using System.Net.Sockets;
using System.Text;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Existing UIs – note that gameOverUI is still used when lives reach 0.
    [SerializeField] private TextMeshProUGUI raiseHandPromptText;

    [SerializeField] private GameObject raiseHandPanel;
    private bool waitingForHand = false;

    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text hiscoreText;
    [SerializeField] private Text livesText;

    // New UI panels and buttons for level-based flow.
    [SerializeField] private GameObject startPanel;    // Shows "Press Spacebar to Start"
    [SerializeField] private GameObject restScreen;      // Level-end screen with buttons
    [SerializeField] private Button playAgainButton;     // Reloads current level
    [SerializeField] private Button nextLevelButton;       // Loads next level

    private Player player;
    private Invaders invaders;
    private MysteryShip mysteryShip;
    private Bunker[] bunkers;

    public int score { get; private set; } = 0;
    public int highScore { get; private set; } = 0;
    public int lives { get; private set; } = 50;

    // Level timer variables.
    private float levelTimer = 120f;
    private bool isLevelActive = false;   // True while the level is running
    private bool levelEnded = false;      // To ensure EndLevel() is called only once
    private static int count = 0;
    public static string patientID = "Unknown";
    private TcpClient client;
    private NetworkStream stream;
    private bool isConnected = false;
    private bool first = true;

    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
            StartCoroutine(GetPatientID());
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    [System.Serializable]
    private class PatientIDResponse
    {
        public string patient_id;
    }

    IEnumerator GetPatientID()
    {
        Debug.Log("Requesting Patient ID from Python...");
        UnityWebRequest request = UnityWebRequest.Get("http://127.0.0.1:5000/get_patient_id");

        yield return request.SendWebRequest();

        // Handle response
        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;
            patientID = JsonUtility.FromJson<PatientIDResponse>(jsonResponse).patient_id;
            PlayerPrefs.SetString("PatientID", patientID);
            PlayerPrefs.Save();
            Debug.Log("Patient ID received: " + patientID);
        }
        else
        {
            Debug.LogError("Failed to fetch Patient ID: " + request.error);
        }
    }

    void OnApplicationQuit()
    {
        // This will be called when the application is about to close
        Debug.Log("Application quitting - stopping recording");
        StopRecording();

        // Add a small delay to ensure message is sent before closing
        try
        {
            // Send a special QUIT command to notify Python that Unity is closing
            if (isConnected)
                SendCommand("STOP");

            // Give it a moment to send the message
            System.Threading.Thread.Sleep(100);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during quit: {e.Message}");
        }

        // Close connections
        if (stream != null)
            stream.Close();
        if (client != null)
            client.Close();
    }
    void ConnectToPython()
    {
        try
        {
            client = new TcpClient("localhost", 9999);
            stream = client.GetStream();
            isConnected = true;
            Debug.Log("Connected to Python script");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to connect to Python: {e.Message}");
        }
    }

    public void StartRecording()
    {
        // Call this method when level starts
        if (isConnected)
            SendCommand("START");
    }

    public void StopRecording()
    {
        // Call this method when level ends
        if (isConnected)
            SendCommand("STOP");
    }
    public void PauseRecording()
    {
        // Call this method when level starts
        if (isConnected)
            SendCommand("PAUSE");
    }
    public void ResumeRecording()
    {
        // Call this method when level starts
        if (isConnected)
            SendCommand("RESUME");
    }
    void SendCommand(string command)
    {
        if (stream != null)
        {
            byte[] data = Encoding.ASCII.GetBytes(command);
            stream.Write(data, 0, data.Length);
            Debug.Log($"Sent command: {command}");
        }
    }

    private void Start()
    {
        // if(count == 0){
        //     PlayerPrefs.SetInt("HasPlayedBefore", 0);
        //     PlayerPrefs.Save();
        // }
        ConnectToPython();
        player = Object.FindAnyObjectByType<Player>();
        invaders = Object.FindAnyObjectByType<Invaders>();
        mysteryShip = Object.FindAnyObjectByType<MysteryShip>();
        bunkers = Object.FindObjectsByType<Bunker>(FindObjectsSortMode.None);


        // Optionally hook up the button events (if not set via Inspector)
        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(PlayAgain);
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(NextLevel);

        // Begin a new game (which sets up the level and shows the start panel)
        NewGame();
        UpdateHiscore();
    }


    private void Update()
    {
        //  Game is idle at start screen
        if (!isLevelActive && startPanel.activeSelf && !waitingForHand)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Space))
            {
                // Hide start panel, show "Raise Hand" panel
                startPanel.SetActive(false);
                raiseHandPanel.SetActive(true);
                waitingForHand = true;
            }
        }

        //  Wait for hand to be raised
        else if (waitingForHand)
        {
            if (HandIsRaised())  // Replace with actual hand detection
            {
                if (first)
                {
                    StartRecording();
                    first = false;
                }
                raiseHandPanel.SetActive(false);
                waitingForHand = false;
                StartLevel();
            }
        }

        // Level running
        if (isLevelActive)
        {
            levelTimer -= Time.deltaTime;
            if (levelTimer <= 0f)
            {
                StopRecording();
                EndLevel();
            }
        }

        // Game Over screen logic
        if (lives <= 0 && Input.GetKeyDown(KeyCode.Space))
        {
            NewGame();
        }
    }
    private bool HandIsRaised()
    {
        return (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow));
    }


    private void NewGame()
    {
        // bool firstGame = PlayerPrefs.GetInt("HasPlayedBefore", 0) == 0;


        // Set the prompt text accordingly
        if (raiseHandPromptText != null)
        {
            if (count == 0)
            {
                // Debug.Log(firstGame);
                raiseHandPromptText.text = "Raise your healthy hand to begin!";
                // PlayerPrefs.SetInt("HasPlayedBefore", 1); // Mark as played
                count++;
            }
            else
            {
                raiseHandPromptText.text = "Raise your unhealthy hand to begin!";
            }
        }

        PlayerPrefs.Save();
        // Hide any end-game UI.
        if (gameOverUI != null)
            gameOverUI.SetActive(false);
        if (restScreen != null)
            restScreen.SetActive(false);

        // Show the start panel.
        if (startPanel != null)
            startPanel.SetActive(true);
        invaders.gameObject.SetActive(false);

        // Pause the game until the player presses Spacebar.
        Time.timeScale = 0;

        // Reset timer and flags.
        levelTimer = 120f;
        isLevelActive = false;
        levelEnded = false;

        // Reset score and lives.
        SetScore(0);
        SetLives(50);

        // Reset level objects (invaders, bunkers, player position, etc.).
        NewRound();
    }

    private void NewRound()
    {
        invaders.ResetInvaders();
        //invaders.gameObject.SetActive(true);

        for (int i = 0; i < bunkers.Length; i++)
        {
            bunkers[i].ResetBunker();
        }

        Respawn();
    }

  
    private void StartLevel()
    {
        if (startPanel != null)
            startPanel.SetActive(false);
        Time.timeScale = 1f;   // Resume game
        isLevelActive = true;
        invaders.gameObject.SetActive(true);
    }

    private void Respawn()
    {
        Vector3 position = player.transform.position;
        position.x = 0f;
        player.transform.position = position;
        player.gameObject.SetActive(true);
    }

    private void EndLevel()
    {
        if (levelEnded)
            return;
        StopRecording();
        levelEnded = true;
        isLevelActive = false;
        Time.timeScale = 0;   // Pause game
        if (restScreen != null)
            restScreen.SetActive(true);

        // Optionally, disable the invaders if they are still active.
        if (invaders != null)
            invaders.gameObject.SetActive(false);
        int level = SceneManager.GetActiveScene().buildIndex;
        SendGameData(patientID, level + 1, score);
        UpdateHiscore();
    }

    private void GameOver()
    {
        if (gameOverUI != null)
            gameOverUI.SetActive(true);
        if (invaders != null)
            invaders.gameObject.SetActive(false);
        int level = SceneManager.GetActiveScene().buildIndex;
        SendGameData(patientID, level + 1, score);
        UpdateHiscore();
    }

    private void SetScore(int score)
    {
        this.score = score;
        if (scoreText != null)
            scoreText.text = score.ToString().PadLeft(4, '0');
    }

    private void SetLives(int lives)
    {
        this.lives = Mathf.Max(lives, 0);
        if (livesText != null)
            livesText.text = this.lives.ToString();
    }

    public void OnPlayerKilled(Player player)
    {
        //SetLives(lives - 1);

        player.gameObject.SetActive(false);

        if (lives > 0)
        {
            // When the player dies but still has lives left, simply respawn.
            // (Note: the level timer is not reset.)
            if (isLevelActive)
                Invoke(nameof(Respawn), 1f);
        }
        else
        {
            GameOver();
        }
    }

    public void OnInvaderKilled(Invader invader)
    {
        invader.gameObject.SetActive(false);
        SetScore(score + invader.score);

        // If this was the last invader and the level is running, immediately end the level.
        if (invaders.GetAliveCount() == 0 && isLevelActive)
        {
            EndLevel();
        }
    }

    public void OnMysteryShipKilled(MysteryShip mysteryShip)
    {
        SetScore(score + mysteryShip.score);
    }

    public void OnBoundaryReached()
    {
        if (invaders.gameObject.activeSelf)
        {
            invaders.gameObject.SetActive(false);
            OnPlayerKilled(player);
        }
    }


    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void UpdateHiscore()
    {
        float hiscore = PlayerPrefs.GetFloat("hiscore", 0);
        if (score > hiscore)
        {
            hiscore = score;
            PlayerPrefs.SetFloat("hiscore", hiscore);
        }
        hiscoreText.text = "HI:" + Mathf.FloorToInt(hiscore).ToString("D4");
    }

    public void SendGameData(string patientID, int level, float score)
    {
        StartCoroutine(SendData(patientID, level, score));
    }

    IEnumerator SendData(string patientID, int level, float score)
    {
        // Create JSON payload
        string jsonData = $"{{\"patient_id\": \"{patientID}\", \"level\":{level},\"score\": {score}}}";

        // Convert to byte array
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        // Set up POST request
        using (UnityWebRequest request = new UnityWebRequest("http://127.0.0.1:5000/receive_data", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Send request
            yield return request.SendWebRequest();

            // Handle response
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Data sent successfully: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error sending data: " + request.error);
            }
        }
    }
}
