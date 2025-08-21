using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

public class PUSpawner : MonoBehaviour
{
    public GameObject[] powerUps;
    public bool[] available;
    public bool PUAllowed;
    GameManager gm;
    RaycastHit hit;
    public LayerMask whatIsPlayer;
    [Range(0f, 3f)]
    public float spawnRate; //power-up per second

    void Start()
    {
        gm = GameManager.instance;
        spawnRate = gm.PUSpawnRate;

        for(int i = 0; i < available.Length; i++)
        available[i] = gm.PUToggle[i];

        PUAllowed = false;
        foreach(bool a in available)
        {
            if (a) {
                PUAllowed = true;
                break;
            }
        }

        if(!PhotonNetwork.IsMasterClient) return;
        
        Invoke("SpawnStartingPU",0.2f);
        StartCoroutine(SpawnPowerUp());
    }

    public void UpdatePUList(bool enabled)
    {
        if (int.TryParse(EventSystem.current.currentSelectedGameObject.name, out int result))
        {
            int id = result;
            available[id] = enabled;
        }
            
        PUAllowed = false;
        foreach(bool a in available)
        {
            if (a) {
                PUAllowed = true;
                break;
            }
        }
    }

    Vector3 RandomPosition()
    {
        int posX = Random.Range(-25, 25);
        int posZ = Random.Range(-40, 40);
        Vector3 point = new Vector3(posX, 10f, posZ);

        Physics.Raycast(point, Vector3.down, out hit, whatIsPlayer);
        float posY = hit.point.y + 0.5f;

        return new Vector3(posX, posY, posZ);
    }

    private void SpawnStartingPU()
    {
        if (!PUAllowed || spawnRate <= 0f) return;

        int n = Random.Range(6, 9);
        for(int i = 0; i < n; i++)
        {
            int random = Random.Range(0, powerUps.Length);
            while(!available[random])
                random = Random.Range(0, powerUps.Length);
            
            Vector3 randomPosition = RandomPosition();

            if (gm.isSingleplayer)
                Instantiate(
                    powerUps[random], 
                    randomPosition, 
                    Quaternion.Euler(0f, Random.Range(0,360), 0f)
                );
            else if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.InstantiateRoomObject(
                    powerUps[random].name, 
                    randomPosition, 
                    Quaternion.Euler(0f, Random.Range(0,360), 0f)
                );
        }
    }
    
    IEnumerator SpawnPowerUp()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f / spawnRate);

            if (!PUAllowed || spawnRate <= 0f) yield break;
            
            int random = Random.Range(0, powerUps.Length);
            while(!available[random])
                random = Random.Range(0, powerUps.Length);

            Vector3 randomPosition = RandomPosition();

            if (gm.isSingleplayer)
                Instantiate(
                    powerUps[random], 
                    randomPosition, 
                    Quaternion.Euler(0f, Random.Range(0, 360), 0f)
                );
            else if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.InstantiateRoomObject(
                    powerUps[random].name, 
                    randomPosition, 
                    Quaternion.Euler(0f, Random.Range(0, 360), 0f)
                );
        }
    }
}
