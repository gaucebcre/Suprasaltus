using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ScenesController : MonoBehaviour
{
    [SerializeField] private string statsSceneName;


    void Update()
    {
        if (InputManager.menuPressed)
        {
            Debug.Log("hi");
            if (!SceneManager.GetSceneByName(statsSceneName).isLoaded)
            {
                SceneManager.LoadScene(statsSceneName, LoadSceneMode.Additive);
            }
            else
            {
                SceneManager.UnloadSceneAsync(statsSceneName);
            }
        }
    }
}
