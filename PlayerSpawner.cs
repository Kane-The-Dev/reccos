using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    public Transform[] spawnPoints, seats;

    [SerializeField]
    GameObject[] playerPrefabs;
    [SerializeField]
    GameObject ballPrefab, viewerPrefab;
    [SerializeField]
    Transform ballSpawnPoint, stadium;

    GameManager gm;
    
    bool playerSpawned;

    void Start()
    {
        gm = GameManager.instance;
        
        foreach(Transform seat in seats)
        {
            int random = Random.Range(0,2);
            if (random > 0)
            {
                GameObject viewer = Instantiate(viewerPrefab,seat.position,Quaternion.identity);
                viewer.transform.parent = stadium;
            }
            
        }
        Invoke("SpawnPlayer",0.1f);
    }

    void SpawnPlayer()
    {
        if (gm.isSingleplayer)
        {   
            Instantiate(playerPrefabs[0], spawnPoints[0].position, Quaternion.identity);
            Instantiate(ballPrefab, ballSpawnPoint.position, Quaternion.identity);
        }
        else
        {
            //GameObject playerToSpawn = playerPrefabs[(int)PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"]];
            GameObject playerToSpawn = playerPrefabs[0];
            PhotonNetwork.Instantiate(playerToSpawn.name, spawnPoints[gm.myID].position, Quaternion.identity);
            PhotonNetwork.InstantiateRoomObject(ballPrefab.name, ballSpawnPoint.position, Quaternion.identity);
        }
    }

    
}
