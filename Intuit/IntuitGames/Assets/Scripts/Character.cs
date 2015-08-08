using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;[RequireComponent(typeof(CharacterController))]public class Character : MonoBehaviour{
    // COMPONENTS
    [SerializeField, HideInInspector]
    private CharacterController characterController;

    // STATS
    [SerializeField, Popup(new string[2] { "Player 1", "Player 2"}, OverrideName = "Player")]
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
                if (Application.isPlaying) GameManager.inputManager.SetupCharacterInput(this);
            }
        }
    }

    [ReadOnly, Header("Basic")]
    public Vector3 movement;
    [Range(0, 25)]
    public float gravity = 4;
    public float moveSpeed = 6;
    [Range(0, 25)]
    public float lowJumpPower = 10;
    [Range(0, 25)]
    public float mediumJumpPower = 17.5f;
    [Range(0, 25)]
    public float hightJumpPower = 25;
    public float maxVelocity = 50;

    // PRIVATES
    private float airTime = 0;

    // STATES
    public bool isAirborne
    {
        get
        {
            if (characterController.isGrounded)
                return false;
            else
            {
                return !Physics.Raycast(transform.position, -transform.up, (characterController.height / 2) + 0.1f);
            }
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
            movement.y += -gravity * airTime;
        }
        else
        {
            movement.y = Mathf.Max(0, movement.y);
            airTime = 0;
        }
    }

    // Is called AFTER input is determined every frame
    public void PostInputUpdate()
    {
        if (!this.enabled) return;

        // Clamp movement velocity
        movement = Vector3.ClampMagnitude(movement, maxVelocity);

        // Apply movement
        transform.LookAt((transform.position + movement).IgnoreY3(transform.position.y));
        characterController.Move(movement * Time.deltaTime);
    }

    public void Movement(float forward, float right)
    {
        Vector2 direction = new Vector2(right, forward).normalized;
        movement.x = direction.x * moveSpeed;
        movement.z = direction.y * moveSpeed;
    }

    public void Jump(int jumpType) // 1 = low, 2 = med, 3 = high
    {
        if (!isAirborne)
            movement.y += jumpType == 3 ? hightJumpPower : jumpType == 2 ? mediumJumpPower : lowJumpPower;
    }

    public void Dash()
    {
        Debug.Log((isPlayerOne ? "Player 1 " : "Player 2 ") + "dash.");
    }

    public void Heavy()
    {
        Debug.Log((isPlayerOne ? "Player 1 " : "Player 2 ") + "heavy.");
    }

    public void Pause()
    {
        Debug.Log((isPlayerOne ? "Player 1 " : "Player 2 ") + "pause.");
    }
}