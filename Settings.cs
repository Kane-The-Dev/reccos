using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class Settings : MonoBehaviourPunCallbacks
{
    GameManager gm;
    [SerializeField]
    GameObject spawner;

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

    public void ChangeCameraSensitivity(float value)
    {
        gm.camSensitivity = value;
    }

    public void ChangePUSpawnRate(float value)
    {
        spawner.GetComponent<PUSpawner>().spawnRate = value;
    }
}