using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;

public class PlayerItem : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI playerName;
    public string curSlot;

    [SerializeField]
    Image background;
    public Color highlightColor;
    [SerializeField] GameObject leftButton, rightButton, kickButton;

    ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable();
    public int ID = 0;

    private const byte KickEventCode = 1; // custom event ID

    Player thisPlayer;
    LobbyManager lobby;

    void Start()
    {
        background = GetComponent<Image>();
        if(!PhotonNetwork.IsMasterClient)
            kickButton.SetActive(false);
    }

    public void SetPlayerInfo(Player player)
    {
        if (player == null) return;

        if (lobby == null)
        lobby = FindObjectOfType<LobbyManager>();

        playerName.text = player.NickName;
        thisPlayer = player;

        if (player.CustomProperties.ContainsKey("ID"))
        UpdatePlayerItem(thisPlayer);
    }

    public void ApplyLocalChanges()
    {
        //leftButton.SetActive(true);
        //rightButton.SetActive(true);
        background.color = highlightColor;
        kickButton.SetActive(false);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable props)
    {
        if (thisPlayer == targetPlayer)
        {
            UpdatePlayerItem(targetPlayer);
        }
    }

    void UpdatePlayerItem(Player player)
    {
        if (player.CustomProperties.ContainsKey("ID"))
        {
            ID = (int)player.CustomProperties["ID"];
        }
        else if (player.IsLocal && !player.CustomProperties.ContainsKey("ID"))
        {
            for(int i = 0; i < lobby.slots.Length; i++)
            {
                if (lobby.slots[i].childCount < 3)
                {
                    ID = i;
                    playerProps["ID"] = ID;
                    PhotonNetwork.SetPlayerCustomProperties(playerProps);
                    break;
                }
            }
        }
        if (player.IsLocal)
        GameManager.instance.myID = ID;
        
        gameObject.transform.SetParent(lobby.slots[ID], false);
        gameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
        curSlot = lobby.slots[ID].gameObject.name;
    }

    //Only MasterClient can call
    public void KickPlayer()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        object content = null;
        RaiseEventOptions options = new RaiseEventOptions { TargetActors = new int[] { thisPlayer.ActorNumber } };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent(KickEventCode, content, options, sendOptions);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == KickEventCode)
        {
            Debug.Log("You were kicked by the host.");
            GameManager.instance.myID = 0;
            PhotonNetwork.LeaveRoom();
        }
    }
}
