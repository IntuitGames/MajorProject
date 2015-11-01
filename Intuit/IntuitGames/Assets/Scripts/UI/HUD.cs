using UnityEngine;
using UnityEngine.UI;using System.Collections;using System.Collections.Generic;using System.Linq;public class HUD : MonoBehaviour{
    public Slider weakenedSlider;
    public Text weakenedTextValue;    void Update()
    {
        weakenedSlider.value = GameManager.PlayerManager.jellyPercentage;

        if (GameManager.PlayerManager.currentJelly < 0.5f && GameManager.PlayerManager.currentJelly > 0)
            weakenedTextValue.text = "1";
        else if (GameManager.PlayerManager.currentJelly < GameManager.PlayerManager.maxJelly && GameManager.PlayerManager.currentJelly > GameManager.PlayerManager.maxJelly - 0.5f)
            weakenedTextValue.text = (GameManager.PlayerManager.maxJelly - 1).ToString();
        else
            weakenedTextValue.text = Mathf.RoundToInt(GameManager.PlayerManager.currentJelly).ToString();
    }}