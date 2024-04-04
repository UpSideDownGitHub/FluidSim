using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct SPHValues
{
    public Vector3 gravity;
    public float restDensity;
    public float gasConstant;
    public float kernalRadius;
    public float mass;
    public float viscosity;
    public float timeStep;
    public float boundaryDamping;
    public float collisionSphereRadius;
    public float collisionMass;
}

[Serializable]
public struct ShaderValues
{
    public float scale;
    public float[] maxValues;
    public int gradientResolution;
}

public class UIManager : MonoBehaviour
{
    public ComputeSPHManager sphManager;
    public SimulationManager simulationManager;
    public ParticleDisplay3D particleDisplay;

    [Header("UI Elements")]
    public TMP_InputField gravityXInput;
    public TMP_InputField gravityYInput;
    public TMP_InputField gravityZInput;
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
    public TMP_Text collisionSphereRadiusText;
    public Slider collisionSphereRadiusSlider;
    public TMP_Text collisionMassText;
    public Slider collisionMassSlider;
    
    public Button resetButton;
    public Button stopButton;
    public Button pauseButton;

    public Button resetValuesButton;

    [Header("Material Settings")]
    public TMP_Text scaleText;
    public Slider scaleSlider;
    public TMP_Text maxValueText;
    public Slider maxValueSlider;
    public TMP_Text gradientResolutionText;
    public Slider gradientResolutionSlider;

    public TMP_Dropdown shadersDropdown;
    public TMP_Dropdown colorMapsDropdown;

    public Button resetShaderButton;

    [Header("Values")]
    public SPHValues min;
    public SPHValues max;
    public SPHValues orignal;
    public ShaderValues minShader;
    public ShaderValues maxShader;
    public ShaderValues orignalShader;

    [Header("Menu")]
    public GameObject menu;
    public GameObject open;

    public GameObject shaderMenu;
    public GameObject openShader;

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
    public void OpenShaderMenu()
    {
        shaderMenu.SetActive(true);
        openShader.SetActive(false);
    }
    public void CloseShaderMenu()
    {
        shaderMenu.SetActive(false);
        openShader.SetActive(true);
    }

    public void OnEnable()
    {
        SetOrignalValues();

        // SPH
        gravityXInput.onValueChanged.AddListener((val) => gravityXChanged(val));
        gravityXInput.text = sphManager.gravity.x.ToString();
        gravityYInput.onValueChanged.AddListener((val) => gravityYChanged(val));
        gravityYInput.text = sphManager.gravity.y.ToString();
        gravityZInput.onValueChanged.AddListener((val) => gravityZChanged(val));
        gravityZInput.text = sphManager.gravity.z.ToString();

        restDensitySlider.minValue = min.restDensity;
        restDensitySlider.maxValue = max.restDensity;
        restDensitySlider.value = orignal.restDensity;
        restDensitySlider.onValueChanged.AddListener((val) => restDesnityChanged(val));
        restDensityText.text = sphManager.restDensity.ToString();

        gasConstantSlider.minValue = min.gasConstant;
        gasConstantSlider.maxValue = max.gasConstant;
        gasConstantSlider.value = orignal.gasConstant;
        gasConstantSlider.onValueChanged.AddListener((val) => gasConstantChanged(val));
        gasConstantText.text = sphManager.gasConstant.ToString();

        kernalRadiusSlider.minValue = min.kernalRadius;
        kernalRadiusSlider.maxValue = max.kernalRadius;
        kernalRadiusSlider.value = orignal.kernalRadius;
        kernalRadiusSlider.onValueChanged.AddListener((val) => kernalRadiusChanged(val));
        kernalRadiusText.text = sphManager.kernalRadius.ToString();

        massSlider.minValue = min.mass;
        massSlider.maxValue = max.mass;
        massSlider.value = orignal.mass;
        massSlider.onValueChanged.AddListener((val) => massChanged(val));
        massText.text = sphManager.mass.ToString();

        viscositySlider.minValue = min.viscosity;
        viscositySlider.maxValue = max.viscosity;
        viscositySlider.value = orignal.viscosity;
        viscositySlider.onValueChanged.AddListener((val) => viscosityChanged(val));
        viscosityText.text = sphManager.viscosityCount.ToString();

        timeStepSlider.minValue = min.timeStep;
        timeStepSlider.maxValue = max.timeStep;
        timeStepSlider.value = orignal.timeStep;
        timeStepSlider.onValueChanged.AddListener((val) => timeStepChanged(val));
        timeStepText.text = sphManager.timeStep.ToString();

        boundaryDampingSlider.minValue = min.boundaryDamping;
        boundaryDampingSlider.maxValue = max.boundaryDamping;
        boundaryDampingSlider.value = orignal.boundaryDamping;
        boundaryDampingSlider.onValueChanged.AddListener((val) => boundaryDampingChanged(val));
        boundaryDampingText.text = sphManager.boundaryDamping.ToString();

        collisionSphereRadiusSlider.minValue = min.collisionSphereRadius;
        collisionSphereRadiusSlider.maxValue = max.collisionSphereRadius;
        collisionSphereRadiusSlider.value = orignal.collisionSphereRadius;
        collisionSphereRadiusSlider.onValueChanged.AddListener((val) => collisionSphereRadiusChanged(val));
        collisionSphereRadiusText.text = sphManager.collisionSphereRadius.ToString();

        collisionMassSlider.minValue = min.collisionMass;
        collisionMassSlider.maxValue = max.collisionMass;
        collisionMassSlider.value = orignal.collisionMass;
        collisionMassSlider.onValueChanged.AddListener((val) => collisionMassChanged(val));
        collisionMassText.text = sphManager.collisionMass.ToString();

        resetButton.onClick.AddListener(() => ResetPressed());
        pauseButton.onClick.AddListener(() => PausePressed());
        stopButton.onClick.AddListener(() => StopPressed());
        resetValuesButton.onClick.AddListener(() => resetToOrignalValues());

        // shader
        scaleSlider.minValue = minShader.scale;
        scaleSlider.maxValue = maxShader.scale;
        scaleSlider.value = orignalShader.scale;
        scaleText.text = orignalShader.scale.ToString();
        scaleSlider.onValueChanged.AddListener((value) => OnScaleSliderChanged(value));
        maxValueSlider.minValue = minShader.maxValues[particleDisplay.currentShaderID];
        maxValueSlider.maxValue = maxShader.maxValues[particleDisplay.currentShaderID];
        maxValueSlider.value = orignalShader.maxValues[particleDisplay.currentShaderID];
        maxValueText.text = orignalShader.scale.ToString();
        maxValueSlider.onValueChanged.AddListener((value) => OnMaxValueSliderChanged(value));
        gradientResolutionSlider.minValue = minShader.gradientResolution;
        gradientResolutionSlider.maxValue = maxShader.gradientResolution;
        gradientResolutionSlider.value = orignalShader.gradientResolution;
        gradientResolutionText.text = orignalShader.gradientResolution.ToString();
        gradientResolutionSlider.onValueChanged.AddListener((value) => OnGradientResolutionChanged(value));

        shadersDropdown.onValueChanged.AddListener((value) => OnShadersDropDownChanged(value));
        colorMapsDropdown.onValueChanged.AddListener((value) => OnColorMapsChanged(value));

        resetShaderButton.onClick.AddListener(() => ResetShaderValues());
    }

