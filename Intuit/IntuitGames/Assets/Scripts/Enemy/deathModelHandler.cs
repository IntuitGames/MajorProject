using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gets all parts of the death model and fades them out.
/// </summary>

public class DeathModelHandler : MonoBehaviour {

    public Enemy parentEnemy;
    List<GameObject> parts = new List<GameObject>();

    void OnEnable ()
    {
        foreach (Transform trans in transform)
        {
            parts.Add(trans.gameObject);
            StartCoroutine(fadeOutOb(trans.gameObject, parentEnemy.fadeTime));
            trans.GetComponent<Rigidbody>().AddForce(Random.insideUnitSphere * parentEnemy.gibForce);
        }
    }

    void Update()
    {
        if (parts.Count == 0)
        {
            parentEnemy.DestroyMe();
        }
    }
    
    IEnumerator fadeOutOb(GameObject ob, float duration)
    {
        Renderer rend = ob.GetComponent<Renderer>();
        Color start = rend.material.color;
        Color end = start;
        end.a = 0f;
        for(float t = 0; t < duration; t +=Time.deltaTime)
        {
            rend.material.color = Color.Lerp(start, end, t / duration);
            yield return null;
        }
        parts.Remove(ob);
    }

}
