using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;

public class ComputeFluidParticle
{
    public ComputeFluidParticle(float3 pos)
    {
        this.pos = pos;
        velocity = float3.zero;
        force = float3.zero;
        density = 0;
        pressure = 0;
    }

    public float3 pos;
    public float3 velocity;
    public float3 force;
    public float density;
    public float pressure;
}

public class FluidParticle : MonoBehaviour
{
    public FluidParticle(Vector2 pos)
    {
        this.pos = pos;
        velocity = Vector2.zero;
        force = Vector2.zero;
        density = 0;
        pressure = 0;
    }

    public Vector2 pos;
    public Vector2 velocity;
    public Vector2 force;
    public float density;
    public float pressure;
}
