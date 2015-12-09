using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using UnityEngine.UI;
using CustomExtensions;public class Fader : MonoBehaviour{
    public Image panel;
    public float fadeOutSpeed = 5;
    public float fadeInSpeed = 5;    public IEnumerator FadeOut(float startDelay, float endDelay, System.Action action)
    {
        if (startDelay > 0)
            yield return WaitForUnscaledSeconds(startDelay);

        while (panel.color.a < 1)
        {
            panel.color = panel.color.SetAlpha(Mathf.Clamp(panel.color.a + (Time.unscaledDeltaTime * fadeOutSpeed), 0, 1));
            yield return new WaitForEndOfFrame();
        }

        if (endDelay > 0)
            yield return WaitForUnscaledSeconds(endDelay);

        if (action != null)
            action();

        ActivatePanel(true, 1);
    }    public IEnumerator FadeIn(float startDelay)
    {
        if (startDelay > 0)
            yield return WaitForUnscaledSeconds(startDelay);

        while (panel.color.a > 0)
        {
            panel.color = panel.color.SetAlpha(Mathf.Clamp(panel.color.a - (Time.unscaledDeltaTime * fadeInSpeed), 0, 1));
            yield return new WaitForEndOfFrame();
        }

        ActivatePanel(false, 0);
    }    public void ActivatePanel(bool state, float alpha)
    {
        panel.gameObject.SetActive(state);
        panel.color = panel.color.SetAlpha(alpha);
    }    // Like WaitForSeconds except it is unaffected by time scale    private IEnumerator WaitForUnscaledSeconds(float time)
    {
        float start = Time.realtimeSinceStartup;

        while (Time.realtimeSinceStartup < start + time)
            yield return null;
    }}