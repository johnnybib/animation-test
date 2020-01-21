using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LightningEffectV3 : MonoBehaviour
{

    [SerializeField]
    private Sprite pathSprite = null;

    [SerializeField]
    private GameObject outline = null;

    [SerializeField]
    private float outlineWeight = 0.2f;


    [SerializeField]
    private GameObject[] radialPoints = null;

    [SerializeField]
    private float repositionChance = 0.5f;
    [SerializeField]
    private float period = 0.1f;
    [SerializeField]
    private float magnitude = 0.2f;
    [SerializeField]
    private float meshResolution = 1f;


    private float timeSinceUpdate;
    private Vector3[] originalVertices;
    private Vector3[] originalOutlineVertices;
    private Vector3[] vertices;

    private Vector3[] outlineVertices;

    private Mesh mesh;
    private Mesh outlineMesh;
    void Start()
    {
        
        mesh = GetComponent<MeshFilter>().mesh;
        outlineMesh = outline.GetComponent<MeshFilter>().mesh;


        List<Vector2> pathPoints = new List<Vector2>();


        //Get points from sprite physics shape
        pathSprite.GetPhysicsShape(0, pathPoints);

        //Lower number of points in mesh if wanted
        if(meshResolution < 1)
        {
            int skipPointInterval = (int)pathPoints.Count/((int)(pathPoints.Count * meshResolution));
            int pos = 0;
            // Debug.Log(pathPoints.Count);
            for (int i = 0; i < pathPoints.Count; i += skipPointInterval, pos++) {
                pathPoints[pos] = pathPoints[i];
            }
            pathPoints.RemoveRange(pos, pathPoints.Count - pos);
            // Debug.Log(pathPoints.Count);
        }


        Vector2[] vertices2D = pathPoints.ToArray();


        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(vertices2D);
        int[] indices = tr.Triangulate();

        //Convert vertices to Vector3
        originalVertices = new Vector3[vertices2D.Length];
        originalOutlineVertices = new Vector3[vertices2D.Length];
        
        for (int i=0; i<originalVertices.Length; i++) {
            originalVertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0);
        }
        for(int i = 0; i<originalVertices.Length; i++)
        {
            Vector3 normalVec = GetNormal(originalVertices, i);
            originalOutlineVertices[i] = originalVertices[i] + normalVec*outlineWeight;
        }

        mesh.vertices = originalVertices;
        mesh.triangles = indices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds(); 


        outlineMesh.vertices = originalOutlineVertices;
        outlineMesh.triangles = indices;
        outlineMesh.RecalculateNormals();
        outlineMesh.RecalculateBounds(); 

       
        
        vertices = new Vector3[originalVertices.Length];
        outlineVertices = new Vector3[originalOutlineVertices.Length];
    }

    // Update is called once per frame
    void Update()
    {
        if(timeSinceUpdate >= period)
        {
            originalVertices.CopyTo(vertices, 0);
            originalOutlineVertices.CopyTo(outlineVertices, 0);

            
            for (int i = 0; i < vertices.Length; i++)
            {
                float randNum = Random.Range(0f, 1.0f);
                if(randNum <= repositionChance)
                {
                    Vector3 closestPoint = Vector2.zero;
                    float closestDist = 1000;
                    foreach(var point in radialPoints)
                    {
                        float newDist = Vector2.Distance(vertices[i], point.transform.localPosition);
                        if(newDist < closestDist)
                        {
                            closestDist = newDist;
                            closestPoint = point.transform.localPosition;
                        }
                    }

                    float randMag = Random.Range(0, magnitude);

                    Vector3 radialVector = vertices[i] - closestPoint;
                    vertices[i] += radialVector.normalized*randMag;
                }
            }

            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds(); 

            for(int i = 0; i<vertices.Length; i++)
            {
                Vector3 normalVec = GetNormal(vertices, i);
                outlineVertices[i] = vertices[i] + normalVec*outlineWeight;
            }




            outlineMesh.vertices = outlineVertices;
            outlineMesh.RecalculateNormals();
            outlineMesh.RecalculateBounds();

            timeSinceUpdate = 0;
        }

        timeSinceUpdate += Time.deltaTime;         
         
    }

    Vector3 GetNormal(Vector3[] points, int i)
    {
        Vector3 prevPoint;
        Vector3 nextPoint;
        Vector3 curPoint = points[i];
        
        if(i <= 0)
        {
            prevPoint = points[points.Length - 1];
        }
        else
        {
            prevPoint = points[i-1];
        }

        if(i >= points.Length-1)
        {
            nextPoint = points[0];
        }
        else
        {
            nextPoint = points[i+1];
        }
        Vector3 vec1 = Vector3.Normalize(prevPoint - curPoint);
        Vector3 vec2 = Vector3.Normalize(nextPoint - curPoint);

        Vector3 normal = vec1+vec2;
        if(normal == Vector3.zero)
        {
            normal = Vector2.Perpendicular(vec1);
        }
        normal = Vector3.Normalize(normal);
        
        // Debug.DrawLine(curPoint, curPoint+normal, Color.blue, period);
        if(Vector3.Magnitude(curPoint - normal) > Vector3.Magnitude(curPoint + normal))
        {
            normal *= -1;
        }
        
        // Debug.DrawLine(curPoint, curPoint+normal, Color.red, period);
        return normal;
    }

}
