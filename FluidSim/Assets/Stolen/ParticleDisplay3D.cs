using UnityEngine;

public class ParticleDisplay3D : MonoBehaviour
{

    public Shader shader;
    public float scale;
    Mesh mesh;
    public Color col;
    Material mat;

    ComputeBuffer argsBuffer;
    Bounds bounds;

    public Gradient colourMap;
    public int gradientResolution;
    public float velocityDisplayMax;
    Texture2D gradientTexture;
    bool needsUpdate;

    public int meshResolution;
    public int debug_MeshTriCount;

    public void Reset()
    {
        argsBuffer.Release();
        needsUpdate = true;
    }

    public void Init(ComputeSPHManager sim)
    {
        mat = new Material(shader);
        mat.SetBuffer("Positions", sim.positionBuffer);
        mat.SetBuffer("Velocities", sim.velocityBuffer);

        mesh = SphereGenerator.GenerateSphereMesh(meshResolution);
        debug_MeshTriCount = mesh.triangles.Length / 3;
        const int subMeshIndex = 0;
        uint[] args = new uint[5];
        args[0] = (uint)mesh.GetIndexCount(subMeshIndex);
        args[1] = (uint)sim.positionBuffer.count;
        args[2] = (uint)mesh.GetIndexStart(subMeshIndex);
        args[3] = (uint)mesh.GetBaseVertex(subMeshIndex);
        args[4] = 0; // offset
        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
        bounds = new Bounds(Vector3.zero, Vector3.one * 10000);
    }

    public void UpdateDisplay()
    {
        UpdateSettings();
        Graphics.DrawMeshInstancedIndirect(mesh, 0, mat, bounds, argsBuffer);
    }

    void UpdateSettings()
    {
        if (needsUpdate)
        {
            needsUpdate = false;
            TextureFromGradient(ref gradientTexture, gradientResolution, colourMap);
            mat.SetTexture("ColourMap", gradientTexture);
        }
        mat.SetFloat("scale", scale);
        mat.SetColor("colour", col);
        mat.SetFloat("velocityMax", velocityDisplayMax);

        Vector3 s = transform.localScale;
        transform.localScale = Vector3.one;
        var localToWorld = transform.localToWorldMatrix;
        transform.localScale = s;

        mat.SetMatrix("localToWorld", localToWorld);
    }

    public void TextureFromGradient(ref Texture2D texture, int width, Gradient gradient, FilterMode filterMode = FilterMode.Bilinear)
    {
        if (texture == null)
        {
            texture = new Texture2D(width, 1);
        }
        else if (texture.width != width)
        {
            texture.Reinitialize(width, 1);
        }
        if (gradient == null)
        {
            gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.black, 0), new GradientColorKey(Color.black, 1) },
                new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
            );
        }
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = filterMode;

        Color[] cols = new Color[width];
        for (int i = 0; i < cols.Length; i++)
        {
            float t = i / (cols.Length - 1f);
            cols[i] = gradient.Evaluate(t);
        }
        texture.SetPixels(cols);
        texture.Apply();
    }

    private void OnValidate()
    {
        needsUpdate = true;
    }

    void OnDestroy()
    {
        argsBuffer.Release();
    }
}
