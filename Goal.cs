using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class Goal : MonoBehaviour
{
    [SerializeField] int goalID; // 0 red 1 blue

    int score;
    public int scoreToWin;
    public Text[] scoreDisplays;
    public TextMeshProUGUI scoreboard;

    Ball ball;
    Transform ballTransform;
    public GameObject ballPrefab;
    public Transform ballSpawnPoint;

    public GameObject goalEffect;
    string annoucement;
    public GameObject winningText;
    public TextMeshProUGUI scoringText;
    Vector3 target = new Vector3(-1400,0,0);

    GameManager gm;
    
    void Start()
    {
        gm = GameManager.instance;
        scoreToWin = gm.scoreToWin;
    }

    void Update()
    {
        if (ball == null)
        ball = Ball.instance;

        if (scoringText.rectTransform.position != target)
            scoringText.rectTransform.anchoredPosition = Vector3.MoveTowards(
                scoringText.rectTransform.anchoredPosition,
                target,
                600f * Time.deltaTime
            );
    }

    public void UpdateScoringText(string lastTouch, int lastTouchID)
    {
        if (lastTouchID % 2 != goalID)
            annoucement = lastTouch + " has scored a goal!";
        else
            annoucement = lastTouch + " scored an own goal...";
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
                Instantiate(goalEffect, ballTransform.position, Quaternion.identity);
                Destroy(ballTransform.parent.gameObject);
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Instantiate(goalEffect.name, ballTransform.position, Quaternion.identity);
                PhotonNetwork.Destroy(ballTransform.parent.gameObject);
            }

            if (score < scoreToWin)
            {
                Invoke("SpawnBall", 0.01f);
            }
            else
            {
                gm.gameEnded = true;
            } 
        }
    }

    void BallScored()
    {
        
    }
    
    void GameEnded()
    {

    }

    void SpawnBall()
    {
        if (gm.isSingleplayer)
            Instantiate(ballPrefab, ballSpawnPoint);
        else if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.InstantiateRoomObject(ballPrefab.name, ballSpawnPoint.position, Quaternion.identity);
    }

    [PunRPC]
    void ChangeScore()
    {
        FindObjectOfType<PlayerCamera>().Shake(2f, 0.5f);
        score++;
        foreach(Text text in scoreDisplays)
        text.text = score.ToString();
        scoreboard.text = score.ToString();
        if (ball) ballTransform = ball.gameObject.GetComponent<Transform>();
        scoringText.text = annoucement;
        scoringText.rectTransform.anchoredPosition = new Vector3(1400, 0, 0);
    }
}
