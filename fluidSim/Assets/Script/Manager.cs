using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class Manager : MonoBehaviour
{
    // https://lucasschuermann.com/writing/implementing-sph-in-2d

    [Header("Simulation Values")]
    public Vector2 gravity = new(0f, -10f);
    public float restDensity = 300f;
    public float gasConstant = 2000f;
    public float kernalRadius = 16f;
    private float squareRadius
    {
        get { return kernalRadius * kernalRadius; }
    }
    public float mass = 2.5f;
    public float viscosityConst = 200f;
    public float timeStep = 0.001f;

    private float epsilon
    {
        get { return kernalRadius; }
    }
    public float boundaryDamping = -0.5f;

    [Header("Interaction")]
    public float interactionInputStrength;
    public float interactionRadius;

    // smoothing kernels defined in M�ller and their gradients
    // adapted to 2D per "SPH Based Shallow Water Simulation" by Solenthaler et al.
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


    [Header("Interaction")]
    public int maxParticles = 2500;
    public int damParticles = 500;
    public int blockParticles = 250;

    [Header("Spawning")]
    public Vector2 view;
    public List<FluidParticle> particles = new();
    public GameObject baseParticle;


    public void Start()
    {
        spawnParticles();
    }

    public void spawnParticles()
    {
        for (float y = epsilon; y < view.y - epsilon * 2f; y += kernalRadius)
        {
            for (float x = view.x / 4; x <= view.x / 2; x += kernalRadius)
            {
                if (particles.Count < damParticles)
                {
                    float jitter = UnityEngine.Random.value / 1;
                    FluidParticle particle = Instantiate(baseParticle, new Vector2(x + jitter, y), Quaternion.identity).GetComponent<FluidParticle>();
                    particle.Init(new Vector2(x + jitter, y));
                    particles.Add(particle);
                }
                else
                    return;
            }
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawLine(new Vector3(0, 0, 0), new Vector3(view.x, 0, 0));
        Gizmos.DrawLine(new Vector3(0, view.y, 0), new Vector3(view.x, view.y, 0));
        Gizmos.DrawLine(new Vector3(0, 0, 0), new Vector3(0, view.y, 0));
        Gizmos.DrawLine(new Vector3(view.x, 0, 0), new Vector3(view.x, view.y, 0));
    }

    public void Update()
    {
        ComputeDensityPressure();
        ComputeForces();
        Integrate();

        foreach (FluidParticle particle in particles)
        {
            particle.transform.position = particle.pos;
        }


        if (Input.GetKeyDown(KeyCode.R))
        {
            foreach (FluidParticle particle in particles)
            {
                Destroy(particle.gameObject);
            }
            particles.Clear();
            spawnParticles();
        }
    }

    public void ComputeDensityPressure()
    {
        foreach (FluidParticle particle in particles)
        {
            particle.density = 0f;
            foreach (FluidParticle particle2 in particles)
            {
                Vector2 dir = particle2.pos - particle.pos;
                float dist = dir.sqrMagnitude;
                if (dist < squareRadius)
                    particle.density += mass * POLY6 * Mathf.Pow(squareRadius - dist, 3f);
            }
            particle.pressure = gasConstant * (particle.density - restDensity);
        }
    }

    public void ComputeForces()
    {
        foreach (FluidParticle particle in particles)
        {
            Vector2 forcePressure = Vector2.zero;
            Vector2 forceViscosity = Vector2.zero;
            foreach (FluidParticle particle2 in particles)
            {
                if (particle == particle2)
                    continue;

                Vector2 dir = particle2.pos - particle.pos;
                float dirMag = dir.magnitude;

                if (dirMag < kernalRadius)
                {
                    forcePressure += -dir.normalized * mass * (particle.pressure + particle2.pressure) / (2 * particle2.density) *
                        SPIKYGRAD * Mathf.Pow(kernalRadius - dirMag, 3);
                    forceViscosity += viscosityConst * mass * (particle2.velocity - particle.velocity) / particle2.density *
                        VISCLAP * (kernalRadius - dirMag);
                }
            }
            Vector2 forceGravity = gravity * mass / particle.density;

            if (Input.GetMouseButton(0))
            {
                Vector2 inputPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                float sqrDst = Vector2.Dot(inputPoint, particle.pos);
                if (sqrDst < interactionRadius * interactionRadius)
                {
                    float dst = Mathf.Sqrt(sqrDst);
                    float edgeT = (dst / kernalRadius);
                    float centreT = 1 - edgeT;
                    Vector2 dirToCentre = inputPoint / dst;

                    float gravityWeight = 1 - (centreT * Mathf.Clamp01(interactionInputStrength / 10));
                    Vector2 accel = gravity * gravityWeight + dirToCentre * centreT * interactionInputStrength;
                    accel -= particle.velocity * centreT;
                    forceGravity = accel;
                }
            }

            particle.force = forcePressure + forceViscosity + forceGravity;
        }
    }

    public void Integrate()
    {
        foreach (FluidParticle particle in particles)
        {
            particle.velocity += timeStep * particle.force / particle.density;
            particle.pos += timeStep * particle.velocity;

            if (particle.pos.x - epsilon < 0f)
            {
                particle.velocity.x *= boundaryDamping;
                particle.pos.x = epsilon;
            }
            if (particle.pos.x + epsilon > view.x)
            {
                particle.velocity.x *= boundaryDamping;
                particle.pos.x = view.x - epsilon;
            }

            if (particle.pos.y - epsilon < 0f)
            {
                particle.velocity.y *= boundaryDamping;
                particle.pos.y = epsilon;
            }
            if (particle.pos.y + epsilon > view.y)
            {
                particle.velocity.y *= boundaryDamping;
                particle.pos.y = view.y - epsilon;
            }
        }
    }
}
