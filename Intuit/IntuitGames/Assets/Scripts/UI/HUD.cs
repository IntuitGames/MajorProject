using UnityEngine;
using UnityEngine.UI;using System.Collections;using System.Collections.Generic;using System.Linq;public class HUD : MonoBehaviour{
    public Slider weakenedSlider;
    public bool doWeakenedSliderFade = true;
    public float weakenedSliderFadeRate = 1;
    private Image[] weakenedSliderImages;    void Awake()
    {
        weakenedSliderImages = weakenedSlider.GetComponentsInChildren<Image>();

        for (int i = 0; i < weakenedSliderImages.Length; i++ )
            weakenedSliderImages[i].color = new Color(weakenedSliderImages[i].color.r, weakenedSliderImages[i].color.g, weakenedSliderImages[i].color.b, 0);
    }    void Update()
    {
        weakenedSlider.value = GameManager.PlayerManager.deathPercentage;

        if (doWeakenedSliderFade)
        {
            for (int i = 0; i < weakenedSliderImages.Length; i++)
                if (weakenedSlider.value >= 1)
                    weakenedSliderImages[i].color = new Color(weakenedSliderImages[i].color.r, weakenedSliderImages[i].color.g, weakenedSliderImages[i].color.b,
                        Mathf.Clamp(weakenedSliderImages[i].color.a - (Time.deltaTime * weakenedSliderFadeRate), 0, 1));
                else
                    weakenedSliderImages[i].color = new Color(weakenedSliderImages[i].color.r, weakenedSliderImages[i].color.g, weakenedSliderImages[i].color.b,
                        Mathf.Clamp(weakenedSliderImages[i].color.a + (Time.deltaTime * weakenedSliderFadeRate), 0, 1));
        }
    }}