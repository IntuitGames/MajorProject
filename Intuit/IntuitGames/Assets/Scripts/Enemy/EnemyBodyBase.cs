using UnityEngine;
using System.Collections;

public class EnemyBodyBase : MonoBehaviour {
	
	protected Enemy parentEnemy;
	private Renderer renderer;

	// Use this for initialization
	void Start () {
		this.parentEnemy = GetComponentInParent<Enemy> ();
		this.renderer = GetComponent<Renderer> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Death()
	{
		StartCoroutine (DeathFade (parentEnemy.fadeTime));
	}

	IEnumerator DeathFade(float fadeTime){

		while (this.renderer.material.color.a <= 0f) 
		{
			Color current = this.renderer.material.color;
			this.renderer.material.color = new Color(current.r, current.g, current.b, (current.a - (fadeTime * Time.deltaTime)));
			yield return null;
		}


	}
}
