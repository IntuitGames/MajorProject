using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;[RequireComponent(typeof(CharacterController))]public class Character : MonoBehaviour{
    // COMPONENTS
    [HideInInspector]
    public CharacterController characterController;

    // STATS
    [SerializeField, Popup(new string[2] { "Player 1", "Player 2"}, OverrideName = "Player")]
    private bool _isPlayerOne = true;    public bool isPlayerOne
    {
        get { return _isPlayerOne; }
        set
        {
            if(value != isPlayerOne)
            {
                _isPlayerOne = value;
                if (Application.isPlaying) GameManager.inputManager.SetupCharacterInput(this);
            }
        }
    }

    public float moveSpeed
    {
        get
        {
            float value = baseMoveSpeed;
            if (isHeavy) value = heavyMoveSpeed;
            return value;
        }
    }
    public float gravity
    {
        get
        {
            float value = baseGravity;
            if (isHeavy) value = heavyGravity;
            return value;
        }
    }

    [ReadOnly, Header("Basic")]
    public Vector3 targetVelocity;
    public float baseMoveSpeed = 7;
    [Range(0, 10)]
    public float baseGravity = 3;
    [Range(0, 25)]
    public float lowJumpPower = 5;
    [Range(0, 25)]
    public float mediumJumpPower = 10;
    [Range(0, 25)]
    public float hightJumpPower = 15;
    public float maxSpeed = 50;

    [Header("Dash"), SerializeField]
    private bool _canDash = true;
    public float dashPower = 25;
    public bool isDashing
    {
        get { return dashTimer.IsPlaying; }
        set
        {
            if (value)
            {
                dashTimer.Restart();
                canDash = false;
            }
            else
                dashTimer.Stop();
        }
    }
    [Tooltip("In seconds (Will try to make it distance soon)"), Range(0, 3)]
    public float dashLength = 0.5f;
    [Range(0, 25)]
    public float dashHeight = 3;
    private TimerPlus dashTimer;
    public bool stopDashOnCollision = true;
    [Range(0, 3)]
    public float dashCooldown = 1;
    private TimerPlus dashCooldownTimer;
    public bool canDash
    {
        get { return _canDash && !dashCooldownTimer.IsPlaying; }
        set
        {
            if (value)
                dashCooldownTimer.Stop();
            else
                dashCooldownTimer.Restart();
        }
    }

    [ReadOnly, Header("Heavy")]
    public bool isHeavy = false;
    public float heavyMoveSpeed = 2;
    public bool canUnheavyMidair = false;
    [Range(0, 10)]
    public float heavyGravity = 6;
    [Range(0, 25)]
    public float heavyJumpPower = 20;

    [Header("Bounce")]
    [Popup(new string[3] { "Velocity Based", "Set Value", "Off" } )]
    public string bounceType = "Velocity Based";
    [Range(0, 1), Conditional("bounceType", "Velocity Based")]
    public float bounceMomentumLoss = 0.5f;
    [Range(0, 25), Conditional("bounceType", "Set Value")]
    public float bouncePower = 10;
    [Range(0, 25), Conditional("bounceType", "Set Value")]
    public float bounceJumpPower = 15;
    public bool bounceWhileHeavy = true;
    public bool bounceOffGround = true;
    [Tooltip("How fast the player must be going in order to bounce from the ground.")]
    public float bounceGroundThreshold = 10;
    [Range(0, 25)]
    public float bounceGroundPower = 5;

    // PRIVATES & STATICS
    public static List<Character> characterList = new List<Character>();

    private float airTime = 0;
    private const float airborneRadiusCheck = 0.4f;
    private const float airborneOffset = 0.1f;

    // STATES
    public bool isWalking
    {
        get
        {
            return new Vector2(targetVelocity.x, targetVelocity.z).magnitude > 0;
        }
    }
    public bool isAirborne
    {
        get
        {
            if (characterController.isGrounded)
                return false;
            else
                return !Physics.Raycast(transform.position, -transform.up, (characterController.height / 2) + 0.1f) &&
                    !Physics.Raycast(transform.position + new Vector3(airborneRadiusCheck, 0, 0), -transform.up, (characterController.height / 2) + airborneOffset) &&
                    !Physics.Raycast(transform.position + new Vector3(-airborneRadiusCheck, 0, 0), -transform.up, (characterController.height / 2) + airborneOffset) &&
                    !Physics.Raycast(transform.position + new Vector3(0, 0, airborneRadiusCheck), -transform.up, (characterController.height / 2) + airborneOffset) &&
                    !Physics.Raycast(transform.position + new Vector3(0, 0, -airborneRadiusCheck), -transform.up, (characterController.height / 2) + airborneOffset);
        }
    }
    public bool isFalling
    {
        get
        {
            return isAirborne && targetVelocity.y < 0;
        }
    }
    void Awake()
    {
        // Set self in player manager
        if (characterList.Count >= 2) characterList.Clear();
        characterList.Add(this);

        // Find component references
        characterController = GetComponent<CharacterController>();

        // Setup dash timers
        dashTimer = TimerPlus.Create(dashLength, TimerPlus.Presets.Standard);
        dashCooldownTimer = TimerPlus.Create(dashCooldown, TimerPlus.Presets.Standard);
    }    void Start()
    {
        // Setup up character input depending on whether this is character 1 or 2
        GameManager.inputManager.SetupCharacterInput(this);
    }

    // Is called BEFORE input is checked every frame
    public void PreInputUpdate()
    {
        if (!this.enabled) return;

        // Apply any changes to the dash length and cooldown
        dashTimer.Length = dashLength;
        dashCooldownTimer.Length = dashCooldown;

        // Apply gravity if airborne
        if (isAirborne)
        {
            airTime += Time.deltaTime;
            targetVelocity.y += -gravity * airTime;
        }
        else
        {
            targetVelocity.y = Mathf.Max(0, targetVelocity.y);
            airTime = 0;
        }
    }

    // Is called AFTER input is determined every frame
    public void PostInputUpdate()
    {
        if (!this.enabled) return;

        // Clamp movement velocity
        targetVelocity = Vector3.ClampMagnitude(targetVelocity, maxSpeed);

        // Rotate in movement direction
        transform.LookAt((transform.position + targetVelocity).IgnoreY3(transform.position.y));

        // Apply movement
        CollisionFlags colFlags = characterController.Move(targetVelocity * Time.deltaTime);

        // Zero Y movement if collided with an object above
        if ((colFlags & CollisionFlags.CollidedAbove) == CollisionFlags.CollidedAbove) targetVelocity.y = Mathf.Min(0, targetVelocity.y);
    }

    public void Movement(float forward, float right)
    {
        Vector2 direction = new Vector2(right, forward).normalized;

        if (!isDashing)
        {
            targetVelocity.x = direction.x * moveSpeed;
            targetVelocity.z = direction.y * moveSpeed;
        }
        else
        {
            targetVelocity.x = transform.forward.x * dashPower;
            targetVelocity.z = transform.forward.z * dashPower;
        }
    }

    public void Jump(int jumpType) // 1 = low, 2 = med, 3 = high, 4 = ledge(high)
    {
        if (!isAirborne)
        {
            if (!isHeavy)           // Standard jump
                targetVelocity.y += jumpType == 3 ? hightJumpPower : jumpType == 2 ? mediumJumpPower : jumpType == 1 ? lowJumpPower : 0;
            else                    // Heavy jump
                targetVelocity.y += heavyJumpPower;
        }
        else if (jumpType == 4)     // Ledge jump
            targetVelocity.y += hightJumpPower;
    }

    public void Dash()
    {
        if (canDash)
        {
            isDashing = true;
            if (!isAirborne) targetVelocity.y += dashHeight;
        }
    }

    public void Heavy(bool isHeldDown)
    {
        if (!canUnheavyMidair && isHeavy && !isHeldDown && isAirborne)
            return;
        else
            isHeavy = isHeldDown;
    }

    public void Pause()
    {
        // Reloads the level for now
        Application.LoadLevel(Application.loadedLevel);
    }

    public void Bounce(GameObject hit, Vector3 normal, Vector3 hitVelocity)
    {
        if (bounceType != "Off" && bounceWhileHeavy || bounceType != "Off" && !isHeavy)
        {
            // Bounce off a bouncy object
            Bouncy bouncyObj = hit.gameObject.GetComponent<Bouncy>();
            if (bouncyObj && bouncyObj.isBouncy)
                if (bounceType == "Velocity Based")
                    targetVelocity = normal * (hitVelocity.magnitude * bounceMomentumLoss) * bouncyObj.bounceMultiplier;
                else if (bounceType == "Set Value")
                    if (!GameManager.inputManager.IsRequestingJump(isPlayerOne))
                        targetVelocity = normal * bouncePower * bouncyObj.bounceMultiplier;
                    else
                        targetVelocity = normal * bounceJumpPower * bouncyObj.bounceMultiplier;
                else
                    Debug.LogWarning("Invalid bounce type specified!");
            // Bounce of any object tagged ground if traveling fast enough
            else if (bounceOffGround && hit.gameObject.tag == "Ground" && Mathf.Abs(hitVelocity.y) > bounceGroundThreshold && normal.y > 0)
                targetVelocity = normal * bounceGroundPower;
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Bounce(hit.gameObject, hit.normal, characterController.velocity);

        // Stop dashing if hit by something in front
        if (hit.normal.z < 0 && stopDashOnCollision) isDashing = false;
    }
}