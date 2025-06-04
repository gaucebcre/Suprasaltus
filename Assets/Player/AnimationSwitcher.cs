using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject[] objects;
    bool activeState;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        bool walkAnimationState = GetComponent<PlayerMovementStats>().walkAnimation;
        if (!activeState && walkAnimationState)
        {
            activeState = true;
            EnableAnimationObjs();
        }
        if (activeState && !walkAnimationState)
        {
            activeState = false;
            DisableAnimationObjs();
        }
    }

    void DisableAnimationObjs()
    {
        foreach (GameObject obj in objects)
        {
            obj.SetActive(false);
        }
    }

    void EnableAnimationObjs()
    {
        foreach (GameObject obj in objects)
        {
            obj.SetActive(true);
        }
    }
}
