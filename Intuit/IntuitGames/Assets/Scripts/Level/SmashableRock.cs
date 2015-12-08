using UnityEngine;
using System.Collections;
using CustomExtensions;

public class SmashableRock : Trigger
{
	public GameObject unsmashedRock;
	public GameObject smashedRock;
	public Pickup pickupDrop;
	public Unity.Chance dropChance = 0.75f;
	public float scoreValue;
	public SoundClip smashSound = new SoundClip(AudioManager.Player.FMOD, AudioManager.Type.SoundEffect, AudioManager.Group.Game);

	void Awake()
	{
		unsmashedRock.SetActive(true);
		smashedRock.SetActive(false);
	}

	void Start()
	{
		smashSound.Initialize();
	}

	void OnDestroy()
	{
		smashSound.Dispose();
	}

	protected override bool canBeTriggered
	{
		get
		{
			return GameManager.PlayerManager.character1.isHeavy && !GameManager.PlayerManager.character1.isGrounded
				|| GameManager.PlayerManager.character2.isHeavy && !GameManager.PlayerManager.character2.isGrounded;
		}
	}

	protected override void OnTrigger (GameObject triggerObject)
	{
		// Swap game objects
		smashedRock.SetActive(true);
		unsmashedRock.SetActive(false);

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
		smashSound.PlayAttached(GetComponent<AudioSource>(), AudioManager.GetFMODAttribute(transform, Vector3.zero), 1, new float[] { 0, GameManager.TetherManager.weakenedParam } );

		// Disable collider
		colliderComp.enabled = false;
	}
}
