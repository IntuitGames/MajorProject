using UnityEngine;
using FMOD.Studio;
using CustomExtensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody), typeof(CharacterAudio), typeof(CapsuleCollider)), SelectionBase]
public class Character : MonoBehaviour, IBounce
{
    #region VARIABLES

    // STATICS
    public static Character character1 { get; private set; }
    public static Character character2 { get; private set; }

    // COMPONENTS
    [HideInInspector]
    public Rigidbody rigidbodyComp;
    [HideInInspector]
    public CapsuleCollider capsuleCollider;
    [HideInInspector]
    public Animator animator;
    [HideInInspector]
    public CharacterAudio audioData;
    public static SmoothCameraFollow cameraFollow;

    // BASIC STATS
    [SerializeField, Popup(new string[2] { "Player 1", "Player 2" }, OverrideName = "Player"), Header("Basic")]
    private bool _isPlayerOne = true;
    [ReadOnly]
    public Vector3 targetVelocity;
    public float baseMoveSpeed = 7;
    public float sprintMoveSpeed = 11;
    [Range(0, MEDIUM)]
    public float rotationSpeed = 50;
    [Range(0, MEDIUM)]
    public float sprintRotationSpeed = 20;
    public float maxSpeed = 50;
    [Range(-MEDIUM, 0)]
    public float normalGravity = -9.8f;
    [Range(-MEDIUM, 0)]
    public float maxGravity = -50;
    [Range(0, MEDIUM)]
    public float gravityGrowthRate = 3;
    public LayerMask groundedLayers;
    public PhysicMaterial normalMaterial;
    [Range(0, LOW)]
    public float normalMass = 1;

    // JUMP
    [Header("Jump"), SerializeField]
    public bool canJump = true;
    public AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [Range(0, MEDIUM), Tooltip("The one-time force applied at the start of the jump.")]
    public float jumpImpulse = 10;
    [Range(0, HIGH), Tooltip("The jump curve force multiplier applied mid jump.")]
    public float jumpForce = 250;
    public bool doJumpMomentum = true;
    [Range(0, LOW), Tooltip("Movement before the jump will influence the impulse direction.")]
    public float jumpMomentum = 0.5f;
    [Range(0, LOW)]
    public float sprintJumpMomentum = 2;
    [Range(0, 1)]
    public float aerialControl = 0.75f;
    [Range(0, MEDIUM)]
    public float aerialRotationSpeed = 10;

    // TETHER
    [Header("Tether")]
    public bool constrainMovement = true;
    [Range(0, MEDIUM)]
    public float constrainingPower = 20;
    [Range(0, MEDIUM)]
    public float freeMovementLength = 8;
    [Range(0, MEDIUM)]
    public float maxDistanceLength = 15;

    // WEAKENED
    [Header("Weakened")]
    public bool canWeaken = true;
    [Range(0, 1)]
    public float weakenedMoveSpeedMulti = 0.5f;
    public bool reconnectOnTouch = true;
    public float minCamZoomDistance = 5;
    public float maxCamZoomDistance = 20;
    public float minCamProximity = 3;
    public float maxCamProximity = 20;

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
    [Header("Heavy"), SerializeField]
    private bool _canHeavy = true;
    public float heavyMoveSpeed = 2;
    public bool canUnheavyMidair = false;
    [Range(0, MEDIUM)]
    public float heavyJumpPower = 20;
    [Range(0, HIGH)]
    public float heavyDownwardForce = 100;
    [Range(0, 90)]
    public float heavySlopeAngle = 0.2f;
    [Range(0, MEDIUM)]
    public float heavyRotationSpeed = 10;
    public PhysicMaterial heavyMaterial;
    [Range(0, LOW)]
    public float heavyMass = 5;
    public bool canBounceWhileHeavy = false;
    [Range(0, LOW)]
    public float exitHeavyDragMulti = 5;
    public float exitHeavyDragDuration = 2;
    private TimerPlus exitHeavyDragTimer;
    [Range(0, LOW)]
    public float minHeavyDuration = 1;
    [Range(0, LOW)]
    public float heavyCooldown = 0;
    private TimerPlus heavyCooldownTimer;
    public bool canHeavy
    {
        get { return _canHeavy && !heavyCooldownTimer.IsPlaying; }
        set { _canHeavy = value; }
    }
    private TimerPlus unheavyDurationTimer;
    public bool canUnheavy
    {
        get { return (canUnheavyMidair ? true : isGrounded) && !unheavyDurationTimer.IsPlaying; }
    }

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

    // FMOD PARAMS
    private float FM_playerFallSpeedValue
    {
        get { return targetVelocity.y < 0 ? Mathf.Lerp(0, 1, Mathf.Abs(targetVelocity.y) / (maxSpeed * 0.5f)) : 0; }
    }
    private float FM_playerMovespeedValue
    {
        get { return Mathf.Lerp(0, 1, currentMoveSpeed / sprintMoveSpeed); }
    }

    // PRIVATES
    private float normalDrag;                           // Normal rigidbody drag.
    private Color normalColour;                         // Normal material full colour.
    private Color heavyColor;                           // Heavy material full colour.
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
    public float currentMoveSpeed
    {
        get
        {
            float value = baseMoveSpeed;
            if (isHeavy) value = heavyMoveSpeed;
            if (isSprinting && isGrounded) value = sprintMoveSpeed;
            if (isHeavy && isSprinting && isGrounded) value = (heavyMoveSpeed / baseMoveSpeed) * sprintMoveSpeed;
            if (!isGrounded) value *= aerialControl;
            if (isWeakened) value *= weakenedMoveSpeedMulti;
            return value;
        }
    }
    public float slopeAngle
    {
        get { return Vector3.Angle(Vector3.up, onObject.normal); }
    }
    public float currentMass
    {
        get { return isHeavy ? heavyMass : normalMass; }
    }
    public float currentRotationSpeed
    {
        get { return isGrounded ? (isSprinting ? sprintRotationSpeed : (isHeavy ? heavyRotationSpeed : rotationSpeed)) : aerialRotationSpeed; }
    }
    public float currentJumpMomentum
    {
        get { return isSprinting ? sprintJumpMomentum : jumpMomentum; }
    }

    // FLAGS
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
    public bool isWeakened { get; set; }
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

    // CONSTANTS
    private const int HIGH = 1000;
    private const int MEDIUM = 100;
    private const int LOW = 10;

    #endregion

    #region MESSAGES

    void Awake()
    {
        // Set self in static
        if (!SetStaticCharacter(this)) DestroyImmediate(gameObject);

        // Find component references
        rigidbodyComp = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        animator = GetComponentInChildren<Animator>();
        audioData = GetComponent<CharacterAudio>();
        cameraFollow = Camera.main.GetComponent<SmoothCameraFollow>();
    }

    void Start()
    {
        // Setup up character input depending on whether this is character 1 or 2
        GameManager.InputManager.SetupCharacterInput(this);

        // Setup dash timers
        dashTimer = TimerPlus.Create(dashLength, TimerPlus.Presets.Standard);
        dashCooldownTimer = TimerPlus.Create(dashCooldown, TimerPlus.Presets.Standard);
        heavyCooldownTimer = TimerPlus.Create(heavyCooldown, TimerPlus.Presets.Standard);
        unheavyDurationTimer = TimerPlus.Create(minHeavyDuration, TimerPlus.Presets.Standard);
        exitHeavyDragTimer = TimerPlus.Create(exitHeavyDragDuration, TimerPlus.Presets.Standard, () => rigidbodyComp.drag = normalDrag);

        // Subscribe to the tether events
        if (isPlayerOne)
        {
            GameManager.TetherManager.OnDisconnected += Weaken;
            GameManager.TetherManager.OnReconnected += Unweaken;
        }

        // Set debug heavy colours
        normalColour = GetComponentInChildren<Renderer>().material.color;
        heavyColor = normalColour * 0.1f;
    }

    void Update()
    {
        // Check for airborne changes
        isGrounded = GroundedRayCheck(transform.position, Vector3.down, airborneRayOffset, out onObject);

        // Apply any changes to timer lengths
        dashTimer.ModifyLength(dashLength);
        dashCooldownTimer.ModifyLength(dashCooldown);
        heavyCooldownTimer.ModifyLength(heavyCooldown);
        unheavyDurationTimer.ModifyLength(minHeavyDuration);
        exitHeavyDragTimer.ModifyLength(exitHeavyDragDuration);

        // Update drag value
        if (!exitHeavyDragTimer.IsPlaying)
            normalDrag = rigidbodyComp.drag;

        // Update weakened camera zoom
        if (isPlayerOne && isWeakened)
        {
            cameraFollow.distance = Vector3.Distance(transform.position, GetPartnerPosition()).Normalize(minCamProximity, maxCamProximity, minCamZoomDistance, maxCamZoomDistance);
        }
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

        // Reconnect on touch
        if (reconnectOnTouch && col.collider.gameObject == GetPartner().gameObject)
            GameManager.TetherManager.Reconnect();
    }

    #endregion

    #region INPUT

    // Is called BEFORE input is checked every frame
    public void PreInputUpdate(float delta)
    {
        if (!this.enabled) return;

        // Update mass
        if(rigidbodyComp.mass != currentMass) rigidbodyComp.mass = currentMass;

        // Add to gravity
        if (!isGrounded) gravity = Mathf.Clamp(gravity - gravityGrowthRate * Time.deltaTime, maxGravity, 0);
        else gravity = normalGravity;

        // Check for bounce
        if (!isBouncing && onObject.collider && onObject.collider.GetComponent<Bouncy>()) Bounce(Vector3.one, onObject.collider.gameObject);

        // Apply heavy downward force and physics materials
        capsuleCollider.material = isHeavy ? heavyMaterial : normalMaterial;
        if (isHeavy) AddConstrainedForce(Vector3.down * (heavyDownwardForce * 50) * delta, ForceMode.Force);
    }

    // Is called AFTER input is determined every frame
    public void PostInputUpdate(float delta)
    {
        if (!this.enabled) return;

        // Clamp movement velocity
        targetVelocity = Vector3.ClampMagnitude(targetVelocity, maxSpeed);

        // Rotate in movement direction
        if(targetVelocity != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetVelocity), delta * currentRotationSpeed);

        // Apply movement
        AddConstrainedMovement(targetVelocity * delta);

        // Apply gravity
        AddConstrainedForce(new Vector3(0, gravity, 0), ForceMode.Force);

        // Apply constraining force
        if(constrainMovement) ApplyConstrainForce();

        // Send animator info
        animator.SetBool("IsAirborne", !isGrounded);
        animator.SetFloat("Speed", targetVelocity.IgnoreY2().normalized.magnitude * (currentMoveSpeed / baseMoveSpeed));
    }

    public void Movement(float forward, float right)
    {
        Vector2 direction = Vector2.ClampMagnitude(new Vector2(right, forward), 1);

        if (!isDashing)
        {
            targetVelocity.x = direction.x * currentMoveSpeed;
            targetVelocity.z = direction.y * currentMoveSpeed;
        }
        else
        {
            targetVelocity.x = transform.forward.x * dashPower;
            targetVelocity.z = transform.forward.z * dashPower;
        }
    }

    public void Jump(bool isPressed)
    {
        if (!canJump) return;

        jumpTime += GameManager.InputManager.jumpDelta;

        if (isBouncing || !isPressed || jumpTime > jumpCurve.Duration() || jumpTime > 0.1f && isGrounded) ResetJumpFlag();

        if (!jumpFlag) return;

        audioData.PlayJumpAudio(jumpTime <= GameManager.InputManager.jumpDelta);

        if (!isHeavy)
        {
            AddConstrainedForce(Vector3.up * jumpCurve.Evaluate(jumpTime) * (jumpForce / jumpCurve.Duration()) * GameManager.InputManager.jumpDelta, ForceMode.Force);
        }
        else if (isGrounded)
        {
            AddConstrainedForce(Vector3.up * heavyJumpPower * jumpImpulse, ForceMode.Impulse);
            ResetJumpFlag();
        }
    }

    public void JumpToggle(bool isPressed)
    {
        if (isPressed && isGrounded)
        {
            jumpFlag = true;

            if (isHeavy) return;

            if (!doJumpMomentum || targetVelocity.IgnoreY2() == Vector2.zero)
                AddConstrainedForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
            else
            {
                Vector3 jumpVector = targetVelocity.normalized * currentJumpMomentum;
                jumpVector.y = 10;
                AddConstrainedForce(jumpVector.normalized * jumpImpulse, ForceMode.Impulse);
            }
        }
        else if (!isPressed && jumpTime < 0.1f)
        {
            rigidbodyComp.velocity = new Vector3(rigidbodyComp.velocity.x, 0, rigidbodyComp.velocity.z);
        }
    }

    public void Dash(bool isPressed)
    {
        if (isPressed && canDash)
        {
            isDashing = true;

            if (isGrounded) targetVelocity.y += dashHeight;

            audioData.PlayDashAudio();
        }
    }

    public void Heavy(bool isPressed)
    {
        if (isPressed == isHeavy) return;

        if (isPressed && canHeavy)
        {
            isHeavy = true;

            audioData.PlayStartHeavyAudio();
            unheavyDurationTimer.Restart();

            GetComponentInChildren<Renderer>().material.color = heavyColor;
        }
        else if (!isPressed && canUnheavy)
        {
            isHeavy = false;

            audioData.PlayEndHeavyAudio();
            heavyCooldownTimer.Restart();

            if (isGrounded) // Reduce sliding down slopes while not heavy
            {
                rigidbodyComp.drag *= exitHeavyDragMulti;
                exitHeavyDragTimer.Restart();
            }

            GetComponentInChildren<Renderer>().material.color = normalColour;
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
            AddConstrainedForce(direction * magnitude, ForceMode.Impulse);
        }
    }

    public void OnFootStep(int footIndex) // 1 left, 2 right
    {
        audioData.PlayWalkAudio(FM_playerMovespeedValue, isWalking && isGrounded);
    }

    public void ApplyConstrainForce()
    {
        if (isWeakened) return;

        float length = GameManager.TetherManager ? GameManager.TetherManager.tetherLength : Vector3.Distance(transform.position, GetPartnerPosition());

        if (length < freeMovementLength) return;

        float alpha = Mathf.Lerp(0, 1, (length - freeMovementLength) / (maxDistanceLength - freeMovementLength));

        Vector3 direction = GameManager.TetherManager ? GameManager.TetherManager.GetStartAndEndMoveDirection(isPlayerOne) : (GetPartnerPosition() - transform.position).normalized;

        rigidbodyComp.AddForce(direction * alpha * constrainingPower, ForceMode.Impulse);
    }

    public void AddConstrainedForce(Vector3 movement, ForceMode forceMode)
    {
        if (!constrainMovement) rigidbodyComp.AddForce(movement, forceMode);

        rigidbodyComp.AddForce(movement, forceMode);
    }

    public void AddConstrainedMovement(Vector3 movement)
    {
        if (!constrainMovement) rigidbodyComp.MovePosition(transform.position + movement);

        rigidbodyComp.MovePosition(transform.position + movement);
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
            if (bouncyObj.gameObject == GetPartner().gameObject && GetPartnerPosition().y > transform.position.y) return;
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

    // Returns the other character
    public Character GetPartner()
    {
        if (this == character1) return character2;
        else return character1;
    }

    // Returns the other character position
    public Vector3 GetPartnerPosition()
    {
        return GetPartner().transform.position;
    }

    #endregion

    #region STATICS

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

    // Weakens both characters
    public static void Weaken(TetherJoint brokenJoint)
    {
        if (character1.canWeaken)
        {
            character1.isWeakened = true;
            character1.audioData.PlayTetherDisconnectAudio(brokenJoint.transform);
        }

        if (character2.canWeaken)
        {
            character2.isWeakened = true;
        }
    }

    // Unweakens both characters
    public static void Unweaken(TetherJoint reconnectedJoint)
    {
        character1.isWeakened = false;
        character2.isWeakened = false;

        character1.audioData.PlayTetherConnectAudio(reconnectedJoint.transform);

        // Reset camera zoom
        cameraFollow.distance = cameraFollow.initialDistance;
    }

    #endregion
}
