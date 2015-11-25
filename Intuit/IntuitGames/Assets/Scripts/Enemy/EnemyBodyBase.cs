using UnityEngine;
using System.Collections;

public abstract class EnemyBodyBase : MonoBehaviour {

    public EnemyBase parentEnemy;
    protected Renderer render;
	protected Rigidbody rigidyBody;


	// Use this for initialization
	protected virtual void Start () {
        this.parentEnemy = GetComponentInParent<EnemyBase>();
        this.render = GetComponent<Renderer> ();
		this.rigidyBody = GetComponent<Rigidbody> ();
	}

	public void Death()
	{
        Debug.Log("Death Called on " + this.gameObject.name);
        StartCoroutine(DeathFade(this.render.material.color, parentEnemy.fadeTime));
        rigidyBody.isKinematic = false;
        rigidyBody.AddForce(Random.insideUnitSphere * parentEnemy.gibForce);
    }

	IEnumerator DeathFade(Color start, float fadeTime){

		Color targetColor = new Color (start.r, start.g, start.b, 0f);

		for (float t = 0; t < fadeTime; t+=Time.deltaTime) {
		
			this.render.material.color = Color.Lerp(start, targetColor, t/fadeTime);
			yield return null;
		}
	}
}
