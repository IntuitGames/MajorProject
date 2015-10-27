using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;/// <summary>
/// Basic player pickups that trigger when touched.
/// </summary>public class Pickup : MonoBehaviour{
    public float jellyValue = 0;    void OnTriggerEnter(Collider col)
    {
        if (col.GetComponent<Character>())
        {
            OnPickup();
        }
    }    private void OnPickup()
    {
        GameManager.PlayerManager.AddDeathTime(jellyValue);
        gameObject.SetActive(false);
    }}