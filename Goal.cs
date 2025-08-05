using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class Goal : MonoBehaviour
{
    int score;
    public int scoreToWin;
    public Text[] scoreDisplays;
    public TextMeshProUGUI scoreboard;
    GameObject ball;
    Transform ballTransform;
    public GameObject ballPrefab;
    public Transform ballSpawnPoint;
    public GameObject goalEffect;
    public GameObject winningText;
    public GameObject scoringText;
    Animator sAnim;
    Animator wAnim;
    GameManager gm;
    
    void Start()
    {
        //sAnim=scoringText.GetComponent<Animator>();
        //wAnim=winningText.GetComponent<Animator>();
        gm = GameManager.instance;
    }

    void Update()
    {
        if (ball == null)
        ball = GameObject.FindWithTag("Ball");
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Ball"))
        {
            if (gm.isSingleplayer)
            {   
                ChangeScore();
            }
            else if (PhotonNetwork.IsMasterClient)
                GetComponent<PhotonView>().RPC("ChangeScore",RpcTarget.All);

            if (gm.isSingleplayer)
            {
                Instantiate(goalEffect,ballTransform.position,Quaternion.identity);
                Destroy(ballTransform.parent.gameObject);
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Instantiate(goalEffect.name,ballTransform.position,Quaternion.identity);
                PhotonNetwork.Destroy(ballTransform.parent.gameObject);
            }

            if (score < scoreToWin)
            {
                Invoke("SpawnBall",0.01f);
                //sAnim.SetBool("slideIn",true);
                //sAnim.SetBool("slideOut",false);
                //Invoke("BallScored",5f);
            }
            else
            {
                //wAnim.SetBool("slideIn",true);
                //wAnim.SetBool("slideOut",false);
                gm.gameEnded=true;
                //Invoke("GameEnded",5f);
            } 
        }
    }

    void BallScored()
    {
        sAnim.SetBool("slideOut",true);
        sAnim.SetBool("slideIn",false);
    }
    
    void GameEnded()
    {
        wAnim.SetBool("slideOut",true);
        wAnim.SetBool("slideIn",false);
    }

    void SpawnBall()
    {
        if (gm.isSingleplayer)
        Instantiate(ballPrefab,ballSpawnPoint);
        else
        PhotonNetwork.InstantiateRoomObject(ballPrefab.name,ballSpawnPoint.position,Quaternion.identity);
    }

    [PunRPC]
    void ChangeScore()
    {
        FindObjectOfType<PlayerCamera>().Shake(2f,0.5f);
        score++;
        foreach(Text text in scoreDisplays)
        text.text = score.ToString();
        scoreboard.text = score.ToString();
        ballTransform = ball.GetComponent<Transform>();
    }
}
