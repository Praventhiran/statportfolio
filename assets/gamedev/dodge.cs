using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDodge : MonoBehaviour
{
    [Header("Settings")]
    public float dodgeCooldown = 1.0f;
    private float lastDodgeTime = -1f;

    [Header("Animations")]
    public string dodgeForwardAnim = "VDodgeFront";
    public string dodgeBackAnim = "VDodgeBack";
    public string dodgeLeftAnim = "VDodgeLeft";
    public string dodgeRightAnim = "VDodgeRight";

    // References
    private Animator animator;
    public ThirdPersonCamera thirdPersonCamera;
    private PlayerController movementScript;

    void Start()
    {

        animator = GetComponent<Animator>();
        movementScript = GetComponent<PlayerController>();

        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > lastDodgeTime + dodgeCooldown)
        {
            TryDodge();
        }
    }

    void TryDodge()
    {
        // 1. Align player to camera (y-axis only)
        transform.rotation = Quaternion.Euler(0, thirdPersonCamera.currentRotationX, 0);

        // 2. Set direction parameter
        if (Input.GetKey(KeyCode.W)) animator.SetInteger("DodgeDirection", 1);
        else if (Input.GetKey(KeyCode.A)) animator.SetInteger("DodgeDirection", 2);
        else if (Input.GetKey(KeyCode.D)) animator.SetInteger("DodgeDirection", 3);
        else animator.SetInteger("DodgeDirection", 0); // Default back dodge

        // 3. Trigger dodge
        animator.SetTrigger("Dodge");

        // 4. Block movement
        if (movementScript) movementScript.allowMovement = false;

        // 5. Start cooldown
        lastDodgeTime = Time.time;
    }

    //string GetDodgeAnimation()
    //{
    //    if (Input.GetKey(KeyCode.W)) return dodgeForwardAnim;
    //    if (Input.GetKey(KeyCode.A)) return dodgeLeftAnim;
    //    if (Input.GetKey(KeyCode.D)) return dodgeRightAnim;
    //    return dodgeBackAnim; // Default if only Space pressed
    //}

    // Called via Animation Event at end of dodge animations
    public void OnDodgeComplete()
    {
        animator.SetInteger("DodgeDirection", -1); // Reset
        if (movementScript) movementScript.allowMovement = true;
    }
}