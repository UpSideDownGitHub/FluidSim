using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class Manager : MonoBehaviour
{
    [Header("Simulation Values")]
    public LayerMask searchLayer;
    public float neighbourSearchRadius = 2f; // Example: 2f (can be adjusted based on particle density)
    public float particleMass = 0.1f; // Example: 0.1f (mass of each particle)
    public float smoothingLength = 1.5f; // Example: 1.5f (smoothing length)
    public float gasConstant = 100f; // Example: 100f (stiffness of the fluid)
    public float restDensity = 1.0f; // Example: 1.0f (rest density of the fluid)
    public float velocityDifference = 0.5f; // Example: 0.5f (initial velocity difference)
    public float viscosityCoefficient = 15f; // Example: 15f (viscosity coefficient)

    [Header("General")]
    public List<FluidParticle> particles = new();
    public float timeStep = 1f;

    [Header("Initilisation")]
    public GameObject particleObject;
    public Vector3 spawnPos;
    public int totalStartingParticles;
    public float spawnTime = 0.1f;

    // Private Variables
    private float _nextUpdate;

    public void Start()
    {
        Physics2D.IgnoreLayerCollision(6, 6, true);
        StartCoroutine(InitaliseParticles(totalStartingParticles));
        _nextUpdate = 0;
    }

    public void Update()
    {
        if (Time.time > _nextUpdate)
        {
            _nextUpdate = Time.time + timeStep;
            UpdateParticles();
        }
    }

    public IEnumerator InitaliseParticles(int count)
    {
        // spawn particles
        for (int i = 0; i < count; i++)
        {
            particles.Add(Instantiate(particleObject, spawnPos, 
                Quaternion.identity).GetComponent<FluidParticle>());
            yield return new WaitForSeconds(spawnTime);
        }
    }

    public void UpdateParticles()
    {
        foreach (FluidParticle particle in particles)
        {
            particle.FindNeighbors(neighbourSearchRadius, searchLayer);
            CalculateDensity(particle);
            CalculatePressure(particle);
        }
        foreach (FluidParticle particle in particles)
        {
            Vector2 pressureForce = CaluclatePressureForce(particle);
            Vector2 viscosityForce = CalculateViscosityForce(particle);

            particle.rb.velocity += pressureForce + viscosityForce;
        }
    }

    public void CalculateDensity(FluidParticle particle)
    {
        float density = 0;
        foreach (FluidParticle neighbour in particle.GetNeighbours())
        {
            var dist = Vector2.Distance(particle.transform.position, neighbour.transform.position);
            density += particleMass * Kernel(dist, smoothingLength);
        }
        particle.density = density;
    }

    public void CalculatePressure(FluidParticle particle)
    {
        particle.presure = gasConstant * (particle.density - restDensity);
    }

    public Vector2 CaluclatePressureForce(FluidParticle particle)
    {
        Vector2 pressureForce = Vector2.zero;
        foreach (FluidParticle neighbour in particle.GetNeighbours())
        {
            Vector2 dir = neighbour.transform.position - particle.transform.position;
            float distSquared = Vector2.SqrMagnitude(dir); // Calculate squared distance to avoid square root
            float dist = Mathf.Sqrt(distSquared); // Calculate distance
            if (dist > 0) // Ensure distance is non-zero to avoid division by zero
            {
                Vector2 normalizedDir = dir / dist; // Normalize direction
                float pressureContribution = -particleMass * (particle.presure + neighbour.presure) /
                                             (2 * neighbour.density);
                Vector2 gradient = KernelGradient(dist, smoothingLength, normalizedDir); // Calculate gradient
                pressureForce += gradient * pressureContribution; // Accumulate force
            }
        }
        return pressureForce;
    }

    public Vector2 CalculateViscosityForce(FluidParticle particle)
    {
        Vector2 viscosityForce = Vector2.zero;
        foreach (FluidParticle neighbour in particle.GetNeighbours())
        {
            Vector2 velocityDifference = neighbour.velocity - particle.velocity;
            Vector2 dir = neighbour.transform.position - particle.transform.position;
            float distSquared = Vector2.SqrMagnitude(dir); // Calculate squared distance to avoid square root
            float dist = Mathf.Sqrt(distSquared); // Calculate distance
            if (dist > 0) // Ensure distance is non-zero to avoid division by zero
            {
                Vector2 normalizedDir = dir / dist; // Normalize direction
                float viscosityContribution = viscosityCoefficient * particleMass / neighbour.density;
                float laplacian = KernelLaplacian(dist, smoothingLength); // Calculate Laplacian
                viscosityForce += velocityDifference * (viscosityContribution * laplacian); // Accumulate force
            }
        }
        return viscosityForce;
    }

    public float Kernel(float distance, float smoothingLength)
    {
        var q = distance / smoothingLength;
        if (q <= 1)
            return (1 / (Mathf.PI * Mathf.Pow(smoothingLength, 3))) * 
                (1 - 1.5f * Mathf.Pow(q, 2) + 0.75f * Mathf.Pow(q, 3));
        return 0f;
    }

    public Vector2 KernelGradient(float distance, float smoothingLength, Vector2 dir)
    {
        var q = distance / smoothingLength;
        if (q <= 1)
        {
            if (distance < Mathf.Epsilon) 
                return Vector2.zero; 
            else
                return (1 / (Mathf.PI * Mathf.Pow(smoothingLength, 4))) *
                       (-3 * q + 2.25f * Mathf.Pow(q, 2)) *
                       (1 / distance) * dir;
        }
        return Vector2.zero;
    }

    public float KernelLaplacian(float distance, float smoothingLength)
    {
        var q = distance / smoothingLength;
        if (q <= 1)
            return (1 / (Mathf.PI * Mathf.Pow(smoothingLength, 5))) *
                (-3 * q + 2.25f * q) *
                (1 / Mathf.Pow(smoothingLength, 2));
        return 0f;
    }
}
