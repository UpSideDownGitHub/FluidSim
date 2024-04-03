using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
public class ComputeSPHManager : MonoBehaviour
{
    public bool justSpawn;
    public bool showValues;

    [Header("Collision Balls")]
    public Balls balls;

    [Header("Simulation Values")]
    public Vector3 gravity = new(0f, -10f, 0f);
    public float restDensity = 300f;
    public float gasConstant = 2000f;
    public float kernalRadius = 16f;
    public float mass = 2.5f;
    public float viscosityCount = 200f;
    public float timeStep = 0.001f;
    public float boundaryDamping = -0.5f;
    public float collisionSphereRadius;
    public float collisionMass;

    [Header("Particles")]
    public int numParticles;
    public Vector3 view;
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
    public ComputeBuffer collisionSphereBuffer;

    const int ComputeDensityKernel = 0;
    const int ComputeExternalKernel = 1;
    const int ComputePressureKernel = 2;
    const int ComputeViscosityKernel = 3;
    const int IntergrateKernel = 4;
    const int CollisionKernel = 5;

    [Header("Display")]
    public ParticleDisplay3D particleDisplay;

    // FOR DEBUGGING THESE WILL SHOW DATA IN INSPECTOR
    public List<Vector3> particlePositions;
    public List<Vector3> particleVelocity;
    public List<Vector3> particleForces;
    public List<float> particleDensity;
    public List<float> particlePressure;

