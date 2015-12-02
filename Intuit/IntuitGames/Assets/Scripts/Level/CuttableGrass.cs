using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using CustomExtensions;public class CuttableGrass : Trigger{
    public GameObject uncutGrassObject;
    public GameObject cutGrassObject;
    public GameObject grassJoint;
    public Pickup pickupDrop;
    public Unity.Chance dropChance = 0.25f;
    public bool requireBothToDash = false;
    public bool requireNotWeakened = false;
    public float scoreValue;
    public SoundClip cutSound = new SoundClip(AudioManager.Player.FMOD, AudioManager.Type.SoundEffect, AudioManager.Group.Game);

    [Header("Transformation Randomization")]
    public bool randomizeOnStart = false;
    public FloatRange randomScale = new FloatRange() { normal = 1, min = 0.8f, max = 1.2f };

    protected override bool canBeTriggered
    {
        get
        {
            if (requireNotWeakened && GameManager.PlayerManager.isWeakened) return false;
            if (requireBothToDash)
                return GameManager.PlayerManager.character1.isDashing && GameManager.PlayerManager.character2.isDashing;
            else
                return GameManager.PlayerManager.character1.isDashing || GameManager.PlayerManager.character2.isDashing;
        }
    }

    void Awake()
    {
        cutGrassObject.SetActive(false);
        uncutGrassObject.SetActive(true);

        if (randomizeOnStart)
            Randomize();
    }

    void Start()
    {
        cutSound.Initialize();
    }

    void OnDestroy()
    {
        cutSound.Dispose();
    }

    protected override void OnTrigger(GameObject triggerObject)
    {
        // Swap game objects
        cutGrassObject.SetActive(true);
        uncutGrassObject.SetActive(false);

        // Play particle effects
        foreach (var particleSys in GetComponentsInChildren<ParticleSystem>())
        {
            particleSys.gameObject.SetActive(true);
            particleSys.enableEmission = true;
            particleSys.Play();
        }

        // Add to score
        GameManager.PlayerManager.collectibleScore += scoreValue;

        // Chance drop
        if (dropChance && pickupDrop)
        {
            Pickup newDrop = (Pickup)Instantiate(pickupDrop, transform.position, pickupDrop.transform.rotation);

            // Delay its activation
            newDrop.enabled = false;
            TimerPlus.Create(1, () => newDrop.enabled = true);
        }

        // Play sound
        cutSound.PlayAttached(GetComponent<AudioSource>(), AudioManager.GetFMODAttribute(transform, Vector3.zero), 1);
    }

    public void Randomize()
    {
        float newScale = randomScale.GetRandom();
        grassJoint.transform.localScale = new Vector3(newScale, newScale, newScale);
        grassJoint.transform.eulerAngles = new Vector3(0, Random.Range(-180, 180), 0);
        cutGrassObject.transform.localScale = new Vector3(newScale, randomScale.normal, newScale);
    }

    public void Reset()
    {
        grassJoint.transform.localScale = new Vector3(randomScale.normal, randomScale.normal, randomScale.normal);
        grassJoint.transform.eulerAngles = Vector3.zero;
        cutGrassObject.transform.localScale = new Vector3(randomScale.normal, randomScale.normal, randomScale.normal);
    }
}