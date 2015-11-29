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

    // COMPONENTS
    [Header("Components")] // Visible because I want valid values before Awake()
    public Rigidbody rigidbodyComp;
    public CapsuleCollider colliderComp;
    public Animator animatorComp;
    public CharacterAudio audioDataComp;
    public SkinnedMeshRenderer bodyRendererComp;
    public MeshRenderer maskRendererComp;

    // BASIC STATS
    [SerializeField, Popup(new string[2] { "Player 1", "Player 2" }, OverrideName = "Player"), Header("Basic")]
    private bool _isPlayerOne = true;
    [ReadOnly]
    public Vector3 targetVelocity;
    public float baseMoveSpeed = 7;
    public float sprintMoveSpeed = 11;
    [Range(0, 100)]
    public float rotationSpeed = 50;
    [Range(0, 100)]
    public float sprintRotationSpeed = 20;
    public float maxSpeed = 50;
    [Range(-100, 0)]
    public float normalGravity = -9.8f;
    [Range(-100, 0)]
    public float maxGravity = -50;
    [Range(0, 100)]
    public float gravityGrowthRate = 3;
    public LayerMask groundedLayers;
    public PhysicMaterial normalMaterial;
    [Range(0, 10)]
    public float normalMass = 1;

    // JUMP
    [Header("Jump"), SerializeField]
    public bool canJump = true;
    public AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [Range(0, 100), Tooltip("The one-time force applied at the start of the jump.")]
    public float jumpImpulse = 10;
    [Range(0, 1000), Tooltip("The jump curve force multiplier applied mid jump.")]
    public float jumpForce = 250;
    public bool doJumpMomentum = true;
    [Range(0, 10), Tooltip("Movement before the jump will influence the impulse direction.")]
    public float jumpMomentum = 0.5f;
    [Range(0, 10)]
    public float sprintJumpMomentum = 2;
    [Range(0, 1)]
    public float aerialControl = 0.75f;
    [Range(0, 100)]
    public float aerialRotationSpeed = 10;

    // DASH
    [Header("Dash"), SerializeField]
    private bool _canDash = true;
    public float dashPower = 30;
    [Tooltip("In seconds (Will try to make it distance soon)"), Range(0, 3)]
    public float dashLength = 0.5f;
    [Range(0, 10)]
    public float dashHeight = 3;
    private TimerPlus dashTimer;
    public bool stopDashOnCollision = true;
    [Range(0, 10)]
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
    public bool canDashJump = true;
    public float dashJumpPower = 20;
    [Range(0, 3)]
    public float dashJumpLength = 0.5f;

    // HEAVY
    [Header("Heavy"), SerializeField]
    private bool _canHeavy = true;
    public float heavyMoveSpeed = 2;
    public bool canUnheavyMidair = false;
    [Range(0, 100)]
    public float heavyJumpPower = 20;
    [Range(0, 1000)]
    public float heavyDownwardForce = 100;
    [Range(0, 90)]
    public float heavySlopeAngle = 0.2f;
    [Range(0, 100)]
    public float heavyRotationSpeed = 10;
    public PhysicMaterial heavyMaterial;
    [Range(0, 10)]
    public float heavyMass = 5;
    public bool canBounceWhileHeavy = false;
    [Range(0, 10)]
    public float exitHeavyDragMulti = 5;
    public float exitHeavyDragDuration = 2;
    private TimerPlus exitHeavyDragTimer;
    [Range(0, 10)]
    public float minHeavyDuration = 1;
    [Range(0, 10)]
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
    [Header("Bounce"), SerializeField]
    private bool _canBounce = true;
    public bool canGroundBounce = true;
    public float groundBounceThreshold = 23;
    public float velocityBounceThreshold = 25;
    [Range(0, 50)]
    public float bouncePower = 10;
    public float minBouncePower = 5;
    public float maxBouncePower = 50;
    [Range(0, 5)]
    public float jumpBounceMulti = 1.5f;
    [Range(0, 5)]
    public float groundBounceMulti = 0.5f;
    public float partnerBouncePower = 5;
    public bool canBounce
    {
        get { return _canBounce && !bounceTimer.IsPlaying; }
    }
    private TimerPlus bounceTimer;

    // PRIVATES
    private float normalDrag;                           // Normal rigidbody drag.
    private Color normalColour;                         // Normal material full colour.
    private Color heavyColor;                           // Heavy material full colour.
    private float gravity;                              // Current gravity on this character.
    private RaycastHit onObject;                        // Which object is currently under this character.
    private const float airborneRayOffset = -1.1f;      // How much additional height offset will the ray checks account for.
    private float jumpTime;                             // How long the jump button has been held in for.
    private bool jumpFlag;                              // Is the character ready to jump again?
    private bool yankFlag = true;                       // Is the character able to be yanked
    private float lastRecoredY;                         // The last recorded position Y Value.

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
            if (isWeakened) value *= GameManager.PlayerManager.weakenedMoveSpeedMulti;
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
            return !isGrounded && transform.position.y < lastRecoredY;
        }
    }
    public bool isWeakened
    {
        get { return GameManager.PlayerManager.isWeakened; }
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
    public bool isDashJumping { get; set; }

    #endregion

    #region MESSAGES

    void Awake()
    {
        // There can only be 2 characters
        if (isPlayerOne && this != GameManager.PlayerManager.character1 || !isPlayerOne && this != GameManager.PlayerManager.character2) DestroyImmediate(gameObject);

        // Find component references
        if (!rigidbodyComp)
            rigidbodyComp = GetComponent<Rigidbody>();
        if (!colliderComp)
            colliderComp = GetComponent<CapsuleCollider>();
        if (!animatorComp)
            animatorComp = GetComponentInChildren<Animator>();
        if (!audioDataComp)
            audioDataComp = GetComponent<CharacterAudio>();
    }

    void Start()
    {
        // Setup up character input depending on whether this is character 1 or 2
        GameManager.InputManager.SubscribeCharacterEvents(this);

        // Setup action timers
        dashTimer = TimerPlus.Create(dashLength, TimerPlus.Presets.Standard, () => 
            {
                // Reset dash jump state
                isDashJumping = false;

                // Make partner yank-able after dash again
                GetPartner().FlipYankFlag(true);
            });
        dashCooldownTimer = TimerPlus.Create(dashCooldown, TimerPlus.Presets.Standard);
        heavyCooldownTimer = TimerPlus.Create(heavyCooldown, TimerPlus.Presets.Standard);
        unheavyDurationTimer = TimerPlus.Create(minHeavyDuration, TimerPlus.Presets.Standard);
        exitHeavyDragTimer = TimerPlus.Create(exitHeavyDragDuration, TimerPlus.Presets.Standard, () => rigidbodyComp.drag = normalDrag);
        bounceTimer = TimerPlus.Create(0.2f, TimerPlus.Presets.Standard);

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
    }

    void OnDestroy()
    {
        // Dispose persistent timers
        dashTimer.Dispose();
        dashCooldownTimer.Dispose();
        heavyCooldownTimer.Dispose();
        unheavyDurationTimer.Dispose();
        exitHeavyDragTimer.Dispose();
        bounceTimer.Dispose();

        // Dispose of FMOD instances
        audioDataComp.Dispose();

        // Unsubscribe this character from events
        GameManager.InputManager.UnsubscribeCharacterEvents(this);
    }

    void OnCollisionEnter(Collision col)
    {
        // Collision audio
        // Try to play land sound if not, try the collide sound
        if (!audioDataComp.ConditionalAudio(() => audioDataComp.PlayLandAudio(col.relativeVelocity.y), col.relativeVelocity.y > 2))
            audioDataComp.ConditionalAudio(() => audioDataComp.PlayCollideAudio(),
                col.gameObject == GetPartner().gameObject && isPlayerOne
                || col.relativeVelocity.magnitude > baseMoveSpeed
                || isGrounded && currentMoveSpeed > baseMoveSpeed
                || isDashing);

        // Bounce off ground
        if (!col.collider.GetComponent<Bouncy>()) gameObject.GetInterface<IBounce>().Bounce(col.relativeVelocity, col.collider.gameObject);

        // Stop dash on collision 
        if (stopDashOnCollision && col.contacts[0].normal.z != 0)
        {
            isDashing = false;
            isDashJumping = false;
        }

        // Reconnect on touch
        if (GameManager.PlayerManager.reconnectOnTouch && col.collider.gameObject == GetPartner().gameObject)
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
        colliderComp.material = isHeavy ? heavyMaterial : normalMaterial;
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
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetVelocity.IgnoreY3(0)), delta * currentRotationSpeed);

        // Apply movement
        AddConstrainedMovement(targetVelocity * delta);

        // Apply gravity
        rigidbodyComp.AddForce(new Vector3(0, gravity, 0), ForceMode.Force);

        // Apply constraining force
        if(GameManager.PlayerManager.constrainMovement) ApplyConstrainForce();

        // Send animator info
        animatorComp.SetBool("IsAirborne", !isGrounded);
        animatorComp.SetFloat("Speed", targetVelocity.IgnoreY2().normalized.magnitude * (currentMoveSpeed / baseMoveSpeed));

        // Updates last recorded Y value
        lastRecoredY = transform.position.y;
    }

    public void Movement(float forward, float right)
    {
        Vector2 direction = Vector2.ClampMagnitude(new Vector2(right, forward), 1);

        if (!isDashing)
        {
            // Apply standard movement
            targetVelocity.x = direction.x * currentMoveSpeed;
            targetVelocity.z = direction.y * currentMoveSpeed;
            targetVelocity.y = 0;
        }
        else
        {
            // Apply standard dash forward movement
            targetVelocity.x = transform.forward.x * dashPower;
            targetVelocity.z = transform.forward.z * dashPower;
            targetVelocity.y = 0;

            // Apply dash jump vertically
            if (isDashJumping)
                targetVelocity.y = dashJumpPower;
        }
    }

    public void Jump(bool p1, bool isPressed)
    {
        if (p1 != isPlayerOne) return;

        if (!canJump) return;

        // Increment jump time
        jumpTime += GameManager.InputManager.jumpDelta;

        // Flag gate
        if (isBouncing || isHeavy || !isPressed || jumpTime > jumpCurve.Duration() || jumpTime > 0.1f && isGrounded) ResetJumpFlag();

        if (!jumpFlag) return;

        // Standard jump
        AddConstrainedForce(Vector3.up * jumpCurve.Evaluate(jumpTime) * (jumpForce / jumpCurve.Duration()) * GameManager.InputManager.jumpDelta, ForceMode.Force);
    }

    public void JumpToggle(bool p1, bool isPressed)
    {
        if (p1 != isPlayerOne) return;

        // Enter the dash jump state
        if (isDashing && isPressed && isGrounded && !isDashJumping && canDashJump)
        {
            isDashJumping = true;
            dashTimer.ModifyValue(dashJumpLength, true);
            return;
        }

        // Flip the jump flag and apply the initial impulse force
        if (isPressed && isGrounded)
        {
            jumpFlag = true;

            // Play sound
            audioDataComp.PlayJumpAudio();

            if (!isHeavy) // Standard jump impulse
            {
                rigidbodyComp.drag = normalDrag;

                if (!doJumpMomentum || targetVelocity.IgnoreY2() == Vector2.zero)
                    AddConstrainedForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
                else
                {
                    Vector3 jumpVector = targetVelocity.normalized * currentJumpMomentum;
                    jumpVector.y = 10;
                    AddConstrainedForce(jumpVector.normalized * jumpImpulse, ForceMode.Impulse);
                }
            }
            else // Heavy jump impulse
            {
                AddConstrainedForce(Vector3.up * heavyJumpPower * jumpImpulse, ForceMode.Impulse);
                ResetJumpFlag();
            }
        }

        // Cut jump short if input was let go while rising
        if (!isPressed && !isFalling && !isHeavy)
        {
            rigidbodyComp.velocity = new Vector3(rigidbodyComp.velocity.x, 0, rigidbodyComp.velocity.z);
        }
    }

    public void Dash(bool p1, bool isPressed)
    {
        if (p1 != isPlayerOne) return;

        if (isPressed && canDash)
        {
            isDashing = true;

            if (isGrounded) targetVelocity.y += dashHeight;

            audioDataComp.PlayDashAudio();
        }
    }

    public void Heavy(bool p1, bool isPressed)
    {
        if (p1 != isPlayerOne) return;

        if (isPressed == isHeavy) return;

        if (isPressed && canHeavy)
        {
            isHeavy = true;

            rigidbodyComp.drag = normalDrag;
            unheavyDurationTimer.Restart();

            // Colour changing for debug purposes
            GetComponentInChildren<Renderer>().material.color = heavyColor;
        }
        else if (!isPressed && canUnheavy)
        {
            isHeavy = false;

            heavyCooldownTimer.Restart();

            if (isGrounded) // Reduce sliding down slopes while not heavy
            {
                rigidbodyComp.drag *= exitHeavyDragMulti;
                exitHeavyDragTimer.Restart();
            }

            // Colour changing for debug purposes
            GetComponentInChildren<Renderer>().material.color = normalColour;
        }
    }

    public void Sprint(bool p1, bool isPressed)
    {
        if (p1 != isPlayerOne) return;

        isSprinting = isPressed && targetVelocity.magnitude != 0;
    }

    public void Pause(bool p1, bool isPressed)
    {
        if (p1 != isPlayerOne) return;

        // Request Pause mode
        if (isPressed) GameManager.ModeManager.RequestGameModeChange(ModeManager.GameMode.PauseMenu, false, 0.1f);
    }

    public void Unpause(bool player1, bool isPressed)
    {
        if (player1 != isPlayerOne) return;

        // Request In-Game mode
        if (isPressed) GameManager.ModeManager.RequestGameModeChange(ModeManager.GameMode.InGame, false, 0.1f);
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
            bounceTimer.Restart();
            AddConstrainedForce(direction.normalized * magnitude, ForceMode.Impulse);
        }
    }

    // Animation event call
    public void OnFootStep(int footIndex) // 1 left, 2 right
    {
        audioDataComp.ConditionalAudio(() => audioDataComp.PlayWalkAudio(onObject.transform ? onObject.transform.gameObject.GetSurfaceType() : Surface.SurfaceTypes.None), isWalking && isGrounded);
    }

    //[System.Obsolete] Soon to be obsolete once all constraining is moved to the two methods below
    public void ApplyConstrainForce()
    {
        float length = GameManager.TetherManager.tetherLength;

        if (GameManager.TetherManager.disconnected || length < GameManager.PlayerManager.freeRadius) return;

        Vector3 direction = GameManager.TetherManager.GetStartAndEndMoveDirection(isPlayerOne);
        float constrainMulti = length.Normalize(GameManager.PlayerManager.freeRadius, GameManager.PlayerManager.maxRadius, 0, 1000);
        rigidbodyComp.AddForce(direction * constrainMulti * GameManager.PlayerManager.constrainingPower * Time.fixedDeltaTime, ForceMode.Force);
    }

    // Rigidbody.AddForce() with constraining
    public void AddConstrainedForce(Vector3 movement, ForceMode forceMode)
    {
        float length = GameManager.TetherManager.tetherLength;

        if (!GameManager.PlayerManager.constrainMovement || length < GameManager.PlayerManager.freeRadius || GameManager.TetherManager.disconnected)
        {
            rigidbodyComp.AddForce(movement, forceMode);
            return;
        }

        // Dot value = 1 when facing towards the tether | 0 = perpendicular to the tether | -1 = facing away from the tether
        float dotValue = Vector3.Dot(movement.normalized, GameManager.TetherManager.GetStartAndEndMoveDirection(isPlayerOne).normalized);
        rigidbodyComp.AddForce(movement * dotValue * GameManager.PlayerManager.constrainingPower, forceMode);
    }

    // RigidBody.MovePosition with constraining
    public void AddConstrainedMovement(Vector3 movement)
    {
        float length = GameManager.TetherManager.tetherLength;

        if (!GameManager.PlayerManager.constrainMovement || length < GameManager.PlayerManager.freeRadius || GameManager.TetherManager.disconnected)
        {
            rigidbodyComp.MovePosition(transform.position + movement);
        }
        else if (isDashing)
        {
            if (!GetPartner().isHeavy || length > GameManager.PlayerManager.maxRadius)
                    rigidbodyComp.MovePosition(transform.position + movement);
            else
                isDashing = false;

            GetPartner().Yank();
        }
        else if (isSprinting)
        {
            Vector3 direction = GameManager.TetherManager.GetStartAndEndMoveDirection(isPlayerOne).normalized;
            if (isGrounded)
                direction.y = direction.magnitude / 10;
            AddConstrainedForce(direction * GameManager.PlayerManager.yankingDashForce, ForceMode.Impulse);
        }
        else
        {
            // Dot value = 1 when facing towards the tether | 0 = perpendicular to the tether | -1 = facing away from the tether
            float dotValue = Vector3.Dot(movement.normalized, GameManager.TetherManager.GetStartAndEndMoveDirection(isPlayerOne).normalized);
            Vector3 additiveVec = movement * dotValue.Normalize(-1, 1, -0.25f, 1);
            rigidbodyComp.MovePosition(transform.position + Vector3.ClampMagnitude(additiveVec, length.Normalize(GameManager.PlayerManager.freeRadius, GameManager.PlayerManager.maxRadius, additiveVec.magnitude, 0)));
        }
    }

    public void Yank()
    {
        if (yankFlag && !isHeavy)
        {
            yankFlag = false;
            rigidbodyComp.drag = 0.25f;
            TimerPlus.Create(1, () => rigidbodyComp.drag = normalDrag);
            //Vector3 direction = GameManager.TetherManager.GetStartAndEndMoveDirection(isPlayerOne).normalized;
            Vector3 direction = (GetPartnerPosition() - transform.position).normalized;
            direction.y = direction.magnitude / 2;
            AddConstrainedForce(direction * GameManager.PlayerManager.yankingDashForce, ForceMode.Impulse);
        }
    }

    #endregion

    #region INTERFACE MEMBERS

    public void Bounce(Vector3 relativeVelocity, GameObject bounceObject)
    {
        Bouncy bouncyObj = bounceObject.GetComponent<Bouncy>();

        // Bounce direction is currently always up due to some issues with side-collisions not acting properly.
        Vector3 bounceDirection = Vector3.up;
        float finalBouncePower = bouncePower;

        // Bouncing off player
        if (bouncyObj && bouncyObj.enabled && bouncyObj.GetComponent<Character>())
        {
            float yDifference = transform.position.y - GetPartnerPosition().y;
            if (yDifference > colliderComp.height / 2f) // Above
            {
                // Increase bounce if holding jump
                finalBouncePower *= GameManager.InputManager.IsRequestingJump(isPlayerOne) ? jumpBounceMulti : 1;

                // Assist the player in bounce on partner
                bounceDirection = (GetPartnerPosition() + new Vector3(0, bouncePower, 0)) - transform.position;
            }
            else if (yDifference > -(colliderComp.height / 2f)) // Equal
            {
                // Preset small bounce
                finalBouncePower = partnerBouncePower;
                bounceDirection = transform.position - GetPartnerPosition();
                bounceDirection.y = bounceDirection.magnitude;
                PerformBounce(bounceDirection, finalBouncePower); // Bypass minimum
                return;
            }
            else // Below
            {
                isBouncing = false;
                return;
            }
        }
        // Bouncing on a bouncy object
        else if (bouncyObj && bouncyObj.enabled)
        {
            // Increase bounce if holding jump
            finalBouncePower *= GameManager.InputManager.IsRequestingJump(isPlayerOne) ? jumpBounceMulti : 1;

            // Apply relative velocity bounce multi
            if (!isBouncing && relativeVelocity.magnitude > velocityBounceThreshold)
                finalBouncePower *= relativeVelocity.magnitude.Normalize(velocityBounceThreshold, maxBouncePower, 1, 2);
        }
        // Bouncing on the ground
        else if (canGroundBounce && relativeVelocity.magnitude > groundBounceThreshold)
        {
            // Decrease bounce power because ground
            finalBouncePower *= groundBounceMulti;

            // Apply relative velocity bounce multi
            if (!isBouncing && relativeVelocity.magnitude > velocityBounceThreshold)
                finalBouncePower *= relativeVelocity.magnitude.Normalize(velocityBounceThreshold, maxBouncePower, 1, 2);
        }
        else
        {
            isBouncing = false;
            return;
        }

        //if (isPlayerOne) Debug.Log(finalBouncePower);
        PerformBounce(bounceDirection, Mathf.Clamp(finalBouncePower, minBouncePower, maxBouncePower));
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
        Ray ray = new Ray(origin, direction);

        bool newResult = Physics.SphereCast(ray, colliderComp.radius, out groundObject, (colliderComp.height / 2) + offset, groundedLayers);

        return newResult;
    }

    // Resets the jump flag so the character can respond to jump input again
    private void ResetJumpFlag()
    {
        jumpFlag = false;
        jumpTime = 0;
    }

    // For exposing the yankFlag property
    public void FlipYankFlag(bool value)
    {
        yankFlag = value;
    }

    // Returns the other character
    public Character GetPartner()
    {
        if (this == GameManager.PlayerManager.character1) return GameManager.PlayerManager.character2;
        else return GameManager.PlayerManager.character1;
    }

    // Returns the other character position
    public Vector3 GetPartnerPosition()
    {
        return GetPartner().transform.position;
    }

    #endregion
}
