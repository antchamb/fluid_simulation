using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Timeline;
using static UnityEngine.Mathf;

public class FilledCircle : MonoBehaviour
{
    // Circle design
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    public float radius = 1f;
    public int segments = 50;
    public Color color = Color.blue;

    // Circle Behavior
    public float gravity = 9.8f;
    public float particleSize = 1f;
    Vector2 position;
    Vector2 velocity;

    // handling collision
    // public Vector2 boundsSize = new Vector2(5f, 5f);
    public float collisionDamping = 0.5f; // between 0 and 1
    public float boundaryRadius = 5f;
    // Drawing the box
    private LineRenderer lineRenderer;
    private LineRenderer circleLineRenderer;

    void Start()
    {
        // Add a MeshFilter and MeshRenderer to the GameObject
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        // Set the material for the mesh (you can set it to any 2D material)
        meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
        meshRenderer.material.color = color;

        position = Vector2.zero;
        velocity = Vector2.zero;

        // Initially create the filled circle
        DrawCircle(position, particleSize, radius);
    
        /*
        // for the box
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 5;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;

        DrawBox();
        */

        circleLineRenderer = gameObject.AddComponent<LineRenderer>();
        circleLineRenderer.startWidth = 0.05f;
        circleLineRenderer.endWidth = 0.05f;
        circleLineRenderer.useWorldSpace = false; 
        circleLineRenderer.positionCount = segments + 1; 
        circleLineRenderer.loop = true;
        circleLineRenderer.startColor = Color.red; 
        circleLineRenderer.endColor = Color.red;

        // Draw the circular boundary
        DrawBoundaryCircle(Vector2.zero, boundaryRadius);

    }


    void Update()
    {
        velocity += Vector2.down * gravity * Time.deltaTime;
        position += velocity * Time.deltaTime;
        ResolveCollisions();
        // Call CreateFilledCircle every frame (e.g., if you want dynamic updates to the circle)
        DrawCircle(position, particleSize, radius);
    }

    void DrawCircle(Vector2 center, float size, float radius)
    {
        // If meshFilter doesn't exist, exit
        if (meshFilter == null) return;

        // Create arrays for vertices and triangles
        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];

        // Set the center vertex
        vertices[0] = new Vector3(center.x, center.y, 0);

        // Calculate angle for each segment
        float angle = 360f / segments;

        // Create the vertices for the circle
        for (int i = 1; i <= segments; i++)
        {
            float theta = Deg2Rad * angle * i;
            float x = Cos(theta) * radius * size;
            float y = Sin(theta) * radius * size;

            vertices[i] = new Vector3(x + center.x, y + center.y, 0);
        }

        // Create the triangles (3 vertices per triangle)
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0; // Center point
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 2 <= segments) ? i + 2 : 1; // Loop back to the first vertex
        }

        // Create the mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // Assign the mesh to the MeshFilter
        meshFilter.mesh = mesh;
    }

    /*
    void ResolveCollisions()
    {
        Vector2 halfBoundsSize = boundsSize / 2 - Vector2.one * particleSize;
        if (Abs(position.x) > halfBoundsSize.x)
        {
            position.x = halfBoundsSize.x * Sign(position.x);
            velocity.x = -1 * collisionDamping;
        }
        if (Abs(position.y) > halfBoundsSize.y)
        {
            position.y = halfBoundsSize.y * Sign(position.y);
            velocity.y *= -1 * collisionDamping;
        }
    }
    */
    void ResolveCollisions()
    {
        float distanceFromCenter = Vector2.Distance(position, Vector2.zero);

        // Check if the circle is outside the boundary
        if (distanceFromCenter + particleSize > boundaryRadius)
        {
            // Calculate the direction to the center
            Vector2 directionToCenter = (position - Vector2.zero).normalized;

            // Reflect the velocity vector off the boundary
            velocity = Vector2.Reflect(velocity, directionToCenter) * collisionDamping;

            // Adjust position to push it slightly inside the boundary
            float overlap = (distanceFromCenter + particleSize) - boundaryRadius;
            position -= directionToCenter * overlap; // Move it just back inside
        }
    }

    /*
    void DrawBox()
    {
        Vector3[] corners = new Vector3[5];
        float halfWidth = boundsSize.x / 2;
        float halfHeight = boundsSize.y / 2;

        corners[0] = new Vector3(-halfWidth, -halfHeight, 0);
        corners[1] = new Vector3(-halfWidth, halfHeight, 0);
        corners[2] = new Vector3(halfWidth, halfHeight, 0);
        corners[3] = new Vector3(halfWidth, -halfHeight, 0);
        corners[4] = corners[0];

        lineRenderer.SetPositions(corners);

    }
    */
    void DrawBoundaryCircle(Vector2 center, float radius)
    {
        float angle = 2 * PI / segments;
        Vector3[] points = new Vector3[segments + 1];

        for (int i = 0; i <= segments; i++)
        {
            float x = Cos(i * angle) * radius;
            float y = Sin(i * angle) * radius;
            points[i] = new Vector3(x + center.x, y + center.y, 0);
        }

        // Set positions for the LineRenderer
        circleLineRenderer.SetPositions(points);
    }
}