    public void gravityXChanged(string val)
    {
        sphManager.gravity.x = float.Parse(val);
        gravityXInput.text = sphManager.gravity.x.ToString();
    }
    public void gravityYChanged(string val)
    {
        sphManager.gravity.y = float.Parse(val);
        gravityYInput.text = sphManager.gravity.y.ToString();
    }
    public void gravityZChanged(string val)
    {
        sphManager.gravity.z = float.Parse(val);
        gravityZInput.text = sphManager.gravity.z.ToString();

    }
    public void restDesnityChanged(float val)
    {
        sphManager.restDensity = val;
        restDensityText.text = sphManager.restDensity.ToString();
    }
    public void gasConstantChanged(float val)
    {
        sphManager.gasConstant = val;
        gasConstantText.text = sphManager.gasConstant.ToString();
    }
    public void kernalRadiusChanged(float val)
    {
        sphManager.kernalRadius = val;
        kernalRadiusText.text = sphManager.kernalRadius.ToString();
    }
    public void massChanged(float val)
    {
        sphManager.mass = val;
        massText.text = sphManager.mass.ToString();
    }
    public void viscosityChanged(float val)
    {
        sphManager.viscosityCount = val;
        viscosityText.text = sphManager.viscosityCount.ToString();
    }
    public void timeStepChanged(float val)
    {
        sphManager.timeStep = val;
        timeStepText.text = sphManager.timeStep.ToString();
    }
    public void boundaryDampingChanged(float val)
    {
        sphManager.boundaryDamping = val;
        boundaryDampingText.text = sphManager.boundaryDamping.ToString();
    }
    public void collisionSphereRadiusChanged(float val)
    {
        sphManager.collisionSphereRadius = val;
        collisionSphereRadiusText.text = sphManager.collisionSphereRadius.ToString();
    }
    public void collisionMassChanged(float val)
    {
        sphManager.collisionMass = val;
        collisionMassText.text = sphManager.collisionMass.ToString();
    }
    public void ResetPressed()
    {
        simulationManager.ResetSystem();
    }
    public void PausePressed()
    {
        if (simulationManager.state == SimulationState.RUNNING)
            simulationManager.state = SimulationState.PAUSED; 
        else
            simulationManager.state = SimulationState.RUNNING; 
    }
    public void StopPressed()
    {
        simulationManager.StopSimulation();
    }

    public void OnScaleSliderChanged(float val)
    {
        particleDisplay.scale = val;
        scaleText.text = val.ToString();
    }

    public void OnMaxValueSliderChanged(float val)
    {
        particleDisplay.maxValue = val;
        maxValueText.text = val.ToString();
    }

    public void OnGradientResolutionChanged(float val)
    {
        particleDisplay.gradientResolution = (int)val;
        particleDisplay.ForceGradientUpdate();
        gradientResolutionText.text = ((int)val).ToString();
    }

    public void OnShadersDropDownChanged(int val)
    {
        particleDisplay.Reset();
        particleDisplay.SetNewShader(val);
        simulationManager.InitParticleDisplay();
        UpdateMaxValueSlider(val);
    }

    public void OnColorMapsChanged(int val)
    {
        particleDisplay.SetNewGrad(val);
        particleDisplay.ForceGradientUpdate();
    }

    public void UpdateMaxValueSlider(int val)
    {
        maxValueSlider.minValue = minShader.maxValues[val];
        maxValueSlider.maxValue = maxShader.maxValues[val];
        maxValueSlider.value = orignalShader.maxValues[val];
        maxValueText.text = orignalShader.maxValues[val].ToString();
    }

    public void resetToOrignalValues()
    {
        // simulation
        sphManager.gravity.x = orignal.gravity.x;
        gravityXInput.text = sphManager.gravity.x.ToString();
        sphManager.gravity.y = orignal.gravity.y;
        gravityYInput.text = sphManager.gravity.y.ToString();
        sphManager.gravity.z = orignal.gravity.z;
        gravityZInput.text = sphManager.gravity.z.ToString();

        sphManager.restDensity = orignal.restDensity;
        restDensitySlider.value = orignal.restDensity;
        restDensityText.text = sphManager.restDensity.ToString();
        sphManager.gasConstant = orignal.gasConstant;
        gasConstantSlider.value = orignal.gasConstant;
        gasConstantText.text = sphManager.gasConstant.ToString();
        sphManager.kernalRadius = orignal.kernalRadius;
        kernalRadiusSlider.value = orignal.kernalRadius;
        kernalRadiusText.text = sphManager.kernalRadius.ToString();
        sphManager.mass = orignal.mass;
        massSlider.value = orignal.mass;
        massText.text = sphManager.mass.ToString();
        sphManager.viscosityCount = orignal.viscosity;
        viscositySlider.value = orignal.viscosity;
        viscosityText.text = sphManager.viscosityCount.ToString();
        sphManager.timeStep = orignal.timeStep;
        timeStepSlider.value = orignal.timeStep;
        timeStepText.text = sphManager.timeStep.ToString();
        sphManager.boundaryDamping = orignal.boundaryDamping;
        boundaryDampingSlider.value = orignal.boundaryDamping;
        boundaryDampingText.text = sphManager.boundaryDamping.ToString();
        sphManager.collisionSphereRadius = orignal.collisionSphereRadius;
        collisionSphereRadiusSlider.value = orignal.collisionSphereRadius;
        collisionSphereRadiusText.text = sphManager.collisionSphereRadius.ToString();
        sphManager.collisionMass = orignal.collisionMass;
        collisionMassSlider.value = orignal.collisionMass;
        collisionMassText.text = sphManager.collisionMass.ToString();
    }

    public void ResetShaderValues()
    {
        shadersDropdown.value = 0;
        colorMapsDropdown.value = 0;
        scaleSlider.value = orignalShader.scale;
        scaleText.text = orignalShader.scale.ToString();
        maxValueSlider.value = orignalShader.maxValues[particleDisplay.currentShaderID];
        maxValueText.text = orignalShader.maxValues[particleDisplay.currentShaderID].ToString();
        gradientResolutionSlider.value = orignalShader.gradientResolution;
        gradientResolutionText.text = orignalShader.gradientResolution.ToString();
    }

    public void SetOrignalValues()
    {
        // simulation
        orignal.gravity.x = sphManager.gravity.x;
        orignal.gravity.y = sphManager.gravity.y;
        orignal.gravity.z = sphManager.gravity.z;
        orignal.restDensity = sphManager.restDensity;
        orignal.gasConstant = sphManager.gasConstant;
        orignal.kernalRadius = sphManager.kernalRadius;
        orignal.mass = sphManager.mass;
        orignal.viscosity = sphManager.viscosityCount;
        orignal.timeStep = sphManager.timeStep;
        orignal.boundaryDamping = sphManager.boundaryDamping;
        orignal.collisionSphereRadius = sphManager.collisionSphereRadius;
        orignal.collisionMass = sphManager.collisionMass;

        // shader
        orignalShader.scale = particleDisplay.scale;
        orignalShader.gradientResolution = particleDisplay.gradientResolution;
    }
}
