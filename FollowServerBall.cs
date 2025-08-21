using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FollowServerBall : MonoBehaviour
{
    [SerializeField] Ball serverBall;
    [SerializeField] GameObject model;
    Rigidbody rb;
    GameManager gm;
    bool isFinding;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gm = GameManager.instance;
        isFinding = false;
    }

    void Update()
    {
        if (serverBall == null)
        {
            serverBall = FindObjectOfType<Ball>();
            if(!isFinding)
            {
                StartCoroutine(TurnOff(0.5f));
                isFinding = true;
            } 
        }
        else isFinding = false;

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
        model.SetActive(false);
        GetComponent<TrailRenderer>().enabled = false;
        yield return new WaitForSeconds(time);
        model.SetActive(true);
        GetComponent<TrailRenderer>().enabled = true;
    }
}
