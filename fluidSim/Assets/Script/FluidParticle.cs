
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class FluidParticle : MonoBehaviour
{
    public Rigidbody2D rb;
    public Vector2 velocity;
    public float density;
    public float presure;

    [SerializeField]private FluidParticle[] neighbours;

    public void Initialise(Vector3 position, Vector2 velocity)
    {
        transform.position = position;
        rb.velocity = velocity;
        this.density = 0f;
        this.presure = 0f;
    }

    public void FindNeighbors(float radius, LayerMask layer)
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position, radius, layer);
        neighbours = colliders
            .Select(collider => collider.GetComponent<FluidParticle>())
            .Where(particle => particle != null)
            .ToArray();
    }

    public FluidParticle[] GetNeighbours() { return neighbours; }

}
