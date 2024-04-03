using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum SimulationState
{
    START,
    RUNNING,
    PAUSED
}

public class SimulationManager : MonoBehaviour
{
    /* TODO:
     *  - Before Simulation
     *      ##Selection Of Vehical##
     *      ##Toggle Collisions##
     *      ¬Start Simulation 
     *  - During Simulation
     *      ¬Change Simulation Values During Simulation
     *      ¬Change Particle View Values During Simulation
     *      ¬Move Camera Around During Simulation
     *      ¬Pause/Resume During Simulation
     *      ¬Stop Simulation During Simulation
    */

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
    public SimulationState state = SimulationState.START;
    
    public void OnDropDownValueChanged(TMP_Dropdown change)
    {
        mainObjects[_previous].SetActive(false);
        collisionObjects[_previous].SetActive(false);
        mainObjects[change.value].SetActive(true);
        if (showCollisionShapes)
            collisionObjects[change.value].SetActive(true);
        _previous = change.value;
    }

    public void OnToggleValueChanged(Toggle val)
    {
        showCollisionShapes = val.isOn;
        collisionObjects[_previous].SetActive(val.isOn);
    }

    public void StartPressed()
    {
        sphManager.StartSimulation(fileNames[_previous]);
        state = SimulationState.RUNNING;
    }

    public void Update()
    {

        switch(state)
        {
            case SimulationState.START:
                break;
            case SimulationState.RUNNING:
                sphManager.UpdateSimulation();
                particleDisplay.UpdateDisplay();
                break;
            case SimulationState.PAUSED:
                break;
            default:
                break;
        }
    }
}
