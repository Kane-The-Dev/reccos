using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Viewer : MonoBehaviour
{
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(ChangeAnimation());
    }

    IEnumerator ChangeAnimation()
    {
        while(true)
        {
            animator.speed = Random.Range(0.75f, 1.25f);
            animator.SetFloat("value", Random.Range(-1f, 1f));
            yield return new WaitForSeconds(Random.Range(5f, 10f));
        }
    }
}
