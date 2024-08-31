using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField] Config config;
    [SerializeField] Material lineMaterial;

    public bool drawBounds = false;

    private void OnRenderObject()
    {
        if (!drawBounds) return; // could be optimized

        // do boundary rendering here  
        float halfSize = config.boundarySize / 2;
        lineMaterial.SetPass(0);
        
        GL.PushMatrix();
        GL.Begin(GL.LINES);

        GL.Color(Color.black);

        // front face
        GL.Vertex3(-halfSize, -halfSize, halfSize);
        GL.Vertex3(halfSize, -halfSize, halfSize);

        GL.Vertex3(halfSize, -halfSize, halfSize);
        GL.Vertex3(halfSize, halfSize, halfSize);

        GL.Vertex3(halfSize, halfSize, halfSize);
        GL.Vertex3(-halfSize, halfSize, halfSize);

        GL.Vertex3(-halfSize, halfSize, halfSize);
        GL.Vertex3(-halfSize, -halfSize, halfSize);

        // rear face
        GL.Vertex3(-halfSize, -halfSize, -halfSize);
        GL.Vertex3(halfSize, -halfSize, -halfSize);

        GL.Vertex3(halfSize, -halfSize, -halfSize);
        GL.Vertex3(halfSize, halfSize, -halfSize);

        GL.Vertex3(halfSize, halfSize, -halfSize);
        GL.Vertex3(-halfSize, halfSize, -halfSize);

        GL.Vertex3(-halfSize, halfSize, -halfSize);
        GL.Vertex3(-halfSize, -halfSize, -halfSize);

        // connecting edges
        GL.Vertex3(-halfSize, -halfSize, halfSize);
        GL.Vertex3(-halfSize, -halfSize, -halfSize);

        GL.Vertex3(halfSize, -halfSize, halfSize);
        GL.Vertex3(halfSize, -halfSize, -halfSize);

        GL.Vertex3(halfSize, halfSize, halfSize);
        GL.Vertex3(halfSize, halfSize, -halfSize);

        GL.Vertex3(-halfSize, halfSize, halfSize);
        GL.Vertex3(-halfSize, halfSize, -halfSize);

        GL.End();
        GL.PopMatrix();
    }
}
