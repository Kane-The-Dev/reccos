using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomItem : MonoBehaviour
{
    public TextMeshProUGUI roomName, playerCount;
    LobbyManager manager;

    void Start()
    {
        manager = FindObjectOfType<LobbyManager>();
    }

    public void SetRoomName(string name, int count)
    {
        roomName.text = name;
        playerCount.text = count + "/6";
    }

    public void OnClickRoomItem()
    {
        manager.JoinRoom(roomName.text);
    }
}
