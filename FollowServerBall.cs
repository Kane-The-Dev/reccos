using UnityEngine;
using Photon.Pun;

public class FollowServerBall : MonoBehaviour
{
    Ball serverBall;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!serverBall)
            serverBall = FindObjectOfType<Ball>();

        if (!PhotonNetwork.IsMasterClient && serverBall)
        {
            serverBall.model.SetActive(false);
            rb.velocity = Vector3.Lerp(rb.velocity, serverBall.rb.velocity, 20f * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, serverBall.transform.position, 20f * Time.deltaTime);
            rb.rotation = Quaternion.RotateTowards(rb.rotation, serverBall.rb.rotation, Time.fixedDeltaTime * 180f);
        }
        else if (serverBall)
        {
            serverBall.model.SetActive(true);
            Destroy(gameObject);
        }
    }
}
