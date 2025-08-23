using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public GameObject lobbyPanel, roomPanel;
    public TMP_InputField roomInput;
    public TextMeshProUGUI roomName, buttonText;

    public RoomItem roomItemPrefab;
    List<RoomItem> roomItemList = new List<RoomItem>();
    public Transform contentObject;

    public float timeBtwUpdates = 1.5f;
    float timeForNextUpdate;

    public PlayerItem playerItemPrefab, myPlayer;
    List<PlayerItem> playerItemList = new List<PlayerItem>();
    
    public Transform[] slots;
    public Transform temp;
    bool[] occupied;
    bool joinedRoom = false;

    public GameObject playButton, roomOptions;

    GameManager gm;

    void Start()
    {
        gm = GameManager.instance;
        PhotonNetwork.JoinLobby();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    //lobby stuff
    public void OnClickCreate()
    {
        if (roomInput.text.Length >= 1)
        {
            if (roomInput.text.Length >= 20)
            StartCoroutine(RoomNameTooLong());
            else
            {
                StartCoroutine(Creating());
                Invoke("NewRoom", 1f);
            }
        }
        else
        StartCoroutine(RoomNameTooShort());
    }

    void NewRoom()
    {
        PhotonNetwork.CreateRoom(roomInput.text,new RoomOptions(){MaxPlayers = 6, BroadcastPropsChangeToAll = true});
    }

    public void OnClickQuit()
    {
        if (!gm.isSingleplayer)
            PhotonNetwork.Disconnect();
            
        SceneManager.LoadScene("Menu");
    }

    IEnumerator Creating()
    {
        while(!joinedRoom)
        {
            buttonText.text = "Creating";
            //icon.sprite = frames[0];
            yield return new WaitForSeconds(0.2f);
            buttonText.text = "Creating.";
            //icon.sprite = frames[1];
            yield return new WaitForSeconds(0.2f);
            buttonText.text = "Creating..";
            //icon.sprite = frames[2];
            yield return new WaitForSeconds(0.2f);
            buttonText.text = "Creating...";
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator RoomNameTooShort()
    {
        buttonText.text = "Please enter a room name";
        yield return new WaitForSeconds(3f);
        buttonText.text = "Create Room";
    }

    IEnumerator RoomNameTooLong()
    {
        buttonText.text = "Your room name is too long";
        yield return new WaitForSeconds(3f);
        buttonText.text = "Create Room";
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (Time.time >= timeForNextUpdate)
        {
            UpdateRoomList(roomList);
            timeForNextUpdate = Time.time + timeBtwUpdates;
        }
    }

    void UpdateRoomList(List<RoomInfo> list)
    {
        foreach(RoomItem item in roomItemList)
        {
            Destroy(item.gameObject);
        }

        foreach(RoomInfo room in list)
        {
            if (!room.IsOpen || !room.IsVisible || room.RemovedFromList)
                continue;

            RoomItem newRoom = Instantiate(roomItemPrefab,contentObject);
            newRoom.SetRoomName(room.Name, room.PlayerCount);
            roomItemList.Add(newRoom);
        }
    }

    //room stuff
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        joinedRoom = true;

        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);
        roomName.text = PhotonNetwork.CurrentRoom.Name;
        if(!PhotonNetwork.IsMasterClient)
            roomOptions.SetActive(false);

        UpdatePlayerList();
        gm.ApplyRoomProperties(PhotonNetwork.CurrentRoom.CustomProperties);
    }

    public void OnClickStart()
    {
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel("Gameplay");
    }

    public void OnClickLeave()
    {
        gm.myID = 0;
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        joinedRoom = false;
        lobbyPanel.SetActive(true);
        roomPanel.SetActive(false);
        buttonText.text = "Create Room";
        roomInput.text = null;
    }

    void UpdatePlayerList()
    {
        foreach(PlayerItem item in playerItemList)
        Destroy(item.gameObject);

        playerItemList.Clear();

        if (PhotonNetwork.CurrentRoom == null)
        return;

        foreach(KeyValuePair<int,Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            PlayerItem newPlayer = Instantiate(playerItemPrefab, temp);
            newPlayer.SetPlayerInfo(player.Value);

            if (player.Value == PhotonNetwork.LocalPlayer)
            {
                myPlayer = newPlayer;
                newPlayer.ApplyLocalChanges();
            }
            playerItemList.Add(newPlayer);
        }
    }

    public void ChangeSlot(int ID)
    {
        string slotName = EventSystem.current.currentSelectedGameObject.name;
        foreach (PlayerItem player in playerItemList)
        {
            if (player.curSlot == slotName)
            {
                Debug.Log("This slot is occupied");
                return;
            }
        }
        myPlayer.gameObject.transform.SetParent(slots[ID],false);
        myPlayer.gameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
        myPlayer.curSlot = slotName;

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["ID"] = ID;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        gm.myID = ID;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount > 0)
            playButton.SetActive(true);
        else
            playButton.SetActive(false);
    }
}    