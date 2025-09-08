//using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using ExitGames.Client.Photon;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;
    public bool isSingleplayer, shakeEnabled, gameEnded, inSettings = false;
    public float camSensitivity, gameLength, defaultSpeed, defaultJumpForce, defaultKickForce, PUSpawnRate, timeLeft;
    public int scoreToWin, dominantFoot, myID;
    public bool[] PUToggle;
    
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
        inSettings = false;
        timeLeft = 0f;
        myID = 0;

        //camSensitivity = 1f;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCamera = GameObject.FindWithTag("MainCamera")?.GetComponent<Transform>();
        FPSDisplay = GameObject.FindWithTag("FPS")?.GetComponent<TextMeshProUGUI>();

        if (scene.name == "Gameplay")
        {
            GameObject.FindWithTag("Spawner").GetComponent<PUSpawner>().spawnRate = PUSpawnRate;
            pingDisplay = GameObject.FindWithTag("Ping")?.GetComponent<TextMeshProUGUI>();
            timer = GameObject.FindWithTag("Time")?.GetComponent<TextMeshProUGUI>();
            timeLeft = gameLength + 3f;
        }

        if (scene.name == "Lobby")
        {
            regionDisplay = GameObject.FindWithTag("Region")?.GetComponent<TextMeshProUGUI>();
            gameLength = 300f;
            scoreToWin = 3;
            defaultSpeed = 8f; 
            defaultJumpForce = 12f; 
            defaultKickForce = 4.2f; 
            PUSpawnRate = 0.5f;
        }
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

        if (!gameEnded && timeLeft >= 0f)
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

    //sync properties
    public void SetRoomProperties()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Hashtable roomProps = new Hashtable
        {
            { "GameLength", gameLength },
            { "ScoreToWin", scoreToWin },
            { "DefaultSpeed", defaultSpeed },
            { "DefaultJumpForce", defaultJumpForce },
            { "DefaultKickForce", defaultKickForce },
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
    }

    public override void OnRoomPropertiesUpdate(Hashtable props)
    {
        ApplyRoomProperties(props);
    }

    public void ApplyRoomProperties(Hashtable props)
    {
        if (props == null) return;

        if (props.ContainsKey("GameLength"))
            gameLength = (float)props["GameLength"];

        if (props.ContainsKey("ScoreToWin"))
            scoreToWin = (int)props["ScoreToWin"];

        if (props.ContainsKey("DefaultSpeed"))
            defaultSpeed = (float)props["DefaultSpeed"];

        if (props.ContainsKey("DefaultJumpForce"))
            defaultJumpForce = (float)props["DefaultJumpForce"];

        if (props.ContainsKey("DefaultKickForce"))
            defaultKickForce = (float)props["DefaultKickForce"];
    }
}
