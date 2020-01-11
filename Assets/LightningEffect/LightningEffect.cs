using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningEffect : MonoBehaviour
{
    private struct PathSegment
    {
        public Vector2 start;
        public Vector2 end;
        public float minDist;
        public float maxDist;


        public PathSegment(Vector2 start, Vector2 end, float minDist, float maxDist)
        {
            this.start = start;
            this.end = end;
            this.minDist = minDist;
            this.maxDist = maxDist;
        }

    }

    public float respositionChance;
    [SerializeField]
    private ParticleSystem emitter;
    
    [SerializeField]
    private Sprite pathSprite;

    [SerializeField]
    private GameObject startPointObj;
    
    private List<Vector2> pathPoints;
    private List<PathSegment> pathSegments; 
    private ParticleSystem.Particle[] particles;
    private float pathLength;
    
    private float temp = 0;


    void Start()
    {
        pathPoints = new List<Vector2>();
        pathSegments = new List<PathSegment>();
        particles = new ParticleSystem.Particle[emitter.main.maxParticles];
        
        pathSprite.GetPhysicsShape(0, pathPoints);

        float startPointMinDist = 100f;
        int startIdx = 0;
        for (int i = 0; i < pathPoints.Count; i++)
        {
            Vector2 point = pathPoints[i];
            float dist = Vector2.Distance(startPointObj.transform.position, point);
            if(dist < startPointMinDist)
            {
                startPointMinDist = dist;
                startIdx = i;
            }

            
        }

        int idx = startIdx;
        Vector2 p1;
        Vector2 p2;
        for (int i = 0; i < pathPoints.Count; i++)
        {
            p1 = pathPoints[idx];
            // GameObject temp = GameObject.Instantiate(startPointObj, pathPoints[idx], Quaternion.identity);
            // temp.GetComponent<SpriteRenderer>().color = new Color((float)i/pathPoints.Count, 0, 0, 1);
            if(idx + 1 >= pathPoints.Count)
            {
                idx = 0;
            }
            else
            {
                idx++;
            }

            //Calculate distance between points
            p2 = pathPoints[idx];
            float curDist = Vector2.Distance(p2, p1);
            pathSegments.Add(new PathSegment(p1, p2, pathLength, pathLength + curDist));
            pathLength += curDist;

        }

    }

    // Update is called once per frame
    void Update()
    {
        int numParticlesAlive = emitter.GetParticles(particles);
        for (int i = 0; i < numParticlesAlive; i++)
        {
            float rand = Random.Range(0f, 1.0f);
            if(rand <= respositionChance)
            {
                Vector2 newPos = GetPositionOnPath(particles[i].remainingLifetime, particles[i].startLifetime);
                
                //Set particle position to new position with respect to the emitter's coordinates
                particles[i].position = emitter.transform.InverseTransformPoint(newPos);
            }


        }
        emitter.SetParticles(particles);
        // if(temp <= 0.35)
        // {
        //     GameObject.Instantiate(startPointObj, GetPositionOnPath(temp), Quaternion.identity);
        //     temp += 0.01f;
        // }

    }

    Vector2 GetPositionOnPath(float remainingLife, float maxLife)
    {
        float normalizedAge = (maxLife - remainingLife)/maxLife;
        float currentDistance = normalizedAge * pathLength;
        
        foreach(var pathSegment in pathSegments)
        {
            if(currentDistance > pathSegment.minDist && currentDistance < pathSegment.maxDist)
            {
                return Vector2.Lerp(pathSegment.start, pathSegment.end, (currentDistance - pathSegment.minDist)/(pathSegment.maxDist - pathSegment.minDist));
            }
        }
        return pathSegments[0].start;
    }
}
