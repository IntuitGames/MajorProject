using UnityEngine;
using FMOD.Studio;
using CustomExtensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource), typeof(CapsuleCollider))]
public class Character : MonoBehaviour, IBounce
{
    #region VARIABLES

    // CONSTANTS
    private const int FORCE_MAX = 1000;
    private const int POWER_MAX = 10;

    // STATICS
    public static Character character1 { get; private set; }
    public static Character character2 { get; private set; }

    // COMPONENTS
    [HideInInspector]
    public Rigidbody rigidBody;
    [HideInInspector]
    public AudioSource audioSource;
    [HideInInspector]
    public CapsuleCollider capsuleCollider;
    [HideInInspector]
    public Animator animator;

    // BASIC STATS
    [SerializeField, Popup(new string[2] { "Player 1", "Player 2" }, OverrideName = "Player"), Header("Basic")]
    private bool _isPlayerOne = true;
    [ReadOnly]
    public Vector3 targetVelocity;
    public float baseMoveSpeed = 7;
    public AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [Range(0, FORCE_MAX)]
    public float jumpPower = 10;
    [Range(0, POWER_MAX)]
    public float rotationSpeed = 10;
    public float maxSpeed = 50;
    public LayerMask groundedLayers;
    public PhysicMaterial normalMaterial;

    // DASH
    [Header("Dash"), SerializeField]
    private bool _canDash = true;
    public float dashPower = 25;
    [Tooltip("In seconds (Will try to make it distance soon)"), Range(0, 3)]
    public float dashLength = 0.5f;
    [Range(0, POWER_MAX)]
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
    [Range(0, POWER_MAX * 5)]
    public float heavyJumpPower = 20;
    [Range(0, FORCE_MAX)]
    public float heavyDownwardForce = 100;
    [Range(0, 90)]
    public float heavySlopeAngle = 0.2f;
    public PhysicMaterial heavyMaterial;
    public bool canBounceWhileHeavy = false;

    // BOUNCE
    [Header("Bounce")]
    public bool canBounce = true;
    [Range(0, 1)]
    public float momentumRetention = 0.8f;
    [Range(0, POWER_MAX)]
    public float bouncePower = 1;
    [Range(0, POWER_MAX)]
    public float jumpBouncePower = 1.5f;
    public bool canGroundBounce = true;
    public float groundBounceThreshold = 10;
    [Range(0, POWER_MAX)]
    public float groundBouncePower = 0.5f;
    public float minGroundBounceMagnitude = 5;
    public float maxGroundBounceMagnitude = 10;

    // AUDIO
    [Header("Audio")]
    public List<AudioClip> walkSounds = new List<AudioClip>();
    public List<AudioClip> jumpSounds = new List<AudioClip>();
    public List<AudioClip> dashSounds = new List<AudioClip>();
    public List<AudioClip> landSounds = new List<AudioClip>();
    public List<AudioClip> bounceSounds = new List<AudioClip>();

    // F-MOD
    [Header("FMOD")]
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
    [Range(0, 1)]
    public float jumpVolume = 0.5f, landVolume = 1, footstepVolume = 1;

    // PRIVATES
    private RaycastHit onObject;                        // Which object is currently under this character.
    private const float airborneRayOffset = 0.05f;      // How much additional height offset will the ray checks account for.
    private float jumpTime;                             // How long the jump button has been held in for.
    private bool jumpFlag;

