using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;

public class ComputeSPHManager : MonoBehaviour
{
    [Header("Simulation Values")]
    public Vector2 gravity = new(0f, -10f);
    public float restDensity = 300f;
    public float gasConstant = 2000f;
    public float kernalRadius = 16f;
    public float mass = 2.5f;
    public float viscosityCount = 200f;
    public float timeStep = 0.001f;
    public float boundaryDamping = -0.5f;

    [Header("Interaction")]
    public float interactionRadius;
    public float interaction1;
    public float interaction2;
    public Vector2 interactionPoint;

    [Header("Particles")]
    public int numParticles;
    public Vector2 view;
    //public List<FluidParticle> particles = new();
    public List<ComputeFluidParticle> particles = new();
    public int initialParticles;

    [Header("Shader Things")]
    public ComputeShader compute;
    public ComputeBuffer positionBuffer;
    public ComputeBuffer velocityBuffer;
    public ComputeBuffer forceBuffer;
    public ComputeBuffer densityBuffer;
    public ComputeBuffer pressureBuffer;

    const int ComputeDensityKernel = 0;
    const int ComputeExternalKernel = 1;
    const int ComputePressureKernel = 2;
    const int ComputeViscosityKernel = 3;
    const int IntergrateKernel = 4;

    [Header("Display")]
    public ParticleDisplay2D particleDisplay;


    void Start()
    {
        SpawnParticles();

        positionBuffer = new ComputeBuffer(numParticles, System.Runtime.InteropServices.Marshal.SizeOf(typeof(float2)));
        velocityBuffer = new ComputeBuffer(numParticles, System.Runtime.InteropServices.Marshal.SizeOf(typeof(float2)));
        forceBuffer = new ComputeBuffer(numParticles, System.Runtime.InteropServices.Marshal.SizeOf(typeof(float2)));
        densityBuffer = new ComputeBuffer(numParticles, System.Runtime.InteropServices.Marshal.SizeOf(typeof(float)));
        pressureBuffer = new ComputeBuffer(numParticles, System.Runtime.InteropServices.Marshal.SizeOf(typeof(float)));

        SetBufferData();

        // Compute Density Pressure Kernel
        compute.SetBuffer(ComputeDensityKernel, "positions", positionBuffer);
        compute.SetBuffer(ComputeDensityKernel, "densities", densityBuffer);

        // Compute External Kernel
        compute.SetBuffer(ComputeExternalKernel, "positions", positionBuffer);
        compute.SetBuffer(ComputeExternalKernel, "densities", densityBuffer);
        compute.SetBuffer(ComputeExternalKernel, "forces", forceBuffer);

        // Compute Pressure Kernel
        compute.SetBuffer(ComputePressureKernel, "positions", positionBuffer);
        compute.SetBuffer(ComputePressureKernel, "densities", densityBuffer);
        compute.SetBuffer(ComputePressureKernel, "forces", forceBuffer);
        compute.SetBuffer(ComputePressureKernel, "pressures", pressureBuffer);

        // Compute Viscosity Kernel
        compute.SetBuffer(ComputeViscosityKernel, "positions", positionBuffer);
        compute.SetBuffer(ComputeViscosityKernel, "densities", densityBuffer);
        compute.SetBuffer(ComputeViscosityKernel, "velocities", velocityBuffer);
        compute.SetBuffer(ComputeViscosityKernel, "forces", forceBuffer);

        // Intergrate Kernel
        compute.SetBuffer(IntergrateKernel, "positions", positionBuffer);
        compute.SetBuffer(IntergrateKernel, "densities", densityBuffer);
        compute.SetBuffer(IntergrateKernel, "velocities", velocityBuffer);
        compute.SetBuffer(IntergrateKernel, "forces", forceBuffer);

        // Particle Number
        compute.SetInt("numParticles", numParticles);

        // Drawing
        particleDisplay.Init(this);
    }

