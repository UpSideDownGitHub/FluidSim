using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Current State of the simulation
/// </summary>
public enum SimulationState
{
    START,
    RUNNING,
    PAUSED
}

/// <summary>
/// Manages the overal simulation (when SPH runs and the high level menu interaction)
/// </summary>
public class SimulationManager : MonoBehaviour
{
    [Header("UI Management")]
    public GameObject startUI;
    public GameObject runningUI;


    [Header("Vehical Selection")]
    /* Order:
     *  0 -> Ambulance 
     *  1 -> Bus
     *  2 -> Firetruck
     *  3 -> Monster Truck
     *  4 -> Muscle
     *  5 -> Roadster
     *  6 -> Sports
     *  7 -> Truck
    */
    public string[] fileNames;
    public GameObject[] mainObjects;
    public GameObject[] collisionObjects;
    private int _previous = 0;
    public bool showCollisionShapes;

    [Header("Simulation Management")]
    public ComputeSPHManager sphManager;
    public ParticleDisplay3D particleDisplay;
    public UIManager uiManager;
    public SimulationState state = SimulationState.START;

    [Header("Collision Balls")]
    public ConvertBalls ballConverter;

    /// <summary>
    /// Starts this instance.
    /// </summary>
    public void Start()
    {
        for (int i = 0; i < collisionObjects.Length; i++)
        {
            string path = Application.persistentDataPath + "/" + fileNames[i];
            if (!File.Exists(path))
            {
                ballConverter.ballsParent = collisionObjects[i].transform;
                ballConverter.fileName = fileNames[i];
                ballConverter.SaveBallData();
            }
        }
        mainObjects[_previous].SetActive(true);
    }

    /// <summary>
    /// Called when [drop down value changed].
    /// </summary>
    /// <param name="change">The change.</param>
    public void OnDropDownValueChanged(TMP_Dropdown change)
    {
        mainObjects[_previous].SetActive(false);
        collisionObjects[_previous].SetActive(false);
        mainObjects[change.value].SetActive(true);
        if (showCollisionShapes)
            collisionObjects[change.value].SetActive(true);
        _previous = change.value;
    }

    /// <summary>
    /// Called when [toggle value changed].
    /// </summary>
    /// <param name="val">The value.</param>
    public void OnToggleValueChanged(Toggle val)
    {
        showCollisionShapes = val.isOn;
        collisionObjects[_previous].SetActive(val.isOn);
    }

    /// <summary>
    /// Initializes the particle display.
    /// </summary>
    public void InitParticleDisplay()
    {
        particleDisplay.Init(sphManager);
    }

    /// <summary>
    /// Called when [start pressed].
    /// </summary>
    public void StartPressed()
    {
        sphManager.StartSimulation(fileNames[_previous]);
        InitParticleDisplay();
        state = SimulationState.RUNNING;
        startUI.SetActive(false);
        runningUI.SetActive(true);
    }

    /// <summary>
    /// Resets the system.
    /// </summary>
    public void ResetSystem()
    {
        state = SimulationState.START;
        sphManager.DestroyCurrent();
        sphManager.StartSimulation(fileNames[_previous]);
        InitParticleDisplay();
        state = SimulationState.RUNNING;
        startUI.SetActive(false);
        runningUI.SetActive(true);
    }

    /// <summary>
    /// Stops the simulation.
    /// </summary>
    public void StopSimulation()
    {
        state = SimulationState.START;
        sphManager.DestroyCurrent();
        startUI.SetActive(true);
        runningUI.SetActive(false);
    }

    /// <summary>
    /// Updates this instance.
    /// </summary>
    public void Update()
    {
        // change actions based on the current state of the simulation
        switch(state)
        {
            case SimulationState.START:
                break;
            case SimulationState.RUNNING:
                sphManager.UpdateSimulation();
                particleDisplay.UpdateDisplay();
                break;
            case SimulationState.PAUSED:
                particleDisplay.UpdateDisplay();
                break;
            default:
                break;
        }
    }
}
