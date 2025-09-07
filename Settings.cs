using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Photon.Pun;
using TMPro;

public class Settings : MonoBehaviourPunCallbacks
{
    GameManager gm;
    [SerializeField] TextMeshProUGUI[] display;
    [Header("Audio")]
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider SFXSlider;
    [Header("Graphic")]
    [SerializeField] Slider brightSlider;
    [SerializeField] Toggle postProToggle;
    [SerializeField] Toggle shakeToggle;
    [Header("Control")]
    [SerializeField] Slider camSenSlider;

    void Start()
    {
        gm = GameManager.instance;

        if (musicSlider != null) musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        if (SFXSlider != null) SFXSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        
        if (brightSlider != null) brightSlider.value = PlayerPrefs.GetFloat("Brightness", 1f);
        if (postProToggle != null) postProToggle.isOn = PlayerPrefs.GetInt("PostProcessing", 1) == 1;
        if (shakeToggle != null) shakeToggle.isOn = PlayerPrefs.GetInt("Screenshake", 1) == 1;

        if (camSenSlider != null) camSenSlider.value = PlayerPrefs.GetFloat("CameraSensitivity", 1f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }

    public void OnClickQuit()
    {
        if (!gm.isSingleplayer)
        PhotonNetwork.Disconnect();
        gm.inSettings = false;
        SceneManager.LoadScene("Menu");
    }

    public void CloseSettings()
    {
        if(SceneManager.GetActiveScene().name == "Gameplay")
        Cursor.lockState = CursorLockMode.Locked;
        gm.inSettings = false;
        gameObject.SetActive(false);
    }

    public void ChangeMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }

    public void ChangeSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }

    public void ChangeCameraSensitivity(float value)
    {
        gm.camSensitivity = value;
        PlayerPrefs.SetFloat("CameraSensitivity", value);
        PlayerPrefs.Save();
    }

    public void ChangeBrightness(float value)
    {
        PlayerPrefs.SetFloat("Brightness", value);
        PlayerPrefs.Save();
    }

    public void ChangePostProcessing(bool value)
    {
        PlayerPrefs.SetInt("PostProcessing", value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ChangeScreenshake(bool value)
    {
        gm.shakeEnabled = value;
        PlayerPrefs.SetInt("Screenshake", value ? 1 : 0);
        PlayerPrefs.Save();
    }

    //room customization
    public void UpdateRoomProps()
    {
        gm.SetRoomProperties();
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
        gm.PUSpawnRate = value * 0.1f;
        display[5].text = (value * 0.1f).ToString("0.#");
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