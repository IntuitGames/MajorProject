﻿using UnityEngine;
using UnityEngine.UI;
using CustomExtensions;
    public Image panel;
    public float fadeOutSpeed = 5;
    public float fadeInSpeed = 5;
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
    }
    {
        if (startDelay > 0)
            yield return WaitForUnscaledSeconds(startDelay);

        while (panel.color.a > 0)
        {
            panel.color = panel.color.SetAlpha(Mathf.Clamp(panel.color.a - (Time.unscaledDeltaTime * fadeInSpeed), 0, 1));
            yield return new WaitForEndOfFrame();
        }

        ActivatePanel(false, 0);
    }
    {
        panel.gameObject.SetActive(state);
        panel.color = panel.color.SetAlpha(alpha);
    }
    {
        float start = Time.realtimeSinceStartup;

        while (Time.realtimeSinceStartup < start + time)
            yield return null;
    }