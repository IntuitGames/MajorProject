﻿using UnityEngine;
/// Basic player pickups that trigger when touched.
/// </summary>
    public float jellyValue = 0;
    {
        if (col.GetComponent<Character>())
        {
            OnPickup();
        }
    }
    {
        GameManager.PlayerManager.AddDeathTime(jellyValue);
        gameObject.SetActive(false);
    }