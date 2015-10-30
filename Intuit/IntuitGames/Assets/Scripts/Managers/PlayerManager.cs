using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;/// <summary>
/// Handles common behavior between both players.
/// </summary>public class PlayerManager : Manager
{
    #region VARIABLES

    public override bool sceneOverride
    {
        get
        {
            return true;
        }
    }

    [Header("Characters")]
    public Character character1;
    public Character character2;

    // WEAKENED
    [Header("Weakened State"), ReadOnly]
    public bool isWeakened = false;
    public bool canWeaken = true;
    [Range(0, 1)]
    public float weakenedMoveSpeedMulti = 0.5f;
    public bool reconnectOnTouch = true;
    public bool canDie = false;
    public float maxJelly = 10;
    public bool autoRecover = false;
    [Range(0, 1)]
    public float recoveryRate = 0.5f;
    [ReadOnly]
    public float currentJelly;

    public float distanceBetweenCharacters
    {
        get { return Vector3.Distance(character1.transform.position, character2.transform.position); }
    }
    public float jellyPercentage
    {
        get { return currentJelly / maxJelly; }
    }

    #endregion

    #region MESSAGES

    void Start()
    {
        currentJelly = maxJelly;

        // Subscribe to tether events
        GameManager.TetherManager.OnDisconnected += Weaken;
        GameManager.TetherManager.OnReconnected += Unweaken;
    }

    void Update()
    {
        if (isWeakened)
            AddJelly(-Time.deltaTime);
        else if (autoRecover)
            AddJelly(Time.deltaTime * recoveryRate);

        if (currentJelly <= 0 && canDie)
            Death();
    }

    #endregion

    public bool SetCharacter(Character character)
    {
        if (!character) return false;

        if (character.isPlayerOne)
            character1 = character;
        else
            character2 = character;

        return true;
    }

    public void Weaken(TetherJoint brokenJoint)
    {
        if (isWeakened) return;

        if (canWeaken)
        {
            isWeakened = true;
            character1.audioData.PlayTetherDisconnectAudio(brokenJoint.transform);
        }
    }

    public void Unweaken(TetherJoint reconnectedJoint)
    {
        if (!isWeakened) return;

        isWeakened = false;

        character1.audioData.PlayTetherConnectAudio(reconnectedJoint.transform);
    }

    public void Death()
    {
#if !UNITY_EDITOR
        Application.Quit();
#else
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void AddJelly(float value)
    {
        currentJelly = Mathf.Clamp(currentJelly + value, 0, maxJelly);
    }
}