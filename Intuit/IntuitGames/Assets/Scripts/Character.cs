using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using FMOD.Studio;
using CustomExtensions;[RequireComponent(typeof(CharacterController), typeof(AudioSource))]public class Character : MonoBehaviour
{
    #region VARIABLES

    // CONSTANTS
    private const int POWER_MAX = 50;
    private const int GRAV_MAX = 10;

    // STATICS
    public static Character character1 { get; private set; }
    public static Character character2 { get; private set; }

    // COMPONENTS
    [HideInInspector]
    public CharacterController characterController;
    [HideInInspector]
    public AudioSource audioSource;
    [HideInInspector]
    public Animator animator;

    // BASIC STATS
    [SerializeField, Popup(new string[2] { "Player 1", "Player 2"}, OverrideName = "Player"), Header("Basic")]
    private bool _isPlayerOne = true;
    [ReadOnly]
    public Vector3 targetVelocity;
    public float baseMoveSpeed = 7;
    [Range(0, GRAV_MAX)]
    public float baseGravity = 3;
    public AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [Range(0, GRAV_MAX)]
    public float jumpPower = 10;
    public float maxSpeed = 50;
    public LayerMask layerDetection;

    // DASH
    [Header("Dash"), SerializeField]
    private bool _canDash = true;
    public float dashPower = 25;
    [Tooltip("In seconds (Will try to make it distance soon)"), Range(0, 3)]
    public float dashLength = 0.5f;
    [Range(0, GRAV_MAX)]
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

    // HEAVY
    [Header("Heavy")]
    public bool canHeavy = true;
    public float heavyMoveSpeed = 2;
    public bool canUnheavyMidair = false;
    [Range(0, GRAV_MAX)]
    public float heavyGravity = 6;
    [Range(0, POWER_MAX)]
    public float heavyJumpPower = 20;
    public bool canBounceWhileHeavy = false;

    // BOUNCE
    [Header("Bounce"), SerializeField]
    private bool _canBounce = true;
    public AnimationCurve momentumRetension = AnimationCurve.EaseInOut(40, 1, 80, 0.75f);
    public float minBounceThreshold = 40;
    public float maxBounceThreshold = 80;
    [Range(0, POWER_MAX)]
    public float minBouncePower = 15;
    [Range(0, POWER_MAX)]
    public float maxBouncePower = 30;
    [Range(0, POWER_MAX)]
    public float minJumpBouncePower = 20;
    [Range(0, POWER_MAX)]
    public float maxJumpBouncePower = 35;
    public bool canBounceOffGround = true;
    public float minGroundBounceThreshold = 50;
    public float maxGroundBounceThreshold = 100;
    [Range(0, POWER_MAX)]
    public float minGroundBouncePower = 10;
    [Range(0, POWER_MAX)]
    public float maxGroundBouncePower = 10;
    public bool canBounce
    {
        get { return canBounceWhileHeavy ? _canBounce : _canBounce && !isHeavy; }
    }

    // AUDIO
    [Header("Audio")]
    public List<AudioClip> walkSounds = new List<AudioClip>();
    public List<AudioClip> jumpSounds = new List<AudioClip>();
    public List<AudioClip> dashSounds = new List<AudioClip>();
    public List<AudioClip> landSounds = new List<AudioClip>();
    public List<AudioClip> bounceSounds = new List<AudioClip>();

    // F-MOD
    public bool useFMOD = true;
    private FMOD_StudioSystem FM_soundSystem;
    private EventInstance FM_jump, FM_land, FM_footstep;
    private ParameterInstance FM_playerFallSpeed, FM_playerMovespeed;
    private float FM_playerFallSpeedValue
    {
        get { return targetVelocity.y < 0 ? Mathf.Lerp(0, 1, Mathf.Abs(targetVelocity.y) / (maxSpeed * 0.5f)) : 0; }
    }
    private float FM_playerMovespeedValue
    {
        get { return targetVelocity.IgnoreY2().magnitude; }
    }

    // PRIVATES
#pragma warning disable 414
    private float airTime = 0;                          // How long this character has been airborne for
    private TimerPlus airTimeResetTimer;
    private RaycastHit onObject;                        // What object is this character standing on
    private const float airborneCenterRayOffset = 0.5f; // How much additional height offset will the ray checks account for
#pragma warning restore 414

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

    // QUICK-ACCESS
    private Bouncy BounceObject
    {
        get { return onObject.collider != null ? onObject.collider.gameObject.GetComponent<Bouncy>() : null; }
    }

    // STATES
    public bool isWalking
    {
        get
        {
            return new Vector2(targetVelocity.x, targetVelocity.z).magnitude > 0;
        }
    }
    private bool wasAirborne;
    public bool isAirborne
    {
        get
        {
            return AirborneRaycheck(transform.position, -transform.up, characterController.height / 2, airborneCenterRayOffset, characterController.radius, out onObject);
        }
    }
    public bool isFalling
    {
        get
        {
            return isAirborne && targetVelocity.y < 0;
        }
    }
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
    public bool isHeavy { get; set; }
    public bool isBouncing { get; set; }

    // EVENTS
    public event System.Action<bool> Landed = delegate { };

    #endregion

    #region MESSAGES

    void Awake()
    {
        // Set self in static
        if (!SetStaticCharacter(this)) DestroyImmediate(gameObject);

        // Find component references
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();

        // Setup dash timers
        dashTimer = TimerPlus.Create(dashLength, TimerPlus.Presets.Standard);
        dashCooldownTimer = TimerPlus.Create(dashCooldown, TimerPlus.Presets.Standard);
        airTimeResetTimer = TimerPlus.Create(0.1f, TimerPlus.Presets.Standard, () => airTime = 0);
    }    void Start()
    {
        // Setup up character input depending on whether this is character 1 or 2
        GameManager.inputManager.SetupCharacterInput(this);

        // Setup FMOD
        SetupFMOD();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Stop dashing if hit by something in front
        if (hit.normal.z < 0 && stopDashOnCollision) isDashing = false;
    }

    void OnDisabled()
    {
        FM_jump.stop(STOP_MODE.IMMEDIATE);
        FM_jump.release();

        FM_land.stop(STOP_MODE.IMMEDIATE);
        FM_land.release();

        FM_footstep.stop(STOP_MODE.IMMEDIATE);
        FM_footstep.release();
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
            airTimeResetTimer.Reset();
            airTime += Time.deltaTime;
            targetVelocity.y += -gravity * airTime;
        }
        else
        {
            if (!isBouncing)
                airTimeResetTimer.Start();
            else
                airTime = 0;

            targetVelocity.y = Mathf.Max(0, targetVelocity.y);
        }

        // Update FMOD parameters
        if (FM_playerFallSpeed != null) FM_playerFallSpeed.setValue(FM_playerFallSpeedValue);
        if (FM_playerMovespeed != null) FM_playerMovespeed.setValue(FM_playerMovespeedValue);
        
        var FM_attributes = GetFMODAttribute(transform, characterController.velocity);

        if (FM_land != null) FM_land.set3DAttributes(FM_attributes);
        if (FM_jump != null) FM_jump.set3DAttributes(FM_attributes);
        if (FM_footstep != null) FM_footstep.set3DAttributes(FM_attributes);
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

        // Send animator info
        animator.SetBool("IsAirborne", isAirborne);
        animator.SetFloat("Speed", targetVelocity.IgnoreY2().normalized.magnitude);
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
        if (isBouncing) return;

        if (jumpTime > jumpCurve.Duration()) return;

        PlaySound(FM_jump, jumpSounds.Random(), jumpTime <= Time.deltaTime);

        if (!isHeavy)
            targetVelocity.y += jumpCurve.Evaluate(jumpTime) * (jumpPower / 6.4f / jumpCurve.Duration());
        else if(!isAirborne)
                targetVelocity.y = heavyJumpPower;
    }

    public void Dash()
    {
        if (canDash)
        {
            isDashing = true;

            if(!useFMOD) audioSource.PlayClip(dashSounds.Random());

            if (!isAirborne) targetVelocity.y += dashHeight;
        }
    }

    public void Heavy(bool isHeldDown)
    {
        if(!canHeavy)
        {
            isHeavy = false;
            Bounce(0);
            return;
        }

        if (!canUnheavyMidair && isHeavy && !isHeldDown && isAirborne)
            return;
        else
        {
            if (!isHeldDown) Bounce(0);
            isHeavy = isHeldDown;
        }
    }

    public void Pause()
    {
        // Reloads the level for now
        TimerPlus.Create(0.25f, () => Application.LoadLevel(Application.loadedLevel));
    }

    public void Bounce(float downVelocity)
    {
        if (!canBounce || downVelocity < 0)
        {
            isBouncing = false;
            return;
        }

        if (isBouncing && downVelocity <= 0) return;

        float bouncePower = 0;

        // Bounce off a bouncy object with jump
        if (BounceObject && BounceObject.isBouncy && GameManager.inputManager.IsRequestingJump(isPlayerOne))
        {
            if (downVelocity >= minBounceThreshold)
                bouncePower = Mathf.Lerp(minJumpBouncePower, maxJumpBouncePower, downVelocity / maxBounceThreshold) * BounceObject.bounceMultiplier;
            else
                bouncePower = minJumpBouncePower;

            BounceObject.OnBounce();
            isBouncing = true;
        }
        // Bounce off a bouncy object without jump
        else if (BounceObject && BounceObject.isBouncy)
        {
            if (downVelocity >= minBounceThreshold)
                bouncePower = Mathf.Lerp(minBouncePower, maxBouncePower, downVelocity / maxBounceThreshold) * BounceObject.bounceMultiplier;
            else
                bouncePower = minBouncePower;

            BounceObject.OnBounce();
            isBouncing = true;
        }
        // Bounce off the ground
        else if (canBounceOffGround && downVelocity > minGroundBounceThreshold && onObject.collider.gameObject.CompareTag("Ground"))
        {
            bouncePower = Mathf.Lerp(minGroundBouncePower, maxGroundBouncePower, downVelocity / maxGroundBounceThreshold);

            isBouncing = true;
        }
        else
            isBouncing = false;

        // Apply bounce power
        if (isBouncing)
        {
            if (!useFMOD) audioSource.PlayClip(bounceSounds.Random());
            bouncePower *= momentumRetension.Evaluate(downVelocity);
            targetVelocity.y = bouncePower;
        }
    }

    #endregion

    #region EVENTS

    private void OnLanded(Vector3 landVelocity)
    {
        Landed(isPlayerOne);

        Bounce(-landVelocity.y);

        PlaySound(FM_land, landSounds.Random(), landVelocity.y < -10);
    }

    void OnFootStep(int footIndex) // 1 left, 2 right
    {
        PlaySound(FM_footstep, walkSounds.Random(), isWalking && !isAirborne);
    }

    #endregion

    #region HELPERS

    // Use ray casts to determine if the character is airborne
    private bool AirborneRaycheck(Vector3 origin, Vector3 direction, float distance, float offset, float radius, out RaycastHit landObject)
    {
        bool newIsAirborne;

        // Cast airborne rays 
        float oneThirdDistance = distance + (offset / 3);
        float twoThirdDistance = distance + ((offset / 3) * 2);
        if (Physics.Raycast(origin, direction, out landObject, distance + offset, layerDetection) ||
            Physics.Raycast(origin + new Vector3(radius, 0, 0), direction, out landObject, oneThirdDistance, layerDetection) ||
            Physics.Raycast(origin + new Vector3(-radius, 0, 0), direction, out landObject, oneThirdDistance, layerDetection) ||
            Physics.Raycast(origin + new Vector3(0, 0, radius), direction, out landObject, oneThirdDistance, layerDetection) ||
            Physics.Raycast(origin + new Vector3(0, 0, -radius), direction, out landObject, oneThirdDistance, layerDetection) ||
            Physics.Raycast(origin + new Vector3(radius / 2, 0, radius / 2), direction, out landObject, twoThirdDistance, layerDetection) ||
            Physics.Raycast(origin + new Vector3(-radius / 2, 0, radius / 2), direction, out landObject, twoThirdDistance, layerDetection) ||
            Physics.Raycast(origin + new Vector3(radius / 2, 0, -radius / 2), direction, out landObject, twoThirdDistance, layerDetection) ||
            Physics.Raycast(origin + new Vector3(-radius / 2, 0, -radius / 2), direction, out landObject, twoThirdDistance, layerDetection))
            newIsAirborne = false;
        else
            newIsAirborne = true;

        // Determine if landed
        if (wasAirborne && !newIsAirborne)
            OnLanded(targetVelocity);

        wasAirborne = newIsAirborne;

        return newIsAirborne;
    }

    private void SetupFMOD()
    {
        FM_soundSystem = FMOD_StudioSystem.instance;

        // Load in banks if necessary
        if (!FM_soundSystem.System.isValid())
        {
            string fileName = Application.dataPath + @"/StreamingAssets/Master Bank.bank";
            Bank bank, bankStrings;
            FMOD_StudioSystem.ERRCHECK(FM_soundSystem.System.loadBankFile(fileName, LOAD_BANK_FLAGS.NORMAL, out bank));
            FMOD_StudioSystem.ERRCHECK(FM_soundSystem.System.loadBankFile(fileName + ".strings", LOAD_BANK_FLAGS.NORMAL, out bankStrings));
        }

        // Find events
        FM_jump = FM_soundSystem.GetEvent("event:/Sound Effects/Player/Jump");
        FM_land = FM_soundSystem.GetEvent("event:/Sound Effects/Player/Land");
        FM_footstep = FM_soundSystem.GetEvent(string.Format("event:/Sound Effects/Player/P{0} Footstep", isPlayerOne ? "1" : "2"));

        // Set parameters
        FM_land.getParameter("PlayerFallSpeed", out FM_playerFallSpeed);
        FM_footstep.getParameter(string.Format("Player{0}MoveSpeed", isPlayerOne ? "1" : "2"), out FM_playerMovespeed);
    }

    private ATTRIBUTES_3D GetFMODAttribute(Transform transform, Vector3 velocity)
    {
        var FM_attribute = new ATTRIBUTES_3D();
        FM_attribute.position = transform.position;
        FM_attribute.velocity = velocity;
        FM_attribute.forward = transform.forward;
        FM_attribute.up = transform.up;
        return FM_attribute;
    }

    private void PlaySound(EventInstance FM_event, AudioClip audioClip, bool condition = true)
    {
        if (!condition) return;

        if (useFMOD)
        {
            if (FM_event != null)
                FM_event.start();
        }
        else { audioSource.PlayClip(audioClip); }
    }

    #endregion

    #region STATICS

    // Returns the other character
    private static Character GetOtherCharacter(bool player1)
    {
        if (player1)
            return character2;
        else
            return character1;
    }

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

    #endregion
}