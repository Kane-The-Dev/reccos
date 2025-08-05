using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TeamManager : MonoBehaviour
{
    GameManager gm;
    PhotonView pv;
    PlayerSpawner ps;
    TeamManager[] TMs;
    public int myID;
    Transform spawnPoint;
    bool teamUpdated;

    void Start()
    {
        gm = GameManager.instance;
        pv = GetComponent<PhotonView>();
        ps = FindObjectOfType<PlayerSpawner>();
    }

    /*IEnumerator ArrangeTeam()
    {
        if (!gm.isSingleplayer && PhotonNetwork.IsMasterClient)
        {
            foreach(TeamManager tm in TMs)
            {
                tm.pv.RPC("GetTeamNumber", RpcTarget.MasterClient);
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    void Update()
    {
        if (myTeamNumber != 0 && !teamUpdated)
        {
            UpdateTeam();
            teamUpdated = true;
        }
    }

    void UpdateTeam()
    {
        int random = Random.Range(0,3);
        if (myTeamNumber == 1)
        {
            spawnPoint = ps.redTeamSpawnPoints[random];
            //gameObject.GetComponent<PlayerMovement>().defaultSkin = redTeam;
            //gameObject.GetComponent<PlayerMovement>().invisibleSkin = invisibleRed;
        }
        else
        {
            spawnPoint = ps.blueTeamSpawnPoints[random];
            //gameObject.GetComponent<PlayerMovement>().defaultSkin = blueTeam;
            //gameObject.GetComponent<PlayerMovement>().invisibleSkin = invisibleBlue;
        }
        transform.position = spawnPoint.position;
    }

    [PunRPC]
    void GetTeamNumber()
    {
        myTeamNumber = gm.nextTeamNumber;
        gm.UpdateTeam();
        pv.RPC("SendTeamNumber", RpcTarget.OthersBuffered, myTeamNumber);
    }

    [PunRPC]
    void SendTeamNumber(int teamNumber)
    {
        myTeamNumber = teamNumber;
    }*/
}