    public float POLY6
    {
        get { return 4f / (Mathf.PI * Mathf.Pow(kernalRadius, 8f)); }
    }
    public float SPIKYGRAD
    {
        get { return -10f / (Mathf.PI * Mathf.Pow(kernalRadius, 5f)); }
    }
    public float VISCLAP
    {
        get { return 40f / (Mathf.PI * Mathf.Pow(kernalRadius, 5f)); }
    }
    public void LoadSavedBallData(string fileName)
    {
        string path = Application.persistentDataPath + "/" + fileName;
        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string json = reader.ReadToEnd();
                balls = JsonUtility.FromJson<Balls>(json);
            }
        }
    }

    public void DestroyCurrent()
    {
        positionBuffer.Release();
        velocityBuffer.Release();
        forceBuffer.Release();
        densityBuffer.Release();
        pressureBuffer.Release();
        collisionSphereBuffer.Release();
        particles.Clear();
        numParticles = 0;
        particleDisplay.Reset();
    }

    public void StartSimulation(string collisionDataName)
    {
        LoadSavedBallData(collisionDataName);
        SpawnParticles();

        positionBuffer = new ComputeBuffer(numParticles, System.Runtime.InteropServices.Marshal.SizeOf(typeof(float3)));
        velocityBuffer = new ComputeBuffer(numParticles, System.Runtime.InteropServices.Marshal.SizeOf(typeof(float3)));
        forceBuffer = new ComputeBuffer(numParticles, System.Runtime.InteropServices.Marshal.SizeOf(typeof(float3)));
        densityBuffer = new ComputeBuffer(numParticles, System.Runtime.InteropServices.Marshal.SizeOf(typeof(float)));
        pressureBuffer = new ComputeBuffer(numParticles, System.Runtime.InteropServices.Marshal.SizeOf(typeof(float)));
        collisionSphereBuffer = new ComputeBuffer(balls.balls.Count, System.Runtime.InteropServices.Marshal.SizeOf(typeof(float3)));

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

        // Collision Kernel
        compute.SetBuffer(CollisionKernel, "positions", positionBuffer);
        compute.SetBuffer(CollisionKernel, "collisionSpheres", collisionSphereBuffer);
        compute.SetBuffer(CollisionKernel, "velocities", velocityBuffer);
        compute.SetBuffer(CollisionKernel, "densities", densityBuffer);

        // Particle Number
        compute.SetInt("numParticles", numParticles);

        // Collision Number
        compute.SetInt("collisionSphereCount", balls.balls.Count);

        // Drawing
        particleDisplay.Init(this);
    }

    void SetBufferData()
    {
        List<float3> points = new();
        List<float3> velocities = new();
        List<float3> forces = new();
        List<float> densities = new();
        List<float> pressures = new();
        List<float3> collisionSpheres = new();
        for (int i = 0; i < particles.Count; i++)
        {
            points.Add(particles[i].pos);
            velocities.Add(particles[i].velocity);
            forces.Add(particles[i].force);
            densities.Add(particles[i].density);
            pressures.Add(particles[i].pressure);
        }
        for (int i = 0; i < balls.balls.Count; i++)
        {
            collisionSpheres.Add(balls.balls[i]);
        }
        float3[] allPoints = points.ToArray();
        float3[] allVelocities = velocities.ToArray();
        float3[] allForces = forces.ToArray();
        float[] allDensities = densities.ToArray();
        float[] allPressures = pressures.ToArray();
        float3[] allCollisionSpheres = collisionSpheres.ToArray();
        positionBuffer.SetData(allPoints);
        velocityBuffer.SetData(allVelocities);
        forceBuffer.SetData(allForces);
        densityBuffer.SetData(allDensities);
        pressureBuffer.SetData(allPressures);
        collisionSphereBuffer.SetData(allCollisionSpheres);
    }

    public void SpawnParticles()
    {
        for (float y = kernalRadius; y < view.y - kernalRadius * 2f; y += kernalRadius)
        {
            for (float x = view.x / 10; x <= (view.x * 9) / 10; x += kernalRadius)
            {
                for (float z = view.z / 10; z <= (view.z * 9) / 10; z += kernalRadius)
                {
                    if (particles.Count < initialParticles || justSpawn)
                    {
                        float jitter = UnityEngine.Random.value / 1;
                        ComputeFluidParticle particle = new ComputeFluidParticle(new float3(x + jitter, view.y - y, z + jitter));
                        particles.Add(particle);
                        numParticles++;
                    }
                    else
                        return;
                }
            }
        }
    }

    // Update is called once per frame
    public void UpdateSimulation()
    {
        UpdateSettings();
        Run();
        if (showValues)
            ShowValues();
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
        dispatchKernal(CollisionKernel);
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
        compute.SetFloat("timeStep", timeStep * Time.deltaTime);
        compute.SetFloat("boundaryDamping", boundaryDamping);
        compute.SetVector("view", view);
        
        compute.SetFloat("POLY6", POLY6);
        compute.SetFloat("SPIKYGRAD", SPIKYGRAD);
        compute.SetFloat("VISCLAP", VISCLAP);

        compute.SetFloat("collisionSphereRadius", collisionSphereRadius);
        compute.SetFloat("collisionMass", collisionMass);
    } 

    private void OnDestroy()
    {
        positionBuffer.Release();
        velocityBuffer.Release();
        forceBuffer.Release();
        densityBuffer.Release();
        pressureBuffer.Release();
        collisionSphereBuffer.Release();
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        var center = new Vector3(view.x / 2, view.y / 2, view.z / 2);
        Gizmos.DrawWireCube(center, view);
    }

    public void ShowValues()
    {
        particlePositions.Clear();
        float3[] positions = new float3[particles.Count];
        positionBuffer.GetData(positions);
        for (int i = 0; i < positions.Length; i++)
        {
            particlePositions.Add(new Vector3(positions[i].x, positions[i].y, positions[i].z));
        }

        particleVelocity.Clear();
        float3[] velocity = new float3[particles.Count];
        velocityBuffer.GetData(velocity);
        for (int i = 0; i < velocity.Length; i++)
        {
            particleVelocity.Add(new Vector3(velocity[i].x, velocity[i].y, velocity[i].z));
        }

        particleForces.Clear();
        float3[] forces = new float3[particles.Count];
        forceBuffer.GetData(forces);
        for (int i = 0; i < forces.Length; i++)
        {
            particleForces.Add(new Vector3(forces[i].x, forces[i].y, forces[i].z));
        }

        particleDensity.Clear();
        float[] densities = new float[particles.Count];
        densityBuffer.GetData(densities);
        for (int i = 0; i < densities.Length; i++)
        {
            particleDensity.Add(densities[i]);
        }

        particlePressure.Clear();
        float[] pressures = new float[particles.Count];
        pressureBuffer.GetData(pressures);
        for (int i = 0; i < pressures.Length; i++)
        {
            particlePressure.Add(pressures[i]);
        }
    }
}