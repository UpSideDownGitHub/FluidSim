using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct SPHValues
{
    public float gravityX;
    public float gravityY;
    public float restDensity;
    public float gasConstant;
    public float kernalRadius;
    public float mass;
    public float viscosity;
    public float timeStep;
    public float boundaryDamping;
    public float interactionRadius;
}

[RequireComponent(typeof(Manager))]
public class UIManager : MonoBehaviour
{
    private Manager _manager;

    [Header("UI Elements")]
    public TMP_InputField gravityXInput;
    public TMP_InputField gravityYInput;
    public TMP_Text restDensityText;
    public Slider restDensitySlider;
    public TMP_Text gasConstantText;
    public Slider gasConstantSlider;
    public TMP_Text kernalRadiusText;
    public Slider kernalRadiusSlider;
    public TMP_Text massText;
    public Slider massSlider;
    public TMP_Text viscosityText;
    public Slider viscositySlider;
    public TMP_Text timeStepText;
    public Slider timeStepSlider;
    public TMP_Text boundaryDampingText;
    public Slider boundaryDampingSlider;
    public TMP_Text interactionRadiusText;
    public Slider interactionRadiusSlider;
    public Button resetButton;
    public Button clearButton;
    public Button toggleTapButton;
    public Button resetValuesButton;

    [Header("Values")]
    public SPHValues min;
    public SPHValues max;
    public SPHValues orignal;

    [Header("Menu")]
    public GameObject menu;
    public GameObject open;

    public void OpenMenu()
    {
        open.SetActive(false);
        menu.SetActive(true);
    }
    public void CloseMenu()
    {
        menu.SetActive(false);
        open.SetActive(true);
    }

    public void Start()
    {
        _manager = GetComponent<Manager>();
        SetOrignalValues();

        gravityXInput.onValueChanged.AddListener((val) => gravityXChanged(val));
        gravityXInput.text = _manager.gravity.x.ToString();
        gravityYInput.onValueChanged.AddListener((val) => gravityYChanged(val));
        gravityYInput.text = _manager.gravity.y.ToString();

        restDensitySlider.minValue = min.restDensity;
        restDensitySlider.maxValue = max.restDensity;
        restDensitySlider.value = orignal.restDensity;
        restDensitySlider.onValueChanged.AddListener((val) => restDesnityChanged(val));
        restDensityText.text = _manager.restDensity.ToString();

        gasConstantSlider.minValue = min.gasConstant;
        gasConstantSlider.maxValue = max.gasConstant;
        gasConstantSlider.value = orignal.gasConstant;
        gasConstantSlider.onValueChanged.AddListener((val) => gasConstantChanged(val));
        gasConstantText.text = _manager.gasConstant.ToString();

        kernalRadiusSlider.minValue = min.kernalRadius;
        kernalRadiusSlider.maxValue = max.kernalRadius;
        kernalRadiusSlider.value = orignal.kernalRadius;
        kernalRadiusSlider.onValueChanged.AddListener((val) => kernalRadiusChanged(val));
        kernalRadiusText.text = _manager.kernalRadius.ToString();

        massSlider.minValue = min.mass;
        massSlider.maxValue = max.mass;
        massSlider.value = orignal.mass;
        massSlider.onValueChanged.AddListener((val) => massChanged(val));
        massText.text = _manager.mass.ToString();

        viscositySlider.minValue = min.viscosity;
        viscositySlider.maxValue = max.viscosity;
        viscositySlider.value = orignal.viscosity;
        viscositySlider.onValueChanged.AddListener((val) => viscosityChanged(val));
        viscosityText.text = _manager.viscosityConst.ToString();

        timeStepSlider.minValue = min.timeStep;
        timeStepSlider.maxValue = max.timeStep;
        timeStepSlider.value = orignal.timeStep;
        timeStepSlider.onValueChanged.AddListener((val) => timeStepChanged(val));
        timeStepText.text = _manager.timeStep.ToString();

        boundaryDampingSlider.minValue = min.boundaryDamping;
        boundaryDampingSlider.maxValue = max.boundaryDamping;
        boundaryDampingSlider.value = orignal.boundaryDamping;
        boundaryDampingSlider.onValueChanged.AddListener((val) => boundaryDampingChanged(val));
        boundaryDampingText.text = _manager.boundaryDamping.ToString();

        interactionRadiusSlider.minValue = min.interactionRadius;
        interactionRadiusSlider.maxValue = max.interactionRadius;
        interactionRadiusSlider.value = orignal.interactionRadius;
        interactionRadiusSlider.onValueChanged.AddListener((val) => interactionRadiusChanged(val));
        interactionRadiusText.text = _manager.interactionRadius.ToString();

        resetButton.onClick.AddListener(() => ResetPressed());
        clearButton.onClick.AddListener(() => ClearPressed());
        toggleTapButton.onClick.AddListener(() => TapPressed());
        resetValuesButton.onClick.AddListener(() => resetToOrignalValues());
    }

    public void gravityXChanged(string val)
    {
        _manager.gravity.x = float.Parse(val);
        gravityXInput.text = _manager.gravity.x.ToString();
    }
    public void gravityYChanged(string val)
    {
        _manager.gravity.y = float.Parse(val);
        gravityYInput.text = _manager.gravity.y.ToString();

    }
    public void restDesnityChanged(float val)
    {
        _manager.restDensity = val;
        restDensityText.text = _manager.restDensity.ToString();
    }
    public void gasConstantChanged(float val)
    {
        _manager.gasConstant = val;
        gasConstantText.text = _manager.gasConstant.ToString();
    }
    public void kernalRadiusChanged(float val)
    {
        _manager.kernalRadius = val;
        kernalRadiusText.text = _manager.kernalRadius.ToString();
    }
    public void massChanged(float val)
    {
        _manager.mass = val;
        massText.text = _manager.mass.ToString();
    }
    public void viscosityChanged(float val)
    {
        _manager.viscosityConst = val;
        viscosityText.text = _manager.viscosityConst.ToString();
    }
    public void timeStepChanged(float val)
    {
        _manager.timeStep = val;
        timeStepText.text = _manager.timeStep.ToString();
    }
    public void boundaryDampingChanged(float val)
    {
        _manager.boundaryDamping = val;
        boundaryDampingText.text = _manager.boundaryDamping.ToString();
    }
    public void interactionRadiusChanged(float val)
    {
        _manager.interactionRadius = val;
        interactionRadiusText.text = _manager.interactionRadius.ToString();
    }
    public void ResetPressed()
    {
        _manager.ResetParticles();
    }
    public void ClearPressed()
    {
        _manager.ClearParticles();
    }
    public void TapPressed()
    {
        _manager.ToggleTap();
    }
    public void resetToOrignalValues()
    {
        _manager.gravity.x = orignal.gravityX;
        gravityXInput.text = _manager.gravity.x.ToString();
        _manager.gravity.y = orignal.gravityY;
        gravityYInput.text = _manager.gravity.y.ToString();
        
        _manager.restDensity = orignal.restDensity;
        restDensitySlider.value = orignal.restDensity;
        restDensityText.text = _manager.restDensity.ToString();
        _manager.gasConstant = orignal.gasConstant;
        gasConstantSlider.value = orignal.gasConstant;
        gasConstantText.text = _manager.gasConstant.ToString();
        _manager.kernalRadius = orignal.kernalRadius;
        kernalRadiusSlider.value = orignal.kernalRadius;
        kernalRadiusText.text = _manager.kernalRadius.ToString();
        _manager.mass = orignal.mass;
        massSlider.value = orignal.mass;
        massText.text = _manager.mass.ToString();
        _manager.viscosityConst = orignal.viscosity;
        viscositySlider.value = orignal.viscosity;
        viscosityText.text = _manager.viscosityConst.ToString();
        _manager.timeStep = orignal.timeStep;
        timeStepSlider.value = orignal.timeStep;
        timeStepText.text = _manager.timeStep.ToString();
        _manager.boundaryDamping = orignal.boundaryDamping;
        boundaryDampingSlider.value = orignal.boundaryDamping;
        boundaryDampingText.text = _manager.boundaryDamping.ToString();
        _manager.interactionRadius = orignal.interactionRadius;
        interactionRadiusSlider.value = orignal.interactionRadius;
        interactionRadiusText.text = _manager.interactionRadius.ToString();
    }

    public void SetOrignalValues()
    {
        orignal.gravityX = _manager.gravity.x;
        orignal.gravityY = _manager.gravity.y;
        orignal.restDensity = _manager.restDensity;
        orignal.gasConstant = _manager.gasConstant;
        orignal.kernalRadius = _manager.kernalRadius;
        orignal.mass = _manager.mass;
        orignal.viscosity = _manager.viscosityConst;
        orignal.timeStep = _manager.timeStep;
        orignal.boundaryDamping = _manager.boundaryDamping;
        orignal.interactionRadius = _manager.interactionRadius;
    }
}
