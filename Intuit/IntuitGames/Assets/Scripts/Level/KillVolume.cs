﻿using UnityEngine;
    protected override void OnTrigger(GameObject triggerObject)
    {
        GameManager.PlayerManager.DeathAction();
    }
}