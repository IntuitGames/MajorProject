using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;// Temporary (For week 13 build)public class AnimCallReceiver : MonoBehaviour{    void FootStep(int footStepIndex)
    {
        SendMessageUpwards("OnFootStep", footStepIndex, SendMessageOptions.DontRequireReceiver);
    }}