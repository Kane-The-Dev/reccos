using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class PUVisual : MonoBehaviour
{
    Slider slider;
    PlayerMovement pm;
    public float duration;
    public int powerUpIndex;
    [SerializeField]
    TextMeshProUGUI stacks;
    GameManager gm;

    void Awake()
    {
        slider = GetComponent<Slider>();
        gm = FindObjectOfType<GameManager>();

        PUVisual[] PUVisuals = FindObjectsOfType<PUVisual>();
        foreach (PUVisual puv in PUVisuals)
        {
            if (puv.powerUpIndex == powerUpIndex && puv != this)
                Destroy(gameObject);
        }
        
        if (gm.isSingleplayer)
        pm = FindObjectOfType<PlayerMovement>();
        else
        {
            PlayerMovement[] PMs = FindObjectsOfType<PlayerMovement>();
            foreach (PlayerMovement PM in PMs)
            {
                if (PM.gameObject.GetComponent<PhotonView>().IsMine)
                pm = PM;
            }
        }
    }

    void Update()
    {
        if (pm != null)
        {
            slider.value = 1f - pm.durations[powerUpIndex] / duration;
            if (powerUpIndex < 3)
            {
                stacks.text = pm.PUStacks[powerUpIndex].ToString();
            }

            if (pm.durations[powerUpIndex] < 0.01f)
            Destroy(gameObject);
        }
    }
}
