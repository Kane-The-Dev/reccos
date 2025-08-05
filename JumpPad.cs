using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [Range(0.8f,1.2f)]
    public float strength;

    Vector3 direction;

    void Start()
    {
        direction = new Vector3(0f, 1.5f, 0f);
        direction += transform.right*-0.5f;
    }
    
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            Rigidbody rb = collider.gameObject.GetComponent<Rigidbody>();
            PlayerMovement pm = collider.gameObject.GetComponent<PlayerMovement>();
            
            rb.velocity = direction * 15f * strength;
            pm.movingAllowed = false;
            pm.jumpsLeft = 0;
            pm.isGrounded = false;
            pm.animator.SetTrigger("jump");
        }
        else if (collider.gameObject.CompareTag("Ball"))
        {
            Rigidbody rb = collider.gameObject.GetComponent<Rigidbody>();
            Ball ball = collider.gameObject.GetComponent<Ball>();
            
            rb.velocity = direction * 15f * strength;
            ball.isDribbling = false;
        }
    }
}
