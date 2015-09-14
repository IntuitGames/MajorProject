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
    private const int HIGH = 1000;
    private const int MEDIUM = 100;
    private const int LOW = 10;

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
    public float sprintMoveSpeed = 11;
    public AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [Range(0, HIGH)]
    public float jumpPower = 10;
    [Range(0, MEDIUM)]
    public float rotationSpeed = 10;
    public float maxSpeed = 50;
    [Range(-MEDIUM, 0)]
    public float normalGravity = -9.8f;
    [Range(-MEDIUM, 0)]
    public float maxGravity = -50;
    [Range(0, MEDIUM)]
    public float gravityGrowthRate = 3;
    public LayerMask groundedLayers;
    public PhysicMaterial normalMaterial;

    // DASH
    [Header("Dash"), SerializeField]
    private bool _canDash = true;
    public float dashPower = 25;
    [Tooltip("In seconds (Will try to make it distance soon)"), Range(0, 3)]
    public float dashLength = 0.5f;
    [Range(0, LOW)]
    public float dashHeight = 3;
    private TimerPlus dashTimer;
    public bool stopDashOnCollision = true;
    [Range(0, LOW)]
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
    [Range(0, MEDIUM)]
    public float heavyJumpPower = 20;
    [Range(0, HIGH)]
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
    [Range(0, LOW)]
    public float bouncePower = 1;
    [Range(0, LOW)]
    public float jumpBouncePower = 1.5f;
    public bool canGroundBounce = true;
    public float groundBounceThreshold = 10;
    [Range(0, LOW)]
    public float groundBouncePower = 0.5f;
    public float minGroundBounceMagnitude = 5;
    public float maxGroundBounceMagnitude = 10;

    // AUDIO
    [Header("Audio")]
    public CharacterAudio audioData = new CharacterAudio();
    private float FM_playerFallSpeedValue
    {
        get { return targetVelocity.y < 0 ? Mathf.Lerp(0, 1, Mathf.Abs(targetVelocity.y) / (maxSpeed * 0.5f)) : 0; }
    }
    private float FM_playerMovespeedValue
    {
        get { return targetVelocity.IgnoreY2().magnitude; }
    }

    // PRIVATES
    private float gravity;                              // Current gravity on this character.
    private RaycastHit onObject;                        // Which object is currently under this character.
    private const float airborneRayOffset = 0.05f;      // How much additional height offset will the ray checks account for.
    private float jumpTime;                             // How long the jump button has been held in for.
    private bool jumpFlag;                              // Is the character ready to jump again?

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
            if (isSprinting) value = sprintMoveSpeed;
            if (isHeavy && isSprinting) value = (heavyMoveSpeed / baseMoveSpeed) * sprintMoveSpeed;
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
    public bool isSprinting { get; set; }

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

        // Setup audio data
        audioData.Initialize(transform, rigidBody);
    }

    void OnDestroy()
    {
        // Dispose of FMOD instances
        audioData.Dispose();
    }

    void OnCollisionEnter(Collision col)
    {
        audioData.PlayLandAudio(FM_playerFallSpeedValue, col.relativeVelocity.magnitude > 1);

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

        // Add to gravity
        if (!isGrounded) gravity = Mathf.Clamp(gravity - gravityGrowthRate * Time.deltaTime, maxGravity, 0);
        else gravity = normalGravity;

        // Apply any changes to the dash length and cooldown
        dashTimer.Length = dashLength;
        dashCooldownTimer.Length = dashCooldown;

        // Check for bounce
        if (!isBouncing && onObject.collider && onObject.collider.GetComponent<Bouncy>()) Bounce(Vector3.one, onObject.collider.gameObject);

        // Apply heavy downward force and physics materials
        capsuleCollider.material = isHeavy ? heavyMaterial : normalMaterial;
        if (isHeavy) rigidBody.AddForce(Vector3.down * (heavyDownwardForce * 50) * delta, ForceMode.Force);
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

        // Apply gravity
        rigidBody.AddForce(new Vector3(0, gravity, 0), ForceMode.Force);

        // Send animator info
        animator.SetBool("IsAirborne", !isGrounded);
        animator.SetFloat("Speed", targetVelocity.IgnoreY2().normalized.magnitude * (moveSpeed / baseMoveSpeed));
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

        audioData.PlayJumpAudio(jumpTime <= GameManager.InputManager.jumpDelta);

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

    public void Sprint(bool isPressed)
    {
        isSprinting = isPressed;
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
        audioData.PlayWalkAudio(FM_playerMovespeedValue, isWalking && isGrounded);
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

    // Casts multiple rays and outs the result
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

    // Resets the jump flag so the character can respond to jump input again
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

    #region NESTED

    /// <summary>
    /// Stores all audio related data.
    /// </summary>
    [System.Serializable]
    public class CharacterAudio : System.IDisposable
    {
        #region VARIABLES

        public bool enabled = true;
        [Range(0, 1)]
        public float volume = 1;

        // COMPONENTS
        public AudioSource audioSource;
        private Transform transform;
        private Rigidbody rigidbody;

        // SOUND EFFECTS
        [Header("Sound Effects")]
        public SoundClip walk = new SoundClip();
        public SoundClip land = new SoundClip();
        public SoundClip jump = new SoundClip();

        // PRIVATES
        private bool isInitialized;

        #endregion

        #region METHODS

        // Initializes components and sound clips
        public void Initialize(Transform transformObj, Rigidbody rigidbodyObj)
        {
            transform = transformObj; rigidbody = rigidbodyObj;

            walk.Initialize();
            land.Initialize();
            jump.Initialize();

            isInitialized = true;
        }

        public void PlayWalkAudio(float moveSpeed, bool condition = true)
        {
            if (!condition || !isInitialized || !enabled) return;

            walk.Play(audioSource, AudioManager.GetFMODAttribute(transform, rigidbody.velocity), volume, parameters: moveSpeed);
        }

        public void PlayLandAudio(float fallSpeed, bool condition = true)
        {
            if (!condition || !isInitialized || !enabled) return;

            land.Play(audioSource, AudioManager.GetFMODAttribute(transform, rigidbody.velocity), volume, parameters: fallSpeed);
        }

        public void PlayJumpAudio(bool condition = true)
        {
            if (!condition || !isInitialized || !enabled) return;

            jump.Play(audioSource, AudioManager.GetFMODAttribute(transform, rigidbody.velocity), volume);
        }

        // Dispose FMOD instances
        public void Dispose()
        {
            walk.Dispose();
            land.Dispose();
            jump.Dispose();
        }

        #endregion
    }

    #endregion
}
