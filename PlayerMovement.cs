using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;

    [Header("Movement")]
    public float speed;
    public float defaultSpeed;
    public float movementX,movementZ;
    public bool movingAllowed;
    
    [Header("Jumping")]
    public float jumpForce;
    public float defaultJumpForce;
    public bool isGrounded, doubleJumpAllowed;
    public int jumpsLeft;

    [Header("Ball Kicking")]
    public float kickForce;
    public float defaultKickForce;
    public Transform left, right;
    float sideIndicator, ballDistance, startCharging = 0f;
    Vector3 kickDirection;
    Transform ballTransform;
    Ball ball;
    ChargeBar chargeBar;
    [Range(-1,1)]
    public int dominantFoot;

    [Header("Jetpack")]
    public float jetpackForce;
    public bool usingJetpack = false;
    float myTime, nextEmission = 0.05f, timeBetweenEmission = 0.05f;
    public Transform jetpackPoint1, jetpackPoint2;
    public GameObject jetpack, fireEffect;
    public Animator jetpackAnimator;

    [Header("Wall Climbing")]
    public bool isClimbing;
    float start = 0f, interval1 = 0.52f, interval2 = 0.69f, delay = 0.51f;
    public Vector3 targetPosition, targetRotation, nextPosition;
    [SerializeField]
    Transform head, target;
    [SerializeField]
    LayerMask obstacle;
    [SerializeField]
    GameObject point;

    [Header("Dribbling")]
    public bool isDribbling;
    float dribblingTimer;
    public Transform dribblePoint, dribbleHitbox;
    public float dribbleCooldown, dribbleRange;
    public float defaultDribbleCooldown, defaultDribbleRange;

    [Header("Invisibility")]
    public bool isInvisible;
    public GameObject invisibleSmoke;

    [Header("Collide n' Slide")]
    int maxBounces = 3;
    Bounds bounds;
    [SerializeField]
    private CapsuleCollider cs;
    [SerializeField]
    private LayerMask whatIsWall;

    [Header("Camera")]
    public float turnSmoothTime = 1f;
    float turnSmoothVelocity;
    Transform cam, pt;
    PlayerCamera pc;

    [Header("Particle Effects")]
    public ParticleSystemForceField psff;
    public GameObject landingEffect1, landingEffect2;
    [SerializeField]
    Transform effectSpawnPoint;
    public float inAirTime = 0.5f;

    [Header("Animation")]
    public GameObject model;
    public Animator animator;

    [Header("Other")]
    GameManager gm;
    public PlayerVisuals visual;
    public PhotonView view;
    public float[] durations;
    public float[] PUStacks;
    public TextMeshPro nameTag;
    
    void Start()
    {
        visual = GetComponent<PlayerVisuals>();
        cam = GameObject.FindWithTag("MainCamera").GetComponent<Transform>();
        view = GetComponent<PhotonView>();
        gm = GameManager.instance;
        
        pt = GetComponent<Transform>();
        pc = FindObjectOfType<PlayerCamera>();

        speed = defaultSpeed;
        jumpForce = defaultJumpForce;
        kickForce = defaultKickForce;
        dribbleCooldown = defaultDribbleCooldown;
        dribbleRange = defaultDribbleRange;

        jetpackAnimator.SetBool("isClosed", true);
        dominantFoot = gm.dominantFoot;

        chargeBar = FindObjectOfType<ChargeBar>();

        bounds = cs.bounds;
        bounds.Expand(-2 * 0.01f);

        Invoke("Initialize",0.5f);
    }

    void Initialize()
    {     
        if (view.IsMine||gm.isSingleplayer)
        {   
            if (gm.myID % 2 == 0) // 0 red 1 blue
                visual.ChangeSkin("red");
            else
                visual.ChangeSkin("blue");

            dribblePoint.gameObject.SetActive(true);
            dribbleHitbox.gameObject.SetActive(true);

            view.RPC("SetNameTag", RpcTarget.All, gm.nickname);

            pc.FollowPlayer(pt);
        }
    }

    [PunRPC]
    void SetNameTag(string nickname)
    {
        nameTag.text = nickname;
        if (view.IsMine)
        nameTag.color = Color.green;
    }

    public void GetPowerUp(int powerUpIndex, float duration)
    {
        durations[powerUpIndex] = duration;
    }
    
    void Update()
    {
        if ((view.IsMine || gm.isSingleplayer))
        {
            if (!isGrounded)
            {
                inAirTime += Time.deltaTime;
                if (rb.velocity.y < 0 && !usingJetpack)
                    rb.velocity += Physics.gravity * 0.5f * Time.deltaTime; // faster fall = better jump
            }

            if (!gm.gameEnded)
            {
                if (ball == null)
                    ball = FindObjectOfType<Ball>();
                else
                    ballTransform = ball.gameObject.GetComponent<Transform>();
            }
            
            //moving
            Vector3 direction = Vector3.zero;
            if (!gm.inSettings)
            {
                movementX = Input.GetAxisRaw("Horizontal");
                movementZ = Input.GetAxisRaw("Vertical");
                direction = new Vector3(movementX, 0f, movementZ).normalized;
            }
            
            pc.SpeedUp(new Vector2(rb.velocity.x, rb.velocity.z).magnitude);

            if (direction.magnitude > 0.1f)
            {
                float targetAngle = cam.eulerAngles.y;
                float moveAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + targetAngle;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, moveAngle, ref turnSmoothVelocity, turnSmoothTime);
                
                if (movingAllowed && !isClimbing)
                {
                    animator.SetBool("isClimbing", false);
                    animator.SetBool("isRunning", true);

                    Vector3 moveDirection = Quaternion.Euler(0f,angle,0f) * Vector3.forward;
                    Vector3 finalMoveDirection = CollideAndSlide(moveDirection, transform.position, 0);

                    transform.rotation = Quaternion.Euler(transform.eulerAngles.x, angle, transform.eulerAngles.z);
                    rb.velocity = new Vector3(finalMoveDirection.x * speed, rb.velocity.y, finalMoveDirection.z * speed);
                }
                else if (isClimbing) // climbing
                {
                    if (start <= 0f)
                    start = Time.time;
                    animator.SetInteger("climbDirection", 0);

                    if (movementZ > 0) animator.SetInteger("climbDirection", 1);
                    else if (movementZ < 0) animator.SetInteger("climbDirection", 3);

                    if (movementX > 0) animator.SetInteger("climbDirection", 2);
                    else if (movementX < 0) animator.SetInteger("climbDirection", 4);

                    float elapsed = Time.time - start;
                    Vector2 gap = Vector2.zero;
                    
                    if (Mathf.Floor((elapsed - delay) / interval2) != Mathf.Floor(((elapsed - delay) - Time.deltaTime) / interval2))
                        gap += new Vector2(2f * movementX, 0f);
                    
                    if (movementX != 0f && movementZ != 0f)
                    {
                        if (Mathf.Floor((elapsed - delay) / interval2) != Mathf.Floor(((elapsed - delay) - Time.deltaTime) / interval2))
                        gap += new Vector2(0f, 1.25f * movementZ);
                    }
                    else
                    {
                        if (Mathf.Floor(elapsed / interval1) != Mathf.Floor((elapsed - Time.deltaTime) / interval1))
                        gap += new Vector2(0f, 1.25f * movementZ);
                    }
                    

                    if (gap != Vector2.zero)
                        nextPosition = transform.position + transform.TransformDirection(gap) * 1f;

                    if (nextPosition != Vector3.zero)
                        transform.position = Vector3.Lerp(transform.position, nextPosition, 4f * Time.deltaTime);
                }

                /*if (transform.position.y < 2.8f)
                {
                    if (gm.isSingleplayer)
                    Instantiate(runningEffect1,effectSpawnPoint.position,Quaternion.Euler(0f,moveAngle+180f,0f));
                    else if (!gm.isSingleplayer)
                    PhotonNetwork.Instantiate(runningEffect1.name,effectSpawnPoint.position,Quaternion.Euler(0f,moveAngle+180f,0f));
                }*/
            }
            else
            {
                if (isClimbing)
                {
                    start = 0f;
                    animator.SetBool("isClimbing", true);
                    animator.SetInteger("climbDirection", 0);
                }

                animator.SetBool("isRunning", false);
                if (movingAllowed)
                    rb.velocity = new Vector3(rb.velocity.x * 0.95f, rb.velocity.y, rb.velocity.z * 0.95f);
            }
            
            //dribbling
            if (rb.velocity.magnitude < 0f || !isGrounded)
                isDribbling = false;
            

            if (isDribbling && ball)
            {
                float ballDistance = Mathf.Min(Vector3.Distance(ballTransform.position, left.position), Vector3.Distance(ballTransform.position, right.position));
                if (ballDistance > dribbleRange)
                    isDribbling = false;
            } 
            
            float z = 1.5f + Mathf.Sqrt(rb.velocity.magnitude) / 5f;
            dribblePoint.localPosition = Vector3.Lerp(dribblePoint.localPosition, new Vector3(0, -1.5f, z), Time.deltaTime);

            if (isDribbling && dribblingTimer >= 0)
            {
                dribblingTimer -= Time.deltaTime;
            }
            else if (isDribbling && dribblingTimer < 0)
            {
                float distance = Vector3.Distance(ballTransform.position, dribblePoint.position);
                Vector3 dribbleDirection = (dribblePoint.position - ballTransform.position).normalized * Mathf.Sqrt(distance) * 3f;

                if (gm.isSingleplayer)
                    ball.rb.AddForce(dribbleDirection,ForceMode.Impulse);
                else
                    ball.view.RPC("KickBall",RpcTarget.MasterClient,dribbleDirection, true);

                dribblingTimer = dribbleCooldown;
            }

            //jumping
            if (Input.GetButtonDown("Jump") && !usingJetpack && !gm.inSettings)
            {
                RaycastHit hit;
                if (Physics.Raycast(head.position, transform.forward, out hit, 2f, obstacle))
                {
                    rb.useGravity = false;
                    rb.velocity = Vector3.zero;
                    jumpsLeft = 0;
                    targetRotation = -hit.normal;
                    targetPosition = hit.point + hit.normal * 0.6f;
                }
                else if (!isClimbing && jumpsLeft > 0)
                {
                    isGrounded = false;
                    jumpsLeft--;

                    if (isDribbling)
                        StartCoroutine(ball.ActivateKickBall(Vector3.up * 8f, 0.15f));

                    if (jumpsLeft == 0 && doubleJumpAllowed)
                    {
                        animator.SetTrigger("jump");
                        rb.velocity = new Vector3(rb.velocity.x, jumpForce * 0.8f, rb.velocity.z);
                        GameObject effect = Instantiate(landingEffect1, effectSpawnPoint.position, Quaternion.identity);
                        effect.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    }
                    else
                    {
                        animator.SetTrigger("jump");
                        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
                    }
                }
            }
            else if (Input.GetButtonDown("Jump") && usingJetpack && jumpsLeft > 0 && !gm.inSettings)
            {
                rb.velocity = new Vector3(rb.velocity.x, jetpackForce, rb.velocity.z);
                movingAllowed = true;
                //Debug.Log("rb.velocity.y");
            }

            //climbing
            if (isClimbing)
            {
                model.transform.localPosition = Vector3.Lerp(model.transform.localPosition, new Vector3(0f, -1.8f, -0.5f), 10f * Time.deltaTime);
                cs.center = Vector3.Lerp(cs.center, new Vector3(0f, -0.4f, -0.4f), 3f * Time.deltaTime);

                RaycastHit hit;
                if (Physics.Raycast(head.position, transform.forward, out hit, obstacle))
                {
                    point.transform.position = hit.point;
                    Debug.Log(Vector3.Distance(transform.position, hit.point));
                    
                    if (Vector3.Distance(transform.position, hit.point) >= 3f)
                    {
                        rb.useGravity = true;
                        isClimbing = false;
                        isGrounded = false;
                        targetPosition = Vector3.zero;
                        nextPosition = Vector3.zero;
                        animator.SetTrigger("jump");
                        rb.velocity = new Vector3(rb.velocity.x, jumpForce * 1.2f, rb.velocity.z);
                    }
                }
                else
                {
                    rb.useGravity = true;
                    isClimbing = false;
                    isGrounded = false;
                    targetPosition = Vector3.zero;
                    nextPosition = Vector3.zero;
                    animator.SetTrigger("jump");
                    rb.velocity = new Vector3(rb.velocity.x, jumpForce * 1.2f, rb.velocity.z);
                }

                if (Input.GetAxisRaw("Vertical") < 0f)
                {
                    rb.useGravity = true;
                    isClimbing = false;
                    isGrounded = false;
                    targetPosition = Vector3.zero;
                    nextPosition = Vector3.zero;
                    animator.SetTrigger("jump");
                    rb.velocity = (transform.up - transform.forward).normalized * jumpForce;
                }
            }
            else
            {
                model.transform.localPosition = Vector3.Lerp(model.transform.localPosition, new Vector3(0f, -1.8f, 0f), 10f * Time.deltaTime);
                cs.center = Vector3.Lerp(cs.center, new Vector3(0f, -0.5f, 0f), 3f * Time.deltaTime);

                if (targetPosition != Vector3.zero)
                {
                    animator.SetBool("isClimbing", true);
                    //Debug.Log(Vector3.Distance(transform.position, targetPosition));

                    if (Vector3.Distance(transform.position, targetPosition) >= 0.01f || Vector3.Dot(transform.forward.normalized, targetRotation.normalized) >= 0.999f)
                    {
                        transform.position = Vector3.Lerp(transform.position, targetPosition, 30f * Time.deltaTime);
                        transform.forward = Vector3.Lerp(transform.forward, targetRotation, 90f * Time.deltaTime);
                        movingAllowed = false;
                    }
                    if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
                    {
                        targetPosition = Vector3.zero;
                        isClimbing = true;
                        isDribbling = false;
                        movingAllowed = true;
                    }
                }  
            }
            
            //kicking ball
            if (Input.GetButtonDown("Kick") && !gm.inSettings)
            {
                sideIndicator = Input.GetAxisRaw("Kick");
                startCharging = Time.time;
            }

            else if (Input.GetButton("Kick") && !gm.inSettings)
            {
                if (sideIndicator != 0)
                {
                    float wave = Mathf.Sin((Time.time - startCharging) * 3f - Mathf.PI * 0.5f) * 0.5f + 0.5f;
                    chargeBar.value = wave;
                }    
                else
                    chargeBar.value = 0f;
            }

            else if (Input.GetButtonUp("Kick") && !isClimbing && !gm.inSettings)
            {
                if (sideIndicator < 0f)
                {
                    if (chargeBar.value >= 0.6f)
                    {
                        animator.SetTrigger("leftShoot");
                        StartCoroutine(StopMoving(0.9f));
                    }
                    else
                    {
                        animator.SetTrigger("leftPass");
                        StartCoroutine(StopMoving(0.3f));
                    }
                
                    ballDistance = Vector3.Distance(ballTransform.position, left.position);
                }
                else if (sideIndicator > 0f)
                {
                    if (chargeBar.value >= 0.6f)
                    {
                        animator.SetTrigger("rightShoot");
                        StartCoroutine(StopMoving(0.9f));
                    }
                    else
                    {
                        animator.SetTrigger("rightPass");
                        StartCoroutine(StopMoving(0.3f));
                    }

                    ballDistance = Vector3.Distance(ballTransform.position, right.position);
                }

                kickDirection = transform.forward.normalized;
                kickDirection.y = 0.5f + chargeBar.value * 0.25f;
                
                float forceMultiplier;
                if (sideIndicator == dominantFoot && sideIndicator!=0)
                {
                    kickDirection.x *= 1f + Random.Range(-0.05f,0.05f);
                    kickDirection.y *= 1f + Random.Range(-0.05f,0.05f);
                    kickDirection.z *= 1f + Random.Range(-0.05f,0.05f);
                    forceMultiplier = 1f;
                }
                else if (dominantFoot == 0)
                {
                    kickDirection.x *= 1f + Random.Range(-0.2f,0.2f);
                    kickDirection.y *= 1f + Random.Range(-0.2f,0.2f);
                    kickDirection.z *= 1f + Random.Range(-0.2f,0.2f);
                    forceMultiplier = 0.9f;
                }
                else
                {
                    kickDirection.x *= 1f + Random.Range(-0.3f,0.3f);
                    kickDirection.y *= 1f + Random.Range(-0.3f,0.3f);
                    kickDirection.z *= 1f + Random.Range(-0.3f,0.3f);
                    forceMultiplier = 0.75f;
                }

                if (ballDistance < 3.5f && sideIndicator != 0)
                {
                    isDribbling = false;
                    Vector3 v = kickDirection * kickForce * (chargeBar.value * 2f + 1f) * forceMultiplier;
                    if (chargeBar.value >= 0.6f)
                        StartCoroutine(ball.ActivateKickBall(v, 0.25f));
                    else
                        StartCoroutine(ball.ActivateKickBall(v, 0.15f));
                }
                chargeBar.value = 0f;
            }

            //power-up durations
            if (isInvisible && durations[4] < 0f)
            {
                if (gm.myID % 2 == 0) // 0 red 1 blue
                    visual.ChangeSkin("red");
                else
                    visual.ChangeSkin("blue");
                isInvisible = false;
            }

            for (int i = 0; i < 10; i++)
                if (durations[i] >= 0f)
                    durations[i] -= Time.deltaTime;
        }
    }

    public void SpawnSmoke()
    {
        if (gm.isSingleplayer) 
        {
            if (gm.myID % 2 == 0) // 0 red 1 blue
            visual.ChangeSkin("red invisible");
            else
            visual.ChangeSkin("blue invisible");
            
            Instantiate(invisibleSmoke, effectSpawnPoint.position, Quaternion.Euler(0f, 0f, 0f));
        }
        else if (view.IsMine)
        {
            if (gm.myID % 2 == 0) // 0 red 1 blue
            visual.ChangeSkin("red invisible");
            else
            visual.ChangeSkin("blue invisible");
    
            PhotonNetwork.Instantiate(invisibleSmoke.name, effectSpawnPoint.position, Quaternion.Euler(0f, 0f, 0f));
        }
    }

    //landing
    private void OnCollisionEnter(Collision collision)
    {
        if (!gm.isSingleplayer && !view.IsMine)
        return;

        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            movingAllowed = true;
            
            if (doubleJumpAllowed)
            jumpsLeft = 2;
            else
            jumpsLeft = 1;

            if (inAirTime > 1.2f)
            {
                animator.SetTrigger("land");
                StartCoroutine(StopMoving(0.2f));
                pc.Shake(1f,0.2f);

                if (gm.isSingleplayer)
                {
                    Instantiate(landingEffect1, effectSpawnPoint.position, Quaternion.Euler(-90f, 0f, 0f));
                    Instantiate(landingEffect2, effectSpawnPoint.position, Quaternion.Euler(-90f, 0f, 0f));
                }
                else if (view.IsMine)
                {
                    PhotonNetwork.Instantiate(landingEffect1.name, effectSpawnPoint.position, Quaternion.Euler(-90f, 0f, 0f));
                    PhotonNetwork.Instantiate(landingEffect2.name, effectSpawnPoint.position, Quaternion.Euler(-90f, 0f, 0f));
                }
            }
            else
            {
                animator.SetTrigger("quickLand");

                if (gm.isSingleplayer)
                {
                    GameObject effect = Instantiate(landingEffect1, effectSpawnPoint.position, Quaternion.Euler(-90f, 0f, 0f));
                    effect.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    Instantiate(landingEffect2, effectSpawnPoint.position, Quaternion.Euler(-90f, 0f, 0f));
                }
                else if (view.IsMine)
                {
                    GameObject effect = PhotonNetwork.Instantiate(landingEffect1.name, effectSpawnPoint.position, Quaternion.Euler(-90f, 0f, 0f));
                    effect.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    PhotonNetwork.Instantiate(landingEffect2.name, effectSpawnPoint.position, Quaternion.Euler(-90f, 0f, 0f));
                }
            }
            inAirTime = 0f;
        }

        if (collision.gameObject.CompareTag("Ball"))
        {
            isDribbling = true;
        }
    }

    //jetpack
    void FixedUpdate()
    {
        if ((view.IsMine || gm.isSingleplayer) && !gm.inSettings)
        {
            if (Input.GetButton("Jump") && usingJetpack && jumpsLeft > 0)
            {
                isGrounded = false;
                jetpackAnimator.SetBool("inAir", true);

                if (myTime > nextEmission)
                {
                    nextEmission = myTime + timeBetweenEmission;

                    //Debug.Log(rb.velocity.y);
                    
                    if (transform.position.y >= 13f)
                    rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(rb.velocity.x, jetpackForce, rb.velocity.z), 20f * Time.deltaTime);
                    else
                    rb.velocity += new Vector3(0f, jetpackForce * (0.4f + (13f - Mathf.Sqrt(transform.position.y)) / 20f), 0f);

                    if (gm.isSingleplayer)
                    {
                        Instantiate(fireEffect,jetpackPoint1.position,Quaternion.Euler(-90f,0f,0f));
                        Instantiate(fireEffect,jetpackPoint2.position,Quaternion.Euler(-90f,0f,0f));
                    }
                    else if (!gm.isSingleplayer)
                    {
                        PhotonNetwork.Instantiate(fireEffect.name,jetpackPoint1.position,Quaternion.Euler(-90f,0f,0f));
                        PhotonNetwork.Instantiate(fireEffect.name,jetpackPoint2.position,Quaternion.Euler(-90f,0f,0f));
                    }
                    nextEmission = nextEmission - myTime;
                    myTime = 0f;
                }
            }
            else if (!Input.GetButton("Jump")) jetpackAnimator.SetBool("inAir", false);
            myTime += Time.fixedDeltaTime;

            Vector3 direction = new Vector3(movementX, 0f, movementZ).normalized;
            if (usingJetpack && !isGrounded && direction.magnitude > 0.1f) {
                model.transform.localRotation = Quaternion.Lerp(
                model.transform.localRotation,
                Quaternion.Euler(0f, 90f, 15f),
                Time.fixedDeltaTime * 5f);
            }
            else {
                model.transform.localRotation = Quaternion.Lerp(
                model.transform.localRotation,
                Quaternion.Euler(0f, 90f, 0f),
                Time.fixedDeltaTime * 5f);
            }
        }
    }

    //collideandslide
    private Vector3 CollideAndSlide(Vector3 velocity, Vector3 position, int depth)
    {
        if (depth >= maxBounces)
        return Vector3.zero;

        float distance = velocity.magnitude + 0.01f;
        position -= new Vector3(0f,2f,0f);
        RaycastHit hit;
        if (Physics.SphereCast(position,bounds.extents.x,velocity.normalized,out hit,distance,whatIsWall))
        {
            //Debug.Log("it worked");
            Vector3 snapToSurface = velocity.normalized * (hit.distance - 0.01f);
            Vector3 leftover = velocity - snapToSurface;

            if (snapToSurface.magnitude <= 0.01f)
            snapToSurface = Vector3.zero;

            float magnitude = leftover.magnitude;
            leftover = Vector3.ProjectOnPlane(leftover, hit.normal).normalized;
            leftover *= magnitude;

            return snapToSurface + CollideAndSlide(leftover, position + snapToSurface, depth + 1);
        }

        return velocity;
    }

    /*IEnumerator RunningEffect()
    {
        yield return new WaitForSeconds(0.5f);
    
        while (animator.GetBool("isRunning") && isGrounded)
        {
            SoundManager.PlaySound(SoundType.RUN, 1);
            if (gm.isSingleplayer)
                Instantiate(runningEffect, leftFoot.position, Quaternion.Euler(-90f, transform.eulerAngles.y, 0f));
            else if (view.IsMine)
                PhotonNetwork.Instantiate(runningEffect.name, leftFoot.position, Quaternion.Euler(-90f, transform.eulerAngles.y, 0f));
        
            yield return new WaitForSeconds(0.47f);
            if(!(animator.GetBool("isRunning") && isGrounded)) continue;

            SoundManager.PlaySound(SoundType.RUN, 1);
            if (gm.isSingleplayer)
                Instantiate(runningEffect, rightFoot.position, Quaternion.Euler(-90f, transform.eulerAngles.y, 0f));
            else if (view.IsMine)
                PhotonNetwork.Instantiate(runningEffect.name, rightFoot.position, Quaternion.Euler(-90f, transform.eulerAngles.y, 0f));
            
            yield return new WaitForSeconds(0.47f);
        }
    }*/

    IEnumerator DebuggingTool()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            Debug.Log(rb.velocity.x + " " + rb.velocity.y + " " + rb.velocity.z);
            //Debug.Log(sideIndicator);
        }
    }

    IEnumerator StopMoving(float duration)
    {
        movingAllowed = false;
        yield return new WaitForSeconds(duration);
        movingAllowed = true;
    }
}
