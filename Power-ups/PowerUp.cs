using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PowerUp : MonoBehaviour
{
    [SerializeField]
    PlayerMovement pm;
    public float speedBuff, jumpForceBuff, kickForceBuff;
    public int powerUpIndex;
    public float duration;
    public GameObject powerUp, PUVisual, collectEffect;
    Transform powerUpHolder, player;
    PhotonView view;
    GameManager gm;

    void Start()
    {
        view = GetComponent<PhotonView>();
        gm = GameManager.instance;
        powerUpHolder = GameObject.FindWithTag("PUH").GetComponent<Transform>();

        if (gm.isSingleplayer)
            Instantiate(collectEffect, transform);
        else if (PhotonNetwork.IsMasterClient)
            view.RPC("SpawnEffectMultiplayer", RpcTarget.All);
    }

    [PunRPC]
    void SpawnEffectMultiplayer()
    {
        Instantiate(collectEffect, transform);
    }

    void Update()
    {
        if (player != null)
        {
            GetComponent<SphereCollider>().enabled = false;
            transform.parent.localScale = Vector3.Lerp(transform.parent.localScale, Vector3.zero, 10f * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            pm = collision.GetComponent<PlayerMovement>();
            player = pm.gameObject.transform;
            Invoke("OnCollected", 0.1f);
            Invoke("Disappear", duration + 0.2f);
        }
    }

    void OnCollected()
    {
        pm.GetPowerUp(powerUpIndex, duration);
        switch(powerUpIndex) {

            case 0: //swift steps
            if (pm.PUStacks[0] < 3)
            {
                pm.PUStacks[0]++;
                pm.speed += pm.defaultSpeed * speedBuff;
            }
            break;
        
            case 1: //spring boots
            if (pm.PUStacks[1] < 3)
            {
                pm.PUStacks[1]++;
                pm.jumpForce += pm.defaultJumpForce * jumpForceBuff;
            }
            break;
        
            case 2: // metal ankle
            if (pm.PUStacks[2] < 3)
            {
                pm.PUStacks[2]++;
                pm.kickForce += pm.defaultKickForce * kickForceBuff;
            }
            break;
        
            case 3: // aether leap
            pm.doubleJumpAllowed = true;
            pm.jumpsLeft = 2;
            break;
        
            case 4: // sneaky scorer
            pm.SpawnSmoke();
            if (pm.isInvisible != true)
            pm.speed += pm.defaultSpeed * speedBuff;
            pm.isInvisible = true;
            break;
        
            case 5: // aerial advantage
            pm.usingJetpack = true;
            pm.rb.velocity -= new Vector3(0f, pm.rb.velocity.y, 0f);
            pm.jetpackAnimator.SetBool("isClosed", false);
            break;

            case 6: // expert feet
            pm.dribbleCooldown = 0.3f;
            pm.dribbleRange = 4f;
            break;
        }
        
        GameObject puv = Instantiate(PUVisual, powerUpHolder);
        puv.GetComponent<PUVisual>().duration = duration;
    }

    void Disappear()
    {
        switch(powerUpIndex) {

            case 0: //swift steps
            if (pm.durations[0] <= 0f)
            {
                pm.speed -= pm.defaultSpeed * speedBuff * pm.PUStacks[0];
                pm.PUStacks[0] = 0;
            }
            break;
        
            case 1: //spring boots
            if (pm.durations[1] <= 0f)
            {
                pm.jumpForce -= pm.defaultJumpForce * jumpForceBuff * pm.PUStacks[1];
                pm.PUStacks[1] = 0;
            }
            break;
        
            case 2: // metal ankle
            if (pm.durations[2] <= 0f)
            {
                pm.kickForce -= pm.defaultKickForce * kickForceBuff * pm.PUStacks[2];
                pm.PUStacks[2] = 0;
            }
            break;
        
            case 3: // aether leap
            if (pm.durations[3] <= 0f)
            {
                pm.doubleJumpAllowed = false;
            }
            break;
        
            case 4: // sneaky scorer
            if (pm.durations[4] <= 0f)
            {
                pm.speed -= pm.defaultSpeed * speedBuff;
                pm.isInvisible = false;
                pm.jetpack.SetActive(true);
                pm.jetpackAnimator.SetBool("isClosed", true);
            }
            break;
        
            case 5: // aerial advantage
            if (pm.durations[5] < 0f)
            {
                pm.usingJetpack = false;
                pm.jetpackAnimator.SetBool("isClosed", true);
            }
            break;

            case 6: // expert feet
            if (pm.durations[6] < 0f)
            {
                pm.dribbleCooldown = pm.defaultDribbleCooldown;
                pm.dribbleRange = pm.defaultDribbleRange;
            }
            break;
        }

        if (gm.isSingleplayer)
            Destroy(powerUp);
        else if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(powerUp);
    }
}
