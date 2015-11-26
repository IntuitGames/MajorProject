using UnityEngine;
using System.Collections;

public abstract class EnemyBodyBase : MonoBehaviour {

    public EnemyBase parentEnemy;
    protected Renderer render;
	protected Rigidbody rigidBody;


	// Use this for initialization
	protected virtual void Start () {
        this.parentEnemy = GetComponentInParent<EnemyBase>();
        this.render = GetComponent<Renderer> ();
		this.rigidBody = GetComponent<Rigidbody> ();
	}

	public void Death()
	{
        Debug.Log("Death Called on " + this.gameObject.name);
        StartCoroutine(DeathFade(this.render.material.color, parentEnemy.fadeTime));
        rigidBody.isKinematic = false;
        rigidBody.AddForce(Random.insideUnitSphere * parentEnemy.gibForce);
		if (this.GetComponent<Collider> () != null)
			this.GetComponent<Collider> ().enabled = false;
		
    }

	IEnumerator DeathFade(Color start, float fadeTime){

		Color targetColor = new Color (start.r, start.g, start.b, 0f);
		for (float t = 0; t < fadeTime; t+=Time.deltaTime) {
			yield return null;
		}

	}
}
