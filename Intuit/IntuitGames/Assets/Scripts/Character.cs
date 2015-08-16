using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;[RequireComponent(typeof(CharacterController))]public class Character : MonoBehaviour
{
    #region VARIABLES

    // STATICS
    public static Character character1 { get; private set; }
    public static Character character2 { get; private set; }

    // COMPONENTS
    [HideInInspector]
    public CharacterController characterController;

    // STATS
    [SerializeField, Popup(new string[2] { "Player 1", "Player 2"}, OverrideName = "Player"), Header("Basic")]
    private bool _isPlayerOne = true;
    [ReadOnly]
    public Vector3 targetVelocity;
    public float baseMoveSpeed = 7;
    [Range(0, 10)]
    public float baseGravity = 3;
    public AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [Range(0, 25)]
    public float jumpPower = 10;
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
    [Range(0, 1)]
    public float bounceMomentumLoss = 0.5f;
    [Range(0, 25)]
    public float bouncePower = 10;
    [Range(0, 25)]
    public float bounceJumpPower = 15;
    public bool bounceWhileHeavy = true;
    public bool bounceOffGround = true;
    [Tooltip("How fast the player must be going in order to bounce from the ground.")]
    public float bounceGroundThreshold = 10;
    [Range(0, 25)]
    public float bounceGroundPower = 5;

    // PRIVATES
    private float airTime = 0;
    private const float airborneRadiusCheck = 0.4f;
    private const float airborneOffset = 0.2f;

    // PROPERTIES
    public bool isPlayerOne
    {
        get { return _isPlayerOne; }
        set
        {
            if (value != isPlayerOne)
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
        {			Debug.DrawRay(transform.position, -transform.up*((characterController.height / 2) + airborneOffset),Color.red );
            if (characterController.isGrounded)
                return false;
            else
                return AirborneRaycheck(transform.position, -transform.up, ((characterController.height / 2) + airborneOffset), airborneRadiusCheck);				
        }
    }
    public bool isFalling
    {
        get
        {
            return isAirborne && targetVelocity.y < 0;
        }
    }

    #endregion

    #region MESSAGES

    void Awake()
    {
        // Set self in static
        if (!SetStaticCharacter(this)) DestroyImmediate(gameObject);

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

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Bounce(hit.gameObject, hit.normal, characterController.velocity);

        // Stop dashing if hit by something in front
        if (hit.normal.z < 0 && stopDashOnCollision) isDashing = false;
    }

    #endregion

    #region INPUT

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
        Vector2 direction = Vector2.ClampMagnitude(new Vector2(right, forward), 1);

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

    public void Jump(float jumpTime)
    {
        if (!isHeavy) // Standard jump
            targetVelocity.y += jumpCurve.Evaluate(jumpTime) * (jumpPower / 10);
        else
            targetVelocity.y = heavyJumpPower; // Heavy High Jump
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
        TimerPlus.Create(0.25f, () => Application.LoadLevel(Application.loadedLevel));
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

    #endregion

    #region HELPERS

    // Set static character references
    private static bool SetStaticCharacter(Character character)
    {
        if (!character) return false;

        if (character.isPlayerOne)
        {
            if (!character1)
            {
                character1 = character;
                return true;
            }
            else
            {
                Debug.LogWarning("Character 1 has already been set!");
                return false;
            }
        }
        else
        {
            if (!character2)
            {
                character2 = character;
                return true;
            }
            else
            {
                Debug.LogWarning("Character 1 has already been set!");
                return false;
            }
        }
    }

    // Use ray casts to determine if the character is airborne
    private bool AirborneRaycheck(Vector3 origin, Vector3 dir, float maxDir, float radius)
    {
        return !Physics.Raycast(origin, dir, maxDir) &&
            !Physics.Raycast(origin + new Vector3(radius, 0, 0), dir, maxDir) &&
            !Physics.Raycast(origin + new Vector3(-radius, 0, 0), dir, maxDir) &&
            !Physics.Raycast(origin + new Vector3(0, 0, radius), dir, maxDir) &&
            !Physics.Raycast(origin + new Vector3(0, 0, -radius), dir, maxDir);
    }

    #endregion
}