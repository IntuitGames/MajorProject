using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Handles all input related elements.
/// </summary>
public class InputManager : Manager
{
    // Input Axes Strings
    private const string player1Str = "P1";
    private const string player2Str = "P2";
    private const string forwardStr = "Vertical_";
    private const string rightStr = "Horizontal_";
    private const string jumpStr = "Jump_";
    private const string dashStr = "Dash_";
    private const string heavyStr = "Heavy_";
    private const string pauseStr = "Submit_";
    private const string unpauseStr = "Submit_";

    // Events
    public event Action preUpdate = delegate { };
    public event Action postUpdate = delegate { };
    public event Action<float> forwardP1 = delegate { };
    public event Action<float> forwardP2 = delegate { };
    public event Action<float> rightP1 = delegate { };
    public event Action<float> rightP2 = delegate { };
    public event Action<float, float> movementP1 = delegate { };
    public event Action<float, float> movementP2 = delegate { };
    public event Action<float> jumpP1 = delegate { };
    public event Action<float> jumpP2 = delegate { };
    public event Action dashP1 = delegate { };
    public event Action dashP2 = delegate { };
    public event Action<bool> heavyP1 = delegate { };
    public event Action<bool> heavyP2 = delegate { };
    public event Action pause = delegate { };
    public event Action unpause = delegate { };

    // Timings
    private bool player1JumpDown;                   // Player 1 is pressing down the jump button
    private bool player2JumpDown;                   // Player 2 is pressing down the jump button
    private float player1JumpHold;                  // How long has player 1 held down the jump button
    private float player2JumpHold;                  // How long has player 1 held down the jump button

    void Update()
    {
        preUpdate();

        // Only check inputs for certain game modes
        switch (GameManager.modeManager.currentGameMode)
        {
            case ModeManager.GameMode.InGame:
                HandleInGameEvents();
                break;

            case ModeManager.GameMode.PauseMenu:
                HandlePauseMenuEvents();
                break;

            default:
                break;
        }

        postUpdate();
    }

    // Is called every frame if game is in a in-game state
    private void HandleInGameEvents()
    {
        // Raise axis input events
        forwardP1(Input.GetAxis(forwardStr + player1Str));
        forwardP2(Input.GetAxis(forwardStr + player2Str));
        rightP1(Input.GetAxis(rightStr + player1Str));
        rightP2(Input.GetAxis(rightStr + player2Str));
        movementP1(Input.GetAxis(forwardStr + player1Str), Input.GetAxis(rightStr + player1Str));
        movementP2(Input.GetAxis(forwardStr + player2Str), Input.GetAxis(rightStr + player2Str));

        // Raise action button event
        HandleJumpEvents();

        if (Input.GetButtonDown(dashStr + player1Str))
            dashP1();
        if (Input.GetButtonDown(dashStr + player2Str))
            dashP2();

        heavyP1(Input.GetButton(heavyStr + player1Str));
        heavyP2(Input.GetButton(heavyStr + player2Str));

        // Pause and unpause check
        if (Input.GetButtonDown(pauseStr + player1Str))
            pause();
        if (Input.GetButtonDown(pauseStr + player2Str))
            pause();
    }

    // Specifically handles the more complex jump event system
    private void HandleJumpEvents()
    {
        // Player 1
        if (Input.GetButtonDown(jumpStr + player1Str) && !Character.character1.isAirborne)
            player1JumpDown = true;

        if (player1JumpDown && Input.GetButton(jumpStr + player1Str))
        {
            player1JumpHold += Time.deltaTime;
            jumpP1(player1JumpHold);
            
            // If heavy make sure its only a one-time boost
            if(Character.character1.isHeavy)
            {
                player1JumpDown = false;
                player1JumpHold = 0;
            }
        }
        else
        {
            player1JumpDown = false;
            player1JumpHold = 0;
        }

        // Player 2
        if (Input.GetButtonDown(jumpStr + player2Str) && !Character.character2.isAirborne)
            player2JumpDown = true;

        if (player2JumpDown && Input.GetButton(jumpStr + player2Str))
        {
            player2JumpHold += Time.deltaTime;
            jumpP2(player2JumpHold);

            // If heavy make sure its only a one-time boost
            if (Character.character2.isHeavy)
            {
                player2JumpDown = false;
                player2JumpHold = 0;
            }
        }
        else
        {
            player2JumpDown = false;
            player2JumpHold = 0;
        }
    }

    // Is called every frame if the game is in a paused state
    private void HandlePauseMenuEvents()
    {
        if (Input.GetButtonDown(unpauseStr + player1Str))
            unpause();
        if (Input.GetButtonDown(unpauseStr + player2Str))
            unpause();
    }

    // Subscribes to all character specific events
    public void SetupCharacterInput(Character characterObj)
    {
        // Ensure these methods are only subscribed to once
        preUpdate -= characterObj.PreInputUpdate;
        postUpdate -= characterObj.PostInputUpdate;
        pause -= characterObj.Pause;
        unpause -= characterObj.Pause;
        preUpdate += characterObj.PreInputUpdate;
        postUpdate += characterObj.PostInputUpdate;
        pause += characterObj.Pause;
        unpause += characterObj.Pause;

        if (characterObj.isPlayerOne)
        {
            // Subscribe to player 1 events
            movementP1 += characterObj.Movement;
            jumpP1 += characterObj.Jump;
            dashP1 += characterObj.Dash;
            heavyP1 += characterObj.Heavy;

            // Unsubscribe from player 2 events
            movementP2 -= characterObj.Movement;
            jumpP2 -= characterObj.Jump;
            dashP2 -= characterObj.Dash;
            heavyP2 -= characterObj.Heavy;
        }
        else
        {
            // Subscribe to player 2 events
            movementP2 += characterObj.Movement;
            jumpP2 += characterObj.Jump;
            dashP2 += characterObj.Dash;
            heavyP2 += characterObj.Heavy;

            // Unsubscribe from player 1 events
            movementP1 -= characterObj.Movement;
            jumpP1 -= characterObj.Jump;
            dashP1 -= characterObj.Dash;
            heavyP1 -= characterObj.Heavy;
        }
    }

    // Is a player currently requesting a jump?
    public bool IsRequestingJump(bool isPlayerOne)
    {
        if (isPlayerOne)
            return player1JumpDown;
        else
            return player2JumpDown;
    }
}
