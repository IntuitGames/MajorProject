using UnityEngine;
using System.Collections;
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
    public event Action<int> jumpP1 = delegate { };
    public event Action<int> jumpP2 = delegate { };
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
    public float mediumJumpThreshold = 0.2f;        // How long the jump button must be held down to do a medium jump
    public float highJumpThreshold = 0.4f;          // How long the jump button must be held down to do a high jump

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
        // Check if the player has pressed the jump button down
        if (Input.GetButtonDown(jumpStr + player1Str))
            player1JumpDown = true;
        if (Input.GetButtonDown(jumpStr + player2Str))
            player2JumpDown = true;

        // Checks if they let go early
        if (Input.GetButtonUp(jumpStr + player1Str))
        {
            if (player1JumpHold > highJumpThreshold)
                jumpP1(3);
            else if (player1JumpHold > mediumJumpThreshold)
                jumpP1(2);
            else if (player1JumpHold > 0)
                jumpP1(1);

            player1JumpDown = false;
        }

        if (Input.GetButtonUp(jumpStr + player2Str))
        {
            if (player2JumpHold > highJumpThreshold)
                jumpP2(3);
            else if (player2JumpHold > mediumJumpThreshold)
                jumpP2(2);
            else if (player2JumpHold > 0)
                jumpP2(1);

            player2JumpDown = false;
        }

        // Determines time and checks if they have held long enough to perform the high jump
        if (player1JumpDown)
        {
            if (player1JumpHold <= highJumpThreshold)
                player1JumpHold += Time.deltaTime;
            else
            {
                jumpP1(3);
                player1JumpDown = false;
                player1JumpHold = 0;
            }
        }
        else
            player1JumpHold = 0;

        if (player2JumpDown)
        {
            if (player2JumpHold <= highJumpThreshold)
                player2JumpHold += Time.deltaTime;
            else
            {
                jumpP2(3);
                player2JumpDown = false;
                player2JumpHold = 0;
            }
        }
        else
            player2JumpHold = 0;
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
