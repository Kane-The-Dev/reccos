using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    GameManager gm;

    void Start()
    {
        gm = GameManager.instance;
    }

    void Update()
    {
        transform.LookAt(2 * transform.position - gm.mainCamera.position);
    }
}
