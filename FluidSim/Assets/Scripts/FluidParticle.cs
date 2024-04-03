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
