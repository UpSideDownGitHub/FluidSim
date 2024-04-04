using UnityEngine;

public class ParticleDisplay3D : MonoBehaviour
{
    public Shader shader;
    public float scale;
    public Mesh mesh;
    public Color col;
    public Gradient colourMap;
    public int gradientResolution;
    public float velocityDisplayMax;

    // private
    private Material _mat;
    private ComputeBuffer _buffer;
    private Bounds _bounds;
    private Texture2D _gradientTexture;
    private bool _updateGradient;

    public void Reset()
    {
        _buffer.Release();
        _updateGradient = true;
    }

    public void Init(ComputeSPHManager sim)
    {
        _updateGradient = true;
        _mat = new Material(shader);
        _mat.SetBuffer("Positions", sim.positionBuffer);
        _mat.SetBuffer("Velocities", sim.velocityBuffer);

        const int subMeshIndex = 0;
        uint[] args = new uint[5];
        args[0] = mesh.GetIndexCount(subMeshIndex);
        args[1] = (uint)sim.positionBuffer.count;
        args[2] = mesh.GetIndexStart(subMeshIndex);
        args[3] = mesh.GetBaseVertex(subMeshIndex);
        args[4] = 0;
        _buffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        _buffer.SetData(args);
        _bounds = new Bounds(Vector3.zero, Vector3.one * 10000);
    }

    public void UpdateDisplay()
    {
        if (_updateGradient)
        {
            _updateGradient = false;
            _gradientTexture = TextureFromGradient(gradientResolution, colourMap);
            _mat.SetTexture("ColourMap", _gradientTexture);
        }
        _mat.SetFloat("scale", scale);
        _mat.SetColor("colour", col);
        _mat.SetFloat("velocityMax", velocityDisplayMax);
        Graphics.DrawMeshInstancedIndirect(mesh, 0, _mat, _bounds, _buffer);
    }

    public Texture2D TextureFromGradient(int width, Gradient gradient)
    {
        Texture2D texture = new Texture2D(width, 1, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        Color32[] colors = new Color32[width];
        for (int i = 0; i < width; i++)
        {
            float t = i / (float)width;
            colors[i] = gradient.Evaluate(t);
        }
        texture.SetPixels32(colors);
        texture.Apply();
        return texture;
    }
    void OnDestroy()
    {
        try
        {
            _buffer.Release();
        }
        catch { /*they dont exist*/}
    }
}
