using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the Main menu of the simulation
/// </summary>
public class MenuManager : MonoBehaviour
{
    public string sceneName;
    /// <summary>
    /// Start the pressed.
    /// </summary>
    public void StartPressed()
    {
        // load the main scene
        SceneManager.LoadSceneAsync(sceneName);
    }
}
