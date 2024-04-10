using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;

/// <summary>
/// base fluid particle, that holds all of the data relating to the particle
///  -- dont think this is actually used anymore so more of a relic --
/// </summary>
public class ComputeFluidParticle
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ComputeFluidParticle"/> class.
    /// </summary>
    /// <param name="pos">The position.</param>
    public ComputeFluidParticle(float3 pos)
    {
        this.pos = pos;
        velocity = float3.zero;
        force = float3.zero;
        density = 0;
        pressure = 0;
    }

    // fluid data
    public float3 pos;
    public float3 velocity;
    public float3 force;
    public float density;
    public float pressure;
}
