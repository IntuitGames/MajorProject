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
    public event Action heavyP1 = delegate { };
    public event Action heavyP2 = delegate { };
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

    private void HandleInGameEvents()
    {
        // Raise axis input events
        forwardP1(Input.GetAxis(forwardStr + player1Str));
        forwardP2(Input.GetAxis(forwardStr + player2Str));
        rightP1(Input.GetAxis(rightStr + player1Str));
        rightP2(Input.GetAxis(rightStr + player2Str));
        movementP1(Input.GetAxis(forwardStr + player1Str), Input.GetAxis(rightStr + player1Str));
        movementP2(Input.GetAxis(forwardStr + player2Str), Input.GetAxis(rightStr + player2Str));

        // Handle jump logic
        if (Input.GetButtonDown(jumpStr + player1Str))
            player1JumpDown = true;
        if (Input.GetButtonDown(jumpStr + player2Str))
            player1JumpDown = true;

        // Perform jump when releasing the jump button
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

            player1JumpDown = false;
        }

        if (player1JumpDown)
        {
            if(player1JumpHold <= highJumpThreshold)
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

        // Raise action button event
        if (Input.GetButtonDown(dashStr + player1Str))
            dashP1();
        if (Input.GetButtonDown(dashStr + player2Str))
            dashP2();
        if (Input.GetButtonDown(heavyStr + player1Str))
            heavyP1();
        if (Input.GetButtonDown(heavyStr + player2Str))
            heavyP2();

        // Pause and unpause check
        if (Input.GetButtonDown(pauseStr + player1Str))
            pause();
        if (Input.GetButtonDown(pauseStr + player2Str))
            pause();
    }

    private void HandlePauseMenuEvents()
    {
        if (Input.GetButtonDown(unpauseStr + player1Str))
            unpause();
        if (Input.GetButtonDown(unpauseStr + player2Str))
            unpause();
    }
}