    void SetBufferData()
    {
        List<float2> points = new();
        List<float2> velocities = new();
        List<float2> forces = new();
        List<float> densities = new();
        List<float> pressures = new();
        for (int i = 0; i < particles.Count; i++)
        {
            points.Add(particles[i].pos);
            velocities.Add(particles[i].velocity);
            forces.Add(particles[i].force);
            densities.Add(particles[i].density);
            pressures.Add(particles[i].pressure);
        }
        float2[] allPoints = points.ToArray();
        float2[] allVelocities = velocities.ToArray();
        float2[] allForces = forces.ToArray();
        float[] allDensities = densities.ToArray();
        float[] allPressures = pressures.ToArray();
        positionBuffer.SetData(allPoints);
        velocityBuffer.SetData(allVelocities);
        forceBuffer.SetData(allForces);
        densityBuffer.SetData(allDensities);
        pressureBuffer.SetData(allPressures);
    }

    public void SpawnParticles()
    {
        for (float y = kernalRadius; y < view.y - kernalRadius * 2f; y += kernalRadius)
        {
            for (float x = view.x / 4; x <= view.x / 2; x += kernalRadius)
            {
                if (particles.Count < initialParticles)
                {
                    float jitter = UnityEngine.Random.value / 1;
                    ComputeFluidParticle particle = new ComputeFluidParticle(new Vector2(x + jitter, y));
                    particles.Add(particle);
                    numParticles++;
                }
                else
                    return;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSettings();
        Run();
    }

    public void Run()
    {
        // density
        //  ¬ Set density value
        // external
        //  ¬ add external forces to velocity
        // pressure
        //  ¬ calculate pressure force & add to velo
        // viscosity
        //  ¬ calculate visocity force & add to velo
        // update positions
        //  ¬ update positions based on velo
        dispatchKernal(ComputeDensityKernel);
        dispatchKernal(ComputeExternalKernel);
        dispatchKernal(ComputePressureKernel);
        dispatchKernal(ComputeViscosityKernel);
        dispatchKernal(IntergrateKernel);
    }

    public void dispatchKernal(int kernal)
    {
        uint x, y, z;
        compute.GetKernelThreadGroupSizes(kernal, out x, out y, out z);
        Vector3Int threadGroupSizes = new Vector3Int((int)x, (int)y, (int)z);
        int numGroupsX = Mathf.CeilToInt(numParticles / (float)threadGroupSizes.x);
        int numGroupsY = Mathf.CeilToInt(1 / (float)threadGroupSizes.y);
        int numGroupsZ = Mathf.CeilToInt(1 / (float)threadGroupSizes.y);
        compute.Dispatch(kernal, numGroupsX, numGroupsY, numGroupsZ);
    }

    public void UpdateSettings()
    {
        compute.SetVector("gravity", gravity);
        compute.SetFloat("restDensity", restDensity);
        compute.SetFloat("gasConstant", gasConstant);
        compute.SetFloat("kernalRadius", kernalRadius);
        compute.SetFloat("mass", mass);
        compute.SetFloat("viscosityConst", viscosityCount);
        compute.SetFloat("timeStep", timeStep);
        compute.SetFloat("boundaryDamping", boundaryDamping);
        compute.SetVector("view", view);
        compute.SetFloat("interactionRadius", interactionRadius);
        compute.SetFloat("interaction1", Input.GetMouseButton(0) ? 1 : 0);
        compute.SetFloat("interaction2", Input.GetMouseButton(1) ? 1 : 0);
        compute.SetVector("interactionPoint", Camera.main.ScreenToWorldPoint(Input.mousePosition));
        compute.SetFloat("POLY6", 4.0f / (Mathf.PI * Mathf.Pow(kernalRadius, 8.0f)));
        compute.SetFloat("SPIKYGRAD", -10.0f / (Mathf.PI * Mathf.Pow(kernalRadius, 5.0f)));
        compute.SetFloat("VISCLAP", 40.0f / (Mathf.PI * Mathf.Pow(kernalRadius, 5.0f)));
    }

    private void OnDestroy()
    {
        positionBuffer.Release();
        velocityBuffer.Release();
        forceBuffer.Release();
        densityBuffer.Release();
        pressureBuffer.Release();
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(0, 0, 0), new Vector3(view.x, 0, 0));
        Gizmos.DrawLine(new Vector3(0, view.y, 0), new Vector3(view.x, view.y, 0));
        Gizmos.DrawLine(new Vector3(0, 0, 0), new Vector3(0, view.y, 0));
        Gizmos.DrawLine(new Vector3(view.x, 0, 0), new Vector3(view.x, view.y, 0));
    }
}