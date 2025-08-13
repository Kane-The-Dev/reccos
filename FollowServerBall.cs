using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FollowServerBall : MonoBehaviour
{
    [SerializeField]
    Ball serverBall;
    Rigidbody rb;
    GameManager gm;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gm = GameManager.instance;
    }

    void Update()
    {
        if (!serverBall)
        {
            StartCoroutine(TurnOff(0.5f));
            serverBall = FindObjectOfType<Ball>();
        }

        if (!gm.isSingleplayer && !PhotonNetwork.IsMasterClient && serverBall)
        {
            serverBall.model.SetActive(false);
            serverBall.trail.enabled = false;
            rb.velocity = Vector3.Lerp(rb.velocity, serverBall.rb.velocity, 20f * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, serverBall.transform.position, 20f * Time.deltaTime);
            rb.rotation = Quaternion.RotateTowards(rb.rotation, serverBall.rb.rotation, Time.fixedDeltaTime * 180f);
        }
        else if (serverBall)
        {
            serverBall.model.SetActive(true);
            serverBall.trail.enabled = true;
            Destroy(gameObject);
        }
    }

    public IEnumerator TurnOff(float time)
    {
        gameObject.SetActive(false);
        GetComponent<TrailRenderer>().enabled = false;
        yield return new WaitForSeconds(time);
        gameObject.SetActive(true);
        GetComponent<TrailRenderer>().enabled = true;
    }
}
