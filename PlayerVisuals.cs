using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerVisuals : MonoBehaviour
{
    [SerializeField]
    Material redSkin, blueSkin, invisibleRed, invisibleBlue, redJet, blueJet;
    [SerializeField]
    SkinnedMeshRenderer robot;
    [SerializeField]
    MeshRenderer[] jetpackParts;
    [SerializeField]
    GameObject jetpack;

    PhotonView view;

    void Awake() 
    {
        view = GetComponent<PhotonView>();
    }
    
    public void ChangeSkin(string color)
    {
        Change(color);
        if (!GameManager.instance.isSingleplayer)
        {
            if (color == "red invisible" || color == "blue invisible")
            view.RPC("Change", RpcTarget.Others, "invisible");
            else
            view.RPC("Change", RpcTarget.Others, color);
        }
    }

    [PunRPC]
    void Change(string color)
    {
        //Debug.Log(color);
        if (color == "red")
        {
            robot.gameObject.SetActive(true);
            robot.material = redSkin;
            jetpack.SetActive(true);
            foreach(MeshRenderer part in jetpackParts)
            {
                part.material = redJet;
            }
        }
        else if (color == "blue")
        {
            robot.gameObject.SetActive(true);
            robot.material = blueSkin;
            jetpack.SetActive(true);
            foreach(MeshRenderer part in jetpackParts)
            {
                part.material = blueJet;
            }
        }
        else if (color == "red invisible")
        {
            robot.material = invisibleRed;
            jetpack.SetActive(false);
        }
        else if (color == "blue invisible")
        {
            robot.material = invisibleBlue;
            jetpack.SetActive(false);
        }
        else
        {
            robot.gameObject.SetActive(false);
            jetpack.SetActive(false);
        }
    }
}
