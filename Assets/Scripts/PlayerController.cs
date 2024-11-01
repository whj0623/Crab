using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviourPunCallbacks
{
    public float spd = 5f;
    public float sens = 2f;
    public float lookLimit = 80f;
    public float crouchSpd = 2f;
    public float jumpPower = 8f;
    public float sprintMult = 2f;
    public float slideDur = 1f;
    public float slideSpd = 12f;
    public float interactDist = 2f;
    public Vector3 crouchOffset;
    public Vector3 slideOffset;
    public float camSmooth = 0.1f;
    public GameObject deathParticle;
    public bool isDead = false;
    public float attackForce = 1;
    public Transform deathCamTarget;
    public GameObject playerModel;
    public Rigidbody rb;
    public Animator anim;
    public Camera cam;
    public GameObject stick;
    public Action OnPlayerDead { get; set; } 
    public float moveX = 0, moveY = 0;
    public bool isFreeze = false;

    private CharacterController cc;
    private float vertRot = 0f;
    private Vector3 moveDir;
    private bool isCrouch = false, isSlide = false, isPushed = false;
    private float slideTimer = 0f;
    private Vector3 camPos, camTargetPos;
    private PlayerVisibility pv;
    private bool isSpectator = false;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        pv = GetComponent<PlayerVisibility>();
        rb.isKinematic = true;

        if (!photonView.IsMine) return;
        camPos = cam.transform.localPosition;
        camTargetPos = camPos;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        if (!isDead)
        {
            Look();
            Anim();
            Attack();
            if (!isFreeze)
                Move();
            CamPosUpdate();

            if (Input.GetKeyDown(KeyCode.C))
            {
                if (IsGrounded())
                {
                    if (Input.GetKey(KeyCode.LeftShift) && cc.velocity.magnitude > 0.2f)
                        Slide();
                    else
                        Crouch(true);
                }
            }

            if (Input.GetKeyUp(KeyCode.C))
            {
                Crouch(false);
                isSlide = false;
                anim.SetBool("Sliding", false);
            }
        }
        else
        {
            CamPosUpdateDead();
            if (isSpectator)
            {
                SpectatorCameraControl();
            }
        }
    }

    void Move()
    {
        if (!cc.enabled) return;

        if (!isPushed)
        {
            moveX = Input.GetAxis("Horizontal") * (isCrouch ? crouchSpd : spd);
            moveY = Input.GetAxis("Vertical") * (isCrouch ? crouchSpd : spd);
        }

        Vector3 moveHorizontal = transform.right * moveX + transform.forward * moveY;

        if (Input.GetKey(KeyCode.LeftShift) && !isPushed)
            moveHorizontal *= sprintMult;

        if (isSlide)
            SlideUpdate();

        if (cc.isGrounded)
        {
            moveDir.y = 0;
            if (Input.GetButtonDown("Jump") && !isSlide && !isPushed)
                moveDir.y = jumpPower;
        }
        else
        {
            moveDir.y += Physics.gravity.y * Time.deltaTime * 3;
        }

        Vector3 finalMove = moveHorizontal + Vector3.up * moveDir.y;
        cc.Move(finalMove * Time.deltaTime);
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, cc.height / 2 + 0.1f);
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * sens;
        transform.Rotate(Vector3.up * mouseX);

        float mouseY = Input.GetAxis("Mouse Y") * sens;
        vertRot -= mouseY;
        vertRot = Mathf.Clamp(vertRot, -lookLimit, lookLimit);
        cam.transform.localRotation = Quaternion.Euler(vertRot, 0f, 0f);
    }

    void Anim()
    {
        anim.SetBool("Grounded", IsGrounded());
        anim.SetBool("Jumping", !IsGrounded());
        float xDir = Input.GetAxis("Horizontal");
        float yDir = Input.GetAxis("Vertical");
        anim.SetFloat("xDir", xDir);
        anim.SetFloat("yDir", yDir);
    }

    void Attack()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            anim.Play("Melee Attack");
            photonView.RPC("PlayAnimation", RpcTarget.Others, "Melee Attack");

            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, interactDist))
            {
                PlayerController targetPlayer = hit.collider.GetComponent<PlayerController>();
                if (targetPlayer == null)
                    targetPlayer = hit.collider.GetComponentInParent<PlayerController>();

                if (targetPlayer != null && !targetPlayer.photonView.IsMine)
                    targetPlayer.photonView.RPC("Pushed", targetPlayer.photonView.Owner, transform.position);
            }
        }
    }
    
    [PunRPC]
    private void Pushed(Vector3 sourcePosition)
    {
        Vector3 directionToAttacker = (transform.position - sourcePosition).normalized;
        isPushed = true;
        Quaternion rotation = Quaternion.Euler(0, -transform.eulerAngles.y, 0).normalized;
        Vector3 adjustedDirection = rotation * directionToAttacker;

        StartCoroutine(PushRoutine(adjustedDirection, attackForce, 0.3f));
    }

    private IEnumerator PushRoutine(Vector3 direction, float force, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float currentForce = Mathf.Lerp(force, 0, t * t);
            moveX = direction.x * currentForce;
            moveY = direction.z * currentForce;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ReEnableMovement();
    }

    [PunRPC]
    void PlayAnimation(string animationName)
    {
        anim.Play(animationName);
    }
   
    private void ReEnableMovement()
    {      
        isPushed = false;
        moveX = 0;
        moveY = 0;
    }

    void Slide()
    {
        isSlide = true;
        slideTimer = slideDur;
        anim.SetBool("Sliding", true);
        camTargetPos = camPos + slideOffset;
    }

    void SlideUpdate()
    {
        slideTimer -= Time.deltaTime;
        float slideProgress = 1f - (slideTimer / slideDur);
        float curSlideSpd = Mathf.Lerp(slideSpd, spd, slideProgress);
        moveDir = transform.forward * curSlideSpd;

        if (slideTimer <= 0f)
        {
            isSlide = false;
            anim.SetBool("Sliding", false);
            if (Input.GetKey(KeyCode.C)) Crouch(true);
        }
    }

    void Crouch(bool state)
    {
        isCrouch = state;
        camTargetPos = isCrouch ? camPos + crouchOffset : camPos;
        anim.SetBool("Crouching", isCrouch);
    }

    void CamPosUpdate()
    {
        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, camTargetPos, camSmooth);
    }

    void CamPosUpdateDead()
    {
        if (deathCamTarget != null)
        {
            Vector3 targetPosition = deathCamTarget.position + deathCamTarget.forward * 5f + new Vector3(0, 1.5f, -5f);
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetPosition, camSmooth);
            cam.transform.LookAt(deathCamTarget);
        }
    }

    public void Eliminated()
    {
        OnPlayerDead?.Invoke();
        
        PhotonNetwork.LocalPlayer.CustomProperties["survived"] = false;
        if (deathParticle != null)
            Instantiate(deathParticle, transform.position, Quaternion.identity);
        pv.DeadCamera();
        anim.enabled = false;
        isDead = true;
        Destroy(playerModel);
        if (photonView.IsMine)
        {
            Hashtable customProps = PhotonNetwork.LocalPlayer.CustomProperties;
            customProps["survived"] = false;
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
        }

        StartCoroutine(SwitchToSpectatorMode());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "DeadZone")
            Dead();
    }

    private void Dead()
    {
        isDead = true;
        anim.enabled = false;
        pv.DeadCamera();
        StartCoroutine(SwitchToSpectatorMode());
    }

    private IEnumerator SwitchToSpectatorMode()
    {
        isSpectator = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        while (isSpectator)
        {
            yield return null;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SpectatorCameraControl()
    {
        float mouseX = Input.GetAxis("Mouse X") * sens;
        float mouseY = Input.GetAxis("Mouse Y") * sens;

        cam.transform.Rotate(-mouseY, mouseX, 0);
        cam.transform.LookAt(transform.position);
    }
}
