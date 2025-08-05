using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerItem : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI playerName;
    public string curSlot;

    [SerializeField]
    Image background;
    public Color highlightColor;
    public GameObject leftButton;
    public GameObject rightButton;

    ExitGames.Client.Photon.Hashtable playerProps = new ExitGames.Client.Photon.Hashtable();
    public int ID = 0;

    Player thisPlayer;
    LobbyManager lobby;

    void Start()
    {
        background = GetComponent<Image>();
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
                if (lobby.slots[i].childCount < 1)
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
}
