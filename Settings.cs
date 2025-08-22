using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Photon.Pun;
using TMPro;

public class Settings : MonoBehaviourPunCallbacks
{
    GameManager gm;
    [SerializeField] TextMeshProUGUI[] display;

    void Start()
    {
        gm = GameManager.instance;
    }

    public void OnClickQuit()
    {
        if (!gm.isSingleplayer)
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("Menu");
    }

    public void CloseSettings()
    {
        Cursor.lockState = CursorLockMode.Locked;
        gameObject.SetActive(false);
        gm.inSettings = false;
    }

    public void UpdateRoomProps()
    {
        gm.SetRoomProperties();
    }

    public void ChangeCameraSensitivity(float value)
    {
        gm.camSensitivity = value;
    }

    public void ChangeGameLength(float value)
    {
        gm.gameLength += value;
        if (gm.gameLength > 600f)
            gm.gameLength = 600f;
        else if (gm.gameLength < 180f)
            gm.gameLength = 180f;
        display[0].text = (gm.gameLength / 60f).ToString();
    }

    public void ChangeScoreToWin(int value)
    {
        gm.scoreToWin += value;
        if (gm.scoreToWin > 5) 
            gm.scoreToWin = 5;
        else if (gm.scoreToWin < 1) 
            gm.scoreToWin = 1;
        display[1].text = gm.scoreToWin.ToString();
    }

    public void ChangeDefaultSpeed(float value)
    {
        gm.defaultSpeed = value;
        display[2].text = value.ToString();
    }

    public void ChangeDefaultJumpForce(float value)
    {
        gm.defaultJumpForce = value;
        display[3].text = value.ToString();
    }

    public void ChangeDefaultKickForce(float value)
    {
        gm.defaultKickForce = value * 0.2f;
        display[4].text = (value * 0.2f).ToString("0.#");
    }

    public void ChangePUSpawnRate(float value)
    {
        gm.PUSpawnRate = value * 0.25f;
        display[5].text = (value * 0.25f).ToString("0.##");
    }

    public void UpdatePUList(bool enabled)
    {
        if (int.TryParse(EventSystem.current.currentSelectedGameObject.name, out int result))
        {
            int id = result;
            gm.PUToggle[id] = enabled;
        }
    }
}