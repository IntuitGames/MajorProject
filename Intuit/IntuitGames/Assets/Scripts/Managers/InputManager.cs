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
    public event Action PreUpdate = delegate { };
    public event Action PostUpdate = delegate { };
    public event Action<float> ForwardP1 = delegate { };
    public event Action<float> ForwardP2 = delegate { };
    public event Action<float> RightP1 = delegate { };
    public event Action<float> RightP2 = delegate { };
    public event Action<float, float> MovementP1 = delegate { };
    public event Action<float, float> MovementP2 = delegate { };
    public event Action<float> JumpP1 = delegate { };
    public event Action<float> JumpP2 = delegate { };
    public event Action DashP1 = delegate { };
    public event Action DashP2 = delegate { };
    public event Action<bool> HeavyP1 = delegate { };
    public event Action<bool> HeavyP2 = delegate { };
    public event Action Pause = delegate { };
    public event Action Unpause = delegate { };

    // Timings
    private bool player1JumpDown;                   // Player 1 is pressing down the jump button
    private bool player2JumpDown;                   // Player 2 is pressing down the jump button
    private float player1JumpHold;                  // How long has player 1 held down the jump button
    private float player2JumpHold;                  // How long has player 1 held down the jump button

    void Update()
    {
        PreUpdate();

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

        PostUpdate();
    }

    // Is called every frame if game is in a in-game state
    private void HandleInGameEvents()
    {
        // Raise axis input events
        ForwardP1(Input.GetAxis(forwardStr + player1Str));
        ForwardP2(Input.GetAxis(forwardStr + player2Str));
        RightP1(Input.GetAxis(rightStr + player1Str));
        RightP2(Input.GetAxis(rightStr + player2Str));
        MovementP1(Input.GetAxis(forwardStr + player1Str), Input.GetAxis(rightStr + player1Str));
        MovementP2(Input.GetAxis(forwardStr + player2Str), Input.GetAxis(rightStr + player2Str));

        // Raise action button event
        HandleJumpEvents();

        if (Input.GetButtonDown(dashStr + player1Str))
            DashP1();
        if (Input.GetButtonDown(dashStr + player2Str))
            DashP2();

        HeavyP1(Input.GetButton(heavyStr + player1Str));
        HeavyP2(Input.GetButton(heavyStr + player2Str));

        // Pause and unpause check
        if (Input.GetButtonDown(pauseStr + player1Str))
            Pause();
        if (Input.GetButtonDown(pauseStr + player2Str))
            Pause();
    }

    // Specifically handles the more complex jump event system
    private void HandleJumpEvents()
    {
        // Player 1
        if (!Character.character1.isBouncing)
        {
            if (Input.GetButtonDown(jumpStr + player1Str) && !Character.character1.isAirborne)
                player1JumpDown = true;

            if (player1JumpDown && Input.GetButton(jumpStr + player1Str))
            {
                player1JumpHold += Time.deltaTime;
                JumpP1(player1JumpHold);

                // If heavy make sure its only a one-time boost
                if (Character.character1.isHeavy)
                    ResetJumpDown(true);
            }
            else
                ResetJumpDown(true);
        }

        // Player 2
        if (!Character.character2.isBouncing)
        {
            if (Input.GetButtonDown(jumpStr + player2Str) && !Character.character2.isAirborne)
                player2JumpDown = true;

            if (player2JumpDown && Input.GetButton(jumpStr + player2Str))
            {
                player2JumpHold += Time.deltaTime;
                JumpP2(player2JumpHold);

                // If heavy make sure its only a one-time boost
                if (Character.character2.isHeavy)
                    ResetJumpDown(false);
            }
            else
                ResetJumpDown(false);
        }
    }

    // Is called every frame if the game is in a paused state
    private void HandlePauseMenuEvents()
    {
        if (Input.GetButtonDown(unpauseStr + player1Str))
            Unpause();
        if (Input.GetButtonDown(unpauseStr + player2Str))
            Unpause();
    }

    // Subscribes to all character specific events
    public void SetupCharacterInput(Character characterObj)
    {
        characterObj.Landed += ResetJumpDown;

        // Ensure these methods are only subscribed to once
        PreUpdate += characterObj.PreInputUpdate;
        PostUpdate += characterObj.PostInputUpdate;
        Pause += characterObj.Pause;
        Unpause += characterObj.Pause;

        if (characterObj.isPlayerOne)
        {
            // Subscribe to player 1 events
            MovementP1 += characterObj.Movement;
            JumpP1 += characterObj.Jump;
            DashP1 += characterObj.Dash;
            HeavyP1 += characterObj.Heavy;

            // Unsubscribe from player 2 events
            MovementP2 -= characterObj.Movement;
            JumpP2 -= characterObj.Jump;
            DashP2 -= characterObj.Dash;
            HeavyP2 -= characterObj.Heavy;
        }
        else
        {
            // Subscribe to player 2 events
            MovementP2 += characterObj.Movement;
            JumpP2 += characterObj.Jump;
            DashP2 += characterObj.Dash;
            HeavyP2 += characterObj.Heavy;

            // Unsubscribe from player 1 events
            MovementP1 -= characterObj.Movement;
            JumpP1 -= characterObj.Jump;
            DashP1 -= characterObj.Dash;
            HeavyP1 -= characterObj.Heavy;
        }
    }

    // Resets the jump hold time
    public void ResetJumpDown(bool player1)
    {
        if (player1)
        {
            player1JumpDown = false;
            player1JumpHold = 0;
        }
        else
        {
            player2JumpDown = false;
            player2JumpHold = 0;
        }
    }

    // Is a player currently requesting a jump?
    public bool IsRequestingJump(bool player1)
    {
        if (player1)
            return Input.GetButton(jumpStr + player1Str);
        else
            return Input.GetButton(jumpStr + player2Str);
    }
}
