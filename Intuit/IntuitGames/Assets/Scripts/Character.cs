﻿using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
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

    [ReadOnly, Header("Heavy")]
    public bool isHeavy = false;
    public float heavyMoveSpeed = 2;
    public bool CanUnheavyMidair = false;
    [Range(0, 10)]
    public float heavyGravity = 6;
    [Range(0, 25)]
    public float heavyJumpPower = 20;

    // PRIVATES
    private float airTime = 0;

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
                return !Physics.Raycast(transform.position, -transform.up, (characterController.height / 2) + 0.1f);
        }
    }
    public bool isFalling
    {
        get
        {
            return isAirborne && targetVelocity.y < 0;
        }
    }
    void Start()
    {
        // Find component references
        characterController = GetComponent<CharacterController>();

        // Setup up character input depending on whether this is character 1 or 2
        GameManager.inputManager.SetupCharacterInput(this);
    }

    // Is called BEFORE input is checked every frame
    public void PreInputUpdate()
    {
        if (!this.enabled) return;

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
        targetVelocity.x = direction.x * moveSpeed;
        targetVelocity.z = direction.y * moveSpeed;
    }

    public void Jump(int jumpType) // 1 = low, 2 = med, 3 = high
    {
        if (!isAirborne)
        {
            if (!isHeavy)   // Standard jump
                targetVelocity.y += jumpType == 3 ? hightJumpPower : jumpType == 2 ? mediumJumpPower : lowJumpPower;
            else            // Heavy jump
                targetVelocity.y += heavyJumpPower;
        }
    }

    public void Dash()
    {

    }

    public void Heavy(bool isHeldDown)
    {
        if (!CanUnheavyMidair && isHeavy && !isHeldDown && isAirborne)
            return;
        else
            isHeavy = isHeldDown;
    }

    public void Pause()
    {

    }
}