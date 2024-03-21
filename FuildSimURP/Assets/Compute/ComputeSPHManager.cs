using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

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
    public List<FluidParticle> particles = new();
    public int initialParticles;

    [Header("Shader Things")]
    public ComputeShader compute;
    public ComputeBuffer positionBuffer;
    public ComputeBuffer velocityBuffer;
    public ComputeBuffer forceBuffer;
    public ComputeBuffer densityBuffer;
    public ComputeBuffer pressureBuffer;

    const int ComputeDensityPressureKernel = 0;
    const int ComputeForcesKernel = 1;
    const int IntergrateKernel = 2;

    void Start()
    {
        positionBuffer = new ComputeBuffer(numParticles, System.Runtime.InteropServices.Marshal.SizeOf(typeof(float2)));
        velocityBuffer = new ComputeBuffer(numParticles, System.Runtime.InteropServices.Marshal.SizeOf(typeof(float2)));
        forceBuffer = new ComputeBuffer(numParticles, System.Runtime.InteropServices.Marshal.SizeOf(typeof(float2)));
        densityBuffer = new ComputeBuffer(numParticles, System.Runtime.InteropServices.Marshal.SizeOf(typeof(float)));
        pressureBuffer = new ComputeBuffer(numParticles, System.Runtime.InteropServices.Marshal.SizeOf(typeof(float)));

        SetBufferData();

        // Compute Density Pressure Kernel
        compute.SetBuffer(ComputeDensityPressureKernel, "positions", positionBuffer);
        compute.SetBuffer(ComputeDensityPressureKernel, "densities", densityBuffer);
        compute.SetBuffer(ComputeDensityPressureKernel, "pressures", pressureBuffer);

        // Compute Forces Kernel
        compute.SetBuffer(ComputeForcesKernel, "positions", positionBuffer);
        compute.SetBuffer(ComputeForcesKernel, "densities", densityBuffer);
        compute.SetBuffer(ComputeForcesKernel, "pressures", pressureBuffer);
        compute.SetBuffer(ComputeForcesKernel, "velocities", velocityBuffer);
        compute.SetBuffer(ComputeForcesKernel, "forces", forceBuffer);

        // Intergrate Kernel
        compute.SetBuffer(IntergrateKernel, "positions", positionBuffer);
        compute.SetBuffer(IntergrateKernel, "densities", densityBuffer);
        compute.SetBuffer(IntergrateKernel, "velocities", velocityBuffer);
        compute.SetBuffer(IntergrateKernel, "forces", forceBuffer);

        // Particle Number
        compute.SetInt("numParticles", numParticles);
    }

    void SetBufferData()
    {
        List<float2> points = new();
        List<float2> velocities = new();
        List<float2> forces = new();
        List<float2> densities = new();
        List<float2> pressures = new();
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
        float2[] allDensities = densities.ToArray();
        float2[] allPressures = pressures.ToArray();
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
                    FluidParticle particle = new FluidParticle(new Vector2(x + jitter, y));
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
        dispatchKernal(ComputeDensityPressureKernel);
        dispatchKernal(ComputeForcesKernel);
        dispatchKernal(IntergrateKernel);
    }

    public void dispatchKernal(int kernal)
    {
        uint x, y, z;
        compute.GetKernelThreadGroupSizes(kernal, out x, out y, out z);
        var threadGroupSizes = new Vector3Int((int)x, (int)y, (int)z);
        int numGroupsX = Mathf.CeilToInt(numParticles / (float)threadGroupSizes.x);
        int numGroupsY = Mathf.CeilToInt(numParticles / (float)threadGroupSizes.y);
        int numGroupsZ = Mathf.CeilToInt(numParticles / (float)threadGroupSizes.y);
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
    }

    private void OnDestroy()
    {
        positionBuffer.Release();
        velocityBuffer.Release();
        forceBuffer.Release();
        densityBuffer.Release();
        pressureBuffer.Release();
    }
}