using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomItem : MonoBehaviour
{
    public TextMeshProUGUI roomName;
    LobbyManager manager;

    void Start()
    {
        manager = FindObjectOfType<LobbyManager>();
    }

    public void SetRoomName(string name)
    {
        roomName.text = name;
    }

    public void OnClickRoomItem()
    {
        manager.JoinRoom(roomName.text);
    }
}
