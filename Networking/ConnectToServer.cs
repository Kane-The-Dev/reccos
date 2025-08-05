using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public TMP_InputField usernameInput;
    public TextMeshProUGUI buttonText;
    public Image icon;
    GameManager gm;
    public int regionID=0;
    [SerializeField]
    string[] regionList, regionNames;
    [SerializeField]
    Sprite[] frames;
    

    void Awake()
    {
        gm = GameManager.instance;
        if (SceneManager.GetActiveScene().name != "Gameplay")
        Cursor.lockState = CursorLockMode.None;
    }

    public void OnClickConnect()
    {
        if (usernameInput.text.Length >= 1)
        {
            if (usernameInput.text.Length >= 16)
            StartCoroutine(NicknameTooLong());
            else
            {
                //if (PhotonNetwork.Connected)
                //PhotonNetwork.Disconnect();

                StartCoroutine(Connecting());

                PhotonNetwork.NickName = usernameInput.text;
                gm.nickname = usernameInput.text;

                PhotonNetwork.AutomaticallySyncScene = true;
                PhotonNetwork.NetworkingClient.AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;

                gm.curRegion = regionNames[regionID];
                PhotonNetwork.ConnectToRegion(regionList[regionID]);     
            }
        }
        else
        StartCoroutine(NicknameTooShort());
    }

    public void SelectRegion(int i)
    {
        regionID = i;
    }

    public override void OnConnectedToMaster()
    {
        //Debug.Log(PhotonNetwork.CloudRegion);
        Invoke("LoadLobby", 1f);
    }

    void LoadLobby()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void OnClickSingleplayer()
    {
        gm.isSingleplayer = true;
        SceneManager.LoadScene("Gameplay");
    }

    IEnumerator Connecting()
    {
        while(true)
        {
            buttonText.text = "Connecting";
            icon.sprite = frames[0];
            yield return new WaitForSeconds(0.2f);
            buttonText.text = "Connecting.";
            icon.sprite = frames[1];
            yield return new WaitForSeconds(0.2f);
            buttonText.text = "Connecting..";
            icon.sprite = frames[2];
            yield return new WaitForSeconds(0.2f);
            buttonText.text = "Connecting...";
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator NicknameTooShort()
    {
        buttonText.text = "Please choose a name!";
        yield return new WaitForSeconds(2f);
        buttonText.text = "Connect";
    }

    IEnumerator NicknameTooLong()
    {
        buttonText.text = "Nickname too long!";
        yield return new WaitForSeconds(2f);
        buttonText.text = "Connect";
    }
}
