using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;/// <summary>
/// Handles common behavior between both players.
/// </summary>public class PlayerManager : Manager
{
    #region VARIABLES

    [Header("Characters")]
    [SerializeField, HideInInspector]
    private Character _character1;
    [SerializeField, HideInInspector]
    private Character _character2;

    public Character character1
    {
        get
        {
            if (_character1)
                return _character1;
            _character1 = GameObject.FindObjectsOfType<Character>().FirstOrDefault(x => x.isPlayerOne);
            return _character1;
        }
    }
    public Character character2
    {
        get
        {
            if (_character2)
                return _character2;
            _character2 = GameObject.FindObjectsOfType<Character>().FirstOrDefault(x => !x.isPlayerOne);
            return _character2;
        }
    }

    [ReadOnly]
    public float collectibleScore;

    // TETHER
    [Header("Tether")]
    public bool constrainMovement = true;
    [Range(0, 10)]
    public float constrainingPower = 2;
    [Range(0, 100)]
    public float baseFreeRadius = 15;
    [Range(0, 100)]
    public float maxRadius = 20;
    [Range(0, 10)]
    public float sprintRadiusExtension = 2;
    [Range(0, 50)]
    public float yankingDashForce = 8;

    // WEAKENED
    [Header("Weakened State"), ReadOnly]
    public bool isWeakened = false;
    public bool canWeaken = true;
    [Range(0, 1)]
    public float weakenedMoveSpeedMulti = 0.5f;
    public bool reconnectOnTouch = true;
    [Popup(new string[] { "Nothing", "Game Over", "Reload Level", "Exit Game" })]
    public string actionOnDeath = "Game Over";
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
    public float freeRadius
    {
        get { return baseFreeRadius + (character1.isSprinting ? sprintRadiusExtension : 0) + (character2.isSprinting ? sprintRadiusExtension : 0); }
    }
    public Vector3 character1Pos
    {
        get { return character1.transform.position; }
    }
    public Vector3 character2Pos
    {
        get { return character2.transform.position; }
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

    public override void ManagerOnLevelLoad()
    {
        collectibleScore = 0;
        currentJelly = maxJelly;
    }

    void Update()
    {
        if (isWeakened)
            AddJelly(-Time.deltaTime);
        else if (autoRecover)
            AddJelly(Time.deltaTime * recoveryRate);

        if (currentJelly <= 0)
            DeathAction();
    }

    #endregion

    public void Weaken(TetherJoint brokenJoint)
    {
        if (isWeakened) return;

        if (canWeaken)
            isWeakened = true;
    }

    public void Unweaken(TetherJoint reconnectedJoint)
    {
        if (!isWeakened) return;

        isWeakened = false;
    }

    public void DeathAction()
    {
        if (actionOnDeath == "Nothing")
            return;
        else if (actionOnDeath == "Reload Level")
            GameManager.ReloadLevel();
        else if (actionOnDeath == "Exit Game")
            GameManager.ExitGame();
        else if (actionOnDeath == "Game Over")
            ModeManager.RequestGameModeChange(global::ModeManager.GameMode.GameOver, true, 0);
    }

    public void AddJelly(float value)
    {
        currentJelly = Mathf.Clamp(currentJelly + value, 0, maxJelly);
    }
}