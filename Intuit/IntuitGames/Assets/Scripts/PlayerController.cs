using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;[RequireComponent(typeof(CharacterController))]public class PlayerController : MonoBehaviour{
    // Component references
    [SerializeField, HideInInspector]
    private CharacterController characterController;

    [SerializeField]
    private bool _isPlayerOne = true;    public bool isPlayerOne
    {
        get
        {
            return _isPlayerOne;
        }
        set
        {
            if(value != isPlayerOne)
            {
                _isPlayerOne = value;
                SetupInput();
            }
        }
    }

    public Vector3 movement;
    public Vector3 velocity;
    public float moveSpeed = 6;
    public float jumpSpeed = 25;
    public float maxVelocity = 50;

    // How long has this character been airborne for?
    private float airTime = 0;

    private bool isAirborne
    {
        get
        {
            if (characterController.isGrounded)
                return false;
            else
                return !Physics.Raycast(transform.position, -transform.up, (characterController.height / 2) + 0.1f);
        }
    }
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        SetupInput();
    }

    void LateUpdate()
    {
        // Apply gravity if airborne
        if (isAirborne)
        {
            airTime += Time.deltaTime;
            movement.y += Physics.gravity.y * airTime;
        }
        else
            airTime = 0;

        // Clamp movement
        movement = new Vector3(Mathf.Clamp(movement.x, -maxVelocity, maxVelocity),
            Mathf.Clamp(movement.y, -maxVelocity, maxVelocity),
            Mathf.Clamp(movement.z, -maxVelocity, maxVelocity));

        // Apply movement
        CollisionFlags colFlags = characterController.Move(movement * Time.deltaTime);

        // Reset movement vector
        //movement = new Vector3(0, movement.y, 0);
        movement = Vector3.SmoothDamp(movement, Vector3.zero, ref velocity, 0.1f);
    }

    #region Input

    private void SetupInput()
    {
        GameManager.inputManager.pause += this.Pause;
        GameManager.inputManager.unpause += this.Pause;

        if (isPlayerOne)
        {
            // Subscribe to player 1 events
            GameManager.inputManager.movementP1 += this.Movement;
            GameManager.inputManager.jumpP1 += this.Jump;
            GameManager.inputManager.dashP1 += this.Dash;
            GameManager.inputManager.heavyP1 += this.Heavy;

            // Unsubscribe from player 2 events
            GameManager.inputManager.movementP2 -= this.Movement;
            GameManager.inputManager.jumpP2 -= this.Jump;
            GameManager.inputManager.dashP2 -= this.Dash;
            GameManager.inputManager.heavyP2 -= this.Heavy;
        }
        else
        {
            // Subscribe to player 2 events
            GameManager.inputManager.movementP2 += this.Movement;
            GameManager.inputManager.jumpP2 += this.Jump;
            GameManager.inputManager.dashP2 += this.Dash;
            GameManager.inputManager.heavyP2 += this.Heavy;

            // Unsubscribe from player 1 events
            GameManager.inputManager.movementP1 -= this.Movement;
            GameManager.inputManager.jumpP1 -= this.Jump;
            GameManager.inputManager.dashP1 -= this.Dash;
            GameManager.inputManager.heavyP1 -= this.Heavy;
        }
    }

    private void Movement(float forward, float right)
    {
        //movement += new Vector3(moveSpeed * right, 0, moveSpeed * forward);
        if(Mathf.Abs(movement.x) < moveSpeed)
            movement.x += moveSpeed * right;
        if (Mathf.Abs(movement.z) < moveSpeed)
            movement.z += moveSpeed * forward;
    }

    private void Jump()
    {
        if (!isAirborne)
            movement.y += jumpSpeed;
    }

    private void Dash()
    {
        Debug.Log((isPlayerOne ? "Player 1 " : "Player 2 ") + "dash.");
    }

    private void Heavy()
    {
        Debug.Log((isPlayerOne ? "Player 1 " : "Player 2 ") + "heavy.");
    }

    private void Pause()
    {
        Debug.Log((isPlayerOne ? "Player 1 " : "Player 2 ") + "pause.");
    }

    #endregion
}