
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(LineRenderer))]
public class dropscript : MonoBehaviour
{
    [Header("Rope Settings")]
    public Transform startPoint;
    public int segmentCount = 12;
    public float segmentLength = 0.15f;

    [Header("Physics")]
    public float segmentMass = 0.1f;
    public float damping = 0.5f;

    private LineRenderer line;
    private Rigidbody2D[] bodies;
    private Rigidbody2D anchorBody;
    public GameObject lightPrefab;



    public void CreateRope()
    {

        foreach (Transform child in transform)
        {
            if(child != startPoint)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        line = GetComponent<LineRenderer>();
        if (startPoint == null)
        {
            Debug.LogError("Assign a Start Point!");
            return;
        }

        bodies = new Rigidbody2D[segmentCount];

        Vector2 start = startPoint.position;

        for (int i = 0; i < segmentCount; i++)
        {
            GameObject segment = new GameObject("Rope Segment " + i);

            segment.transform.SetParent(transform);
            segment.transform.position =
                start + Vector2.down * segmentLength * (i + 1);

            Rigidbody2D rb = segment.AddComponent<Rigidbody2D>();
            rb.mass = segmentMass;
            rb.linearDamping = damping;
            rb.gravityScale = 1f;

            bodies[i] = rb;

            DistanceJoint2D joint =
                segment.AddComponent<DistanceJoint2D>();
            GameObject spawnedLight = Instantiate(
            lightPrefab,
            segment.transform
            );

            spawnedLight.transform.localPosition = Vector3.zero;

            joint.autoConfigureDistance = false;
            joint.distance = segmentLength;

            if (i == 0)
            {
                anchorBody = startPoint.GetComponentInParent<Rigidbody2D>();

                if (anchorBody != null)
                {
                    joint.connectedBody = anchorBody;
                    joint.connectedAnchor =
                        anchorBody.transform.InverseTransformPoint(
                            startPoint.position
                        );
                }
            }
            else
            {
                joint.connectedBody = bodies[i - 1];
                joint.connectedAnchor = Vector2.zero;
            }

            joint.enableCollision = false;
        }

        line.positionCount = segmentCount + 1;
    }

    void LateUpdate()
    {
        if (bodies == null || startPoint == null)
            return;

        line.positionCount = bodies.Length + 1;

        line.SetPosition(0, startPoint.position);

        for (int i = 0; i < bodies.Length; i++)
        {
            if (bodies[i] == null)
                continue;

            line.SetPosition(i + 1, bodies[i].position);
        }
    }
    public Rigidbody2D LastBody
    {
        get
        {
            if (bodies == null || bodies.Length == 0)
                return null;

            return bodies[bodies.Length - 1];
        }
    }
}
