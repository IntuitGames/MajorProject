using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//just a sneak comment to note rotations as needed on the Level1 prefab:
// Environment rot = 0, 90, 0
// Art  pos = 129.14, 0, -145.3
//      rot = 0, -90, 0

public class deathModelHandler : MonoBehaviour {

    public EnemyBase parentEnemy;
    List<GameObject> parts = new List<GameObject>();

    void Start ()
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