    // PROPERTIES
    public bool isPlayerOne
    {
        get { return _isPlayerOne; }
        set
        {
            _isPlayerOne = value;
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
    public float slopeAngle
    {
        get { return Vector3.Angle(Vector3.up, onObject.normal); }
    }

    // STATES
    public bool isWalking
    {
        get
        {
            return new Vector2(targetVelocity.x, targetVelocity.z).magnitude > 0;
        }
    }
    public bool isGrounded { get; set; }
    public bool isFalling
    {
        get
        {
            return !isGrounded && targetVelocity.y < 0;
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

    #endregion

    #region MESSAGES

    void Awake()
    {
        // Set self in static
        if (!SetStaticCharacter(this)) DestroyImmediate(gameObject);

        // Find component references
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        // Setup up character input depending on whether this is character 1 or 2
        GameManager.InputManager.SetupCharacterInput(this);

        // Setup dash timers
        dashTimer = TimerPlus.Create(dashLength, TimerPlus.Presets.Standard);
        dashCooldownTimer = TimerPlus.Create(dashCooldown, TimerPlus.Presets.Standard);

        // Setup FMOD
        SetupFMOD();
    }

    void OnDisabled()
    {
        // Dispose FMOD instances
        FM_jump.stop(STOP_MODE.IMMEDIATE);
        FM_jump.release();

        FM_land.stop(STOP_MODE.IMMEDIATE);
        FM_land.release();

        FM_footstep.stop(STOP_MODE.IMMEDIATE);
        FM_footstep.release();
    }

    void OnCollisionEnter(Collision col)
    {
        PlaySound(FM_land, landSounds.Random(), col.relativeVelocity.magnitude > 1);

        // Bounce off ground
        if (!col.collider.GetComponent<Bouncy>()) gameObject.GetInterface<IBounce>().Bounce(col.relativeVelocity, col.collider.gameObject);

        // Stop dash on collision 
        if (stopDashOnCollision && col.contacts[0].normal.z != 0) isDashing = false;
    }

    #endregion

    #region INPUT

    // Is called BEFORE input is checked every frame
    public void PreInputUpdate(float delta)
    {
        if (!this.enabled) return;

        // Check for airborne changes
        isGrounded = GroundedRayCheck(transform.position, Vector3.down, airborneRayOffset, out onObject);

        // Apply any changes to the dash length and cooldown
        dashTimer.Length = dashLength;
        dashCooldownTimer.Length = dashCooldown;

        // Check for bounce
        if (!isBouncing && onObject.collider && onObject.collider.GetComponent<Bouncy>()) Bounce(Vector3.one, onObject.collider.gameObject);

        // Apply heavy downward force and physics materials
        capsuleCollider.material = isHeavy ? heavyMaterial : normalMaterial;
        if (isHeavy) rigidBody.AddForce(Vector3.down * (heavyDownwardForce * 50) * delta, ForceMode.Force);

        // Update FMOD parameters
        if (FM_playerFallSpeed != null) FM_playerFallSpeed.setValue(FM_playerFallSpeedValue);
        if (FM_playerMovespeed != null) FM_playerMovespeed.setValue(FM_playerMovespeedValue);

        var FM_attributes = GetFMODAttribute(transform, rigidBody.velocity);

        if (FM_land != null) FM_land.set3DAttributes(FM_attributes);
        if (FM_jump != null) FM_jump.set3DAttributes(FM_attributes);
        if (FM_footstep != null) FM_footstep.set3DAttributes(FM_attributes);

        if (FM_land != null) FM_land.setVolume(landVolume);
        if (FM_jump != null) FM_jump.setVolume(jumpVolume);
        if (FM_footstep != null) FM_footstep.setVolume(footstepVolume);
    }

    // Is called AFTER input is determined every frame
    public void PostInputUpdate(float delta)
    {
        if (!this.enabled) return;

        // Clamp movement velocity
        targetVelocity = Vector3.ClampMagnitude(targetVelocity, maxSpeed);

        // Rotate in movement direction
        if(targetVelocity != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetVelocity), delta * rotationSpeed);

        // Apply movement
        rigidBody.MovePosition(transform.position + targetVelocity * delta);

        // Send animator info
        animator.SetBool("IsAirborne", !isGrounded);
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

    public void Jump(bool isPressed)
    {
        jumpTime += GameManager.InputManager.jumpDelta;

        if (isBouncing || !isPressed || jumpTime > jumpCurve.Duration()) ResetJumpFlag();

        if (!jumpFlag) return;

        PlaySound(FM_jump, jumpSounds.Random(), jumpTime <= GameManager.InputManager.jumpDelta);

        if (!isHeavy)
        {
            rigidBody.AddForce(Vector3.up * jumpCurve.Evaluate(jumpTime) * (jumpPower / jumpCurve.Duration()) * 7.5f * GameManager.InputManager.jumpDelta, ForceMode.Force);
        }
        else if (isGrounded)
        {
            rigidBody.AddForce(Vector3.up * heavyJumpPower, ForceMode.Impulse);
            ResetJumpFlag();
        }
    }

    public void JumpToggle(bool isPressed)
    {
        if(isPressed && isGrounded) jumpFlag = true;
    }

    public void Dash(bool isPressed)
    {
        if (isPressed && canDash)
        {
            isDashing = true;

            if (!useFMOD) audioSource.PlayClip(dashSounds.Random());

            if (isGrounded) targetVelocity.y += dashHeight;
        }
    }

    public void Heavy(bool isPressed)
    {
        if (!canHeavy)
        {
            isHeavy = false;
            return;
        }

        if (!canUnheavyMidair && isHeavy && !isPressed && !isGrounded)
            return;
        else
        {
            isHeavy = isPressed;
        }
    }

    public void Pause(bool isPressed)
    {
        // Reloads the level for now
        if(isPlayerOne && isPressed) TimerPlus.Create(0.25f, () => Application.LoadLevel(Application.loadedLevel));
    }

    #endregion

    #region ACTIONS & EVENTS

    private void PerformBounce(Vector3 direction, float magnitude)
    {
        if (!canBounce || magnitude <= 0 || !canBounceWhileHeavy && isHeavy)
        {
            isBouncing = false;
        }
        else
        {
            isBouncing = true;
            rigidBody.AddForce(direction * magnitude, ForceMode.Impulse);
        }
    }

    public void OnFootStep(int footIndex) // 1 left, 2 right
    {
        PlaySound(FM_footstep, walkSounds.Random(), isWalking && isGrounded);
    }

    #endregion

    #region INTERFACE MEMBERS

    public void Bounce(Vector3 relativeVelocity, GameObject bounceObject)
    {
        Bouncy bouncyObj = bounceObject.GetComponent<Bouncy>();

        // Bounce direction is currently always up due to some issues with side-collisions not acting properly.
        Vector3 bounceDirection = Vector3.up;

        float baseBouncePower, bounceMultiplier, minBouncePower, maxBouncePower;

        // Bouncing on a bouncy object
        if (bouncyObj && relativeVelocity.magnitude > bouncyObj.velocityThreshold && bouncyObj.isBouncy)
        {
            // Do not bounce if other character is bouncing on this character
            if (bouncyObj.gameObject == GetOtherCharacter(isPlayerOne).gameObject && GetOtherCharacter(isPlayerOne).transform.position.y > transform.position.y) return;
            baseBouncePower = bouncePower;
            bounceMultiplier = bouncyObj.bounceMultiplier * (relativeVelocity.magnitude * momentumRetention);
            minBouncePower = bouncyObj.minBounceMagnitude * (GameManager.InputManager.IsRequestingJump(isPlayerOne) ? jumpBouncePower : 1);
            maxBouncePower = bouncyObj.maxBounceMagnitude;
            PerformBounce(bounceDirection, Mathf.Clamp(baseBouncePower * bounceMultiplier, minBouncePower, maxBouncePower));
        }
        // Bouncing on the ground
        else if (canGroundBounce && relativeVelocity.magnitude > groundBounceThreshold)
        {
            baseBouncePower = groundBouncePower;
            bounceMultiplier = (relativeVelocity.magnitude * momentumRetention);
            minBouncePower = minGroundBounceMagnitude; maxBouncePower = maxGroundBounceMagnitude;
            PerformBounce(bounceDirection, Mathf.Clamp(groundBouncePower * (relativeVelocity.magnitude * momentumRetention), minGroundBounceMagnitude, maxGroundBounceMagnitude));
        }
        else
            isBouncing = false;
    }

    GameObject IUnityInterface.gameObject
    {
        get { return this.gameObject; }
    }

    #endregion

    #region HELPERS

    // Use ray casts to determine if the character is airborne
    private bool GroundedRayCheck(Vector3 origin, Vector3 direction, float offset, out RaycastHit groundObject)
    {
        float scaledHeight = transform.localScale.y * (capsuleCollider.height / 2);
        float scaledRadius = transform.localScale.y * (capsuleCollider.radius / 2);

        Vector3[] origins = new Vector3[5]
            {
                origin,
                origin + new Vector3(scaledRadius, 0, 0),
                origin + new Vector3(-scaledRadius, 0, 0),
                origin + new Vector3(0, 0, scaledRadius),
                origin + new Vector3(0, 0, -scaledRadius)
            };
        float[] lengths = new float[5]
            {
                scaledHeight + offset,
                (scaledHeight + offset) * 0.9f,
                (scaledHeight + offset) * 0.9f,
                (scaledHeight + offset) * 0.9f,
                (scaledHeight + offset) * 0.9f,
            };
        return MultiRayCheck(origins, direction, lengths, out groundObject, true);
    }

    private bool MultiRayCheck(Vector3[] origins, Vector3 direction, float[] lengths, out RaycastHit groundObject, bool draw = false)
    {
        groundObject = default(RaycastHit);
        int index = -1;
        do
        {
            index++;
            if (Mathf.Min(origins.Length, lengths.Length) <= index) return false;
            if (draw) Debug.DrawRay(origins[index], direction * lengths[index], Color.red, 0);
        }
        while (!Physics.Raycast(origins[index], direction, out groundObject, lengths[index], groundedLayers));
        return true;
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

    private void ResetJumpFlag()
    {
        jumpFlag = false;
        jumpTime = 0;
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
