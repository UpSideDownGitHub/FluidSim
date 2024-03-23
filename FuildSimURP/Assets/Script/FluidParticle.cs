using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ComputeFluidParticle
{
    public ComputeFluidParticle(Vector2 pos)
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
