using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    Animator animator;

    void Start()
    {
        animator = transform.GetChild(0).GetComponent<Animator>();
        animator.SetBool("isRunning", true);    
    }
}
