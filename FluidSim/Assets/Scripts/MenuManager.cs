using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public string sceneName;
    public void StartPressed()
    {
        SceneManager.LoadSceneAsync(sceneName);
    }
}
