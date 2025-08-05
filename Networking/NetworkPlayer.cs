using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviour,IPunObservable
{
    PlayerMovement player;
    PhotonView pv;
    Vector3 RemotePlayerPosition;
    float remoteMovementX,remoteMovementZ;

    private void Awake()
    {
        player = GetComponent<PlayerMovement>();
        pv = GetComponent<PhotonView>();
    }

    public void Update()
    {
        if (pv.IsMine)
        return;

        var LagDistance=RemotePlayerPosition - transform.position;
        LagDistance.y=0;

        if (LagDistance.magnitude>5f)
        {
            transform.position=RemotePlayerPosition;
            LagDistance=Vector3.zero;
        }

        if (LagDistance.magnitude<=0.1f)
        {
            player.movementX=0;
            player.movementZ=0;
        }
        else
        {
            player.movementX=LagDistance.normalized.x;
            player.movementZ=LagDistance.normalized.z;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(player.movementX);
            stream.SendNext(player.movementZ);
        }
        else
        {
            RemotePlayerPosition=(Vector3)stream.ReceiveNext();
            remoteMovementX=(float)stream.ReceiveNext();
            remoteMovementZ=(float)stream.ReceiveNext();
        }
    }
}
