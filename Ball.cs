using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Ball : MonoBehaviour
{
    public static Ball instance;
    public Rigidbody rb;
    public PhotonView view;
    GameManager gm;

    public bool isDribbling;
    public float hostPing = 0f;
    
    public GameObject model, collidingEffect, ballHolder;
    public TrailRenderer trail;
    [SerializeField] LayerMask obstacle;

    float counter = 0f;
    bool count = false;
    Vector3 preVel;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if(instance != null && instance != this)
        {
            if (gm.isSingleplayer)
                Destroy(ballHolder);
            else if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.Destroy(ballHolder);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        view = GetComponent<PhotonView>();
        gm = GameManager.instance;

        isDribbling = false;
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
            view.RPC("UpdateHostPing", RpcTarget.All, gm.ping * 1f);

        if (rb.velocity.magnitude - preVel.magnitude > 1f)
        {
            //Debug.Log(counter);
            count = false;
            counter = 0f;
        }
        preVel = rb.velocity;

        if (count)
            counter += Time.deltaTime;
    }

    public IEnumerator ActivateKickBall(Vector3 kickDir, float delay, Vector3 foot)
    {
        if (Physics.Linecast(transform.position, foot, out RaycastHit hit, obstacle))
        {
            Debug.Log("Blocked!");
            yield break;
        }    

        float volume = 0.25f + kickDir.magnitude / 20f;
        if (volume > 1f) volume = 1f;
        SoundManager.PlaySound(SoundType.KICK, volume);

        if (gm.isSingleplayer) {
            yield return new WaitForSeconds(delay);
            isDribbling = false;
            rb.AddForce(kickDir, ForceMode.Impulse);
        }
        else {
            if (delay > 0f)
                count = true; // debugging

            float newDelay = delay;
            if (!PhotonNetwork.IsMasterClient) {

                newDelay = delay - (gm.ping + hostPing) / 1000f;
                if (newDelay < 0f)
                    newDelay = 0f;
            }

            // Debug.Log(newDelay);
            yield return new WaitForSeconds(newDelay);
            isDribbling = false;
            view.RPC("KickBall", RpcTarget.MasterClient, kickDir, false);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Barrier") && rb.velocity.magnitude > 2f)
        {
            float size = Mathf.Sqrt(rb.velocity.magnitude) * 0.3f;
        
            ContactPoint contact = collision.contacts[0];
            Vector3 position = contact.point;
            Quaternion rotation = collision.gameObject.transform.rotation;

            if (gm.isSingleplayer)
            {
                GameObject effect = Instantiate(collidingEffect,position,rotation);
                effect.transform.localScale = new Vector3(size, size, size);
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                view.RPC("SpawnCollidingEffect", RpcTarget.All, position, rotation, size);
            }
        }
    }

    [PunRPC]
    void KickBall(Vector3 kickDir, bool dribble)
    {
        isDribbling = dribble;
        if (gm.isSingleplayer || PhotonNetwork.IsMasterClient)
            rb.AddForce(kickDir, ForceMode.Impulse);
    }

    [PunRPC]
    void SpawnCollidingEffect(Vector3 pos, Quaternion rot, float size)
    {
        GameObject effect = Instantiate(collidingEffect, pos, rot);
        effect.transform.localScale = new Vector3(size, size, size);
    }

    [PunRPC]
    void UpdateHostPing(float value)
    {
        hostPing = value;
    }
    
    IEnumerator GetBallVelocity()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f);
            Debug.Log(rb.velocity.x + " " + rb.velocity.y + " " + rb.velocity.z);
        }
    }
}
