using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerFootstep : MonoBehaviour
{
    GameManager gm;
    PhotonView view;
    PlayerMovement pm;

    [SerializeField]
    GameObject runningEffect;
    [SerializeField]
    Transform leftFoot, rightFoot;

    void Start()
    {
        gm = GameManager.instance;
        view = GetComponent<PhotonView>();
        pm = transform.parent.gameObject.GetComponent<PlayerMovement>();
    }

    public void StepRight()
    {
        if (!pm.isGrounded) return;

        SoundManager.PlaySound(SoundType.RUN, 1);
        float angle = transform.parent.eulerAngles.y;
        if (gm.isSingleplayer)
            Instantiate(runningEffect, rightFoot.position, Quaternion.Euler(-90f, angle, 0f));
        else if (view.IsMine)
            PhotonNetwork.Instantiate(runningEffect.name, rightFoot.position, Quaternion.Euler(-90f, angle, 0f));
    }

    public void StepLeft()
    {
        if (!pm.isGrounded) return;
        
        SoundManager.PlaySound(SoundType.RUN, 1);
        float angle = transform.parent.eulerAngles.y;
        if (gm.isSingleplayer)
            Instantiate(runningEffect, leftFoot.position, Quaternion.Euler(-90f, angle, 0f));
        else if (view.IsMine)
            PhotonNetwork.Instantiate(runningEffect.name, leftFoot.position, Quaternion.Euler(-90f, angle, 0f));
    }
}
