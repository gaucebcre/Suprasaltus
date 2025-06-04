using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject[] objectsToEnable;
    [SerializeField] private GameObject[] objectsToDisable;
    bool localActiveState = true;

    void Update()
    {
        bool walkAnimationState = GetComponent<PlayerMovement>().playerMoveStats.walkAnimation;
        if (!localActiveState && walkAnimationState)
        {
            localActiveState = true;
            EnableAnimationObjs();
        }
        if (localActiveState && !walkAnimationState)
        {
            localActiveState = false;
            DisableAnimationObjs();
        }
    }

    void DisableAnimationObjs()
    {
        foreach (GameObject obj in objectsToDisable)
        {
            obj.SetActive(false);
        }

        foreach (GameObject obj in objectsToEnable)
        {
            obj.SetActive(true);
        }
    }

    void EnableAnimationObjs()
    {
        foreach (GameObject obj in objectsToDisable)
        {
            obj.SetActive(true);
        }

        foreach (GameObject obj in objectsToEnable)
        {
            obj.SetActive(false);
        }
    }
}
