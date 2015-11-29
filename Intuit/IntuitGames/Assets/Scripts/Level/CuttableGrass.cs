using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;public class CuttableGrass : Trigger{
    public GameObject uncutGrassObject;
    public GameObject cutGrassObject;
    public GameObject grassJoint;
    public List<ParticleSystem> particleSystems;
    public bool requireBothToDash = false;
    public float scoreValue;
    public SoundClip cutSound = new SoundClip(AudioManager.Player.FMOD, AudioManager.Type.SoundEffect, AudioManager.Group.Game);
    public bool randomizeOnStart = false;
    public FloatRange randomScale = new FloatRange() { normal = 1, min = 0.8f, max = 1.2f };

    protected override bool canBeTriggered
    {
        get
        {
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
        cutGrassObject.SetActive(true);
        uncutGrassObject.SetActive(false);

        particleSystems.ForEach(x => x.Play());

        GameManager.PlayerManager.collectibleScore += scoreValue;
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