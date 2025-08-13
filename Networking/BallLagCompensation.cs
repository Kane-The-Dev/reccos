using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BallLagCompensation : MonoBehaviour
{
    Rigidbody rb;
    PhotonView view;
    Vector3 networkPosition;
    Quaternion networkRotation;

    void Start()
    {
        rb=GetComponent<Rigidbody>();
        view=GetComponent<PhotonView>();
    }

    public void OnPhotonSerializeView(PhotonStream stream,PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rb.position);
            stream.SendNext(rb.rotation);
            stream.SendNext(rb.velocity);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            rb.velocity = (Vector3)stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            rb.position += rb.velocity * lag;
        }
    }

    void FixedUpdate()
    {
        if (!view.IsMine)
        {
            rb.position = Vector3.MoveTowards(rb.position,networkPosition,Time.fixedDeltaTime);
            rb.rotation = Quaternion.RotateTowards(rb.rotation,networkRotation,Time.fixedDeltaTime*100f);
        }
    }
}