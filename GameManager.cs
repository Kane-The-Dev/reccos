using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool isSingleplayer, gameEnded, inSettings = false;
    public float camSensitivity, timeLeft;
    public int nextTeamNumber, dominantFoot, myID;
    
    public Transform mainCamera;
    public string nickname, curRegion;
    [SerializeField]
    TextMeshProUGUI pingDisplay, regionDisplay, timer, FPSDisplay;
    float deltaTime = 0f;
    
    public int ping;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
            instance = this;
        else if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        //PhotonNetwork.EnableCloseConnection = true;
        Screen.SetResolution(1500, 900, false);        
        camSensitivity = 1f;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCamera = GameObject.FindWithTag("MainCamera")?.GetComponent<Transform>();
        FPSDisplay = GameObject.FindWithTag("FPS")?.GetComponent<TextMeshProUGUI>();

        if (scene.name == "Gameplay")
        {
            pingDisplay = GameObject.FindWithTag("Ping")?.GetComponent<TextMeshProUGUI>();
            timer = GameObject.FindWithTag("Time")?.GetComponent<TextMeshProUGUI>();
            timeLeft = 303f;
        }

        if (scene.name == "Lobby")
            regionDisplay = GameObject.FindWithTag("Region")?.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (!isSingleplayer)
        ping = PhotonNetwork.GetPing();
        else
        ping = 0;
        
        if (pingDisplay != null)
            pingDisplay.text = "Ping: " + ping + "ms";

        if (FPSDisplay != null)
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            FPSDisplay.text = Mathf.Ceil(fps).ToString() + " FPS";
        }
            
        if (regionDisplay != null)
            regionDisplay.text = "Region: " + curRegion;

        if (timer != null && timeLeft >= 0f)
            timer.text = FormatTime(timeLeft);

        if (!gameEnded)
        {
            timeLeft -= Time.deltaTime;
        }
    }

    public string FormatTime(float totalSeconds)
    {
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
