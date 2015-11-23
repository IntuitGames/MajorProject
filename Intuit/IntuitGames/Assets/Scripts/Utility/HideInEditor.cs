using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;[ExecuteInEditMode]public class HideInEditor : MonoBehaviour{
    public bool show = false;
    public GameObject[] gameObjects;
    private bool isShown;    void Update()
    {
        if (!Application.isPlaying)
        {
            if (isShown != show)
            {
                foreach (var GO in gameObjects)
                {
                    GO.SetActive(show);
                }
                isShown = show;
            }
        }
        else
        {
            if (!isShown)
            {
                foreach(var GO in gameObjects)
                {
                    GO.SetActive(true);
                }
                isShown = true;
            }
        }
    }}