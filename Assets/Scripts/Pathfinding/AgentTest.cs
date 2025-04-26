using System.Collections;
using System.Collections.Generic;
using Scripts.Pathfinding;
using Scripts.Random;
using UnityEngine;

public class AgentTest : MonoBehaviour
{
    [SerializeField] private Renderer _boundBox; // for debug, remove later
    private IList<Vector3> Path = new List<Vector3>();

    private ISeedRandom _rand;

    private void Awake()
    {
        // _rand = new SeedRandom(gameObject);
    }

    private void Start()
    {
        
        Bounds values = _boundBox.bounds;

        Vector3 randomEnd = new Vector3(
            _rand.Range(values.min.x, values.max.x),
            transform.position.y, // Random.Range(values.min.y, values.max.y), // this wont be needed later i assume
            _rand.Range(values.min.z, values.max.z)
        );

        Debug.DrawLine(transform.position, randomEnd, Color.yellow, float.MaxValue);

        // Find path
        Path = DRcHandle.FindPath(transform.position, randomEnd);

        Vector3 last = Path[0];
        foreach ( Vector3 vec in Path)
        {
            Debug.DrawLine(vec, last, Color.magenta, float.MaxValue);
            last = vec;
        }

        StartCoroutine(FollowPath(Path, 2f));
    }


    private IEnumerator FollowPath(IList<Vector3> path, float speed)
    {
        foreach (Vector3 point in path)
        {
            while (Vector3.Distance(transform.position, point) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, point, speed * Time.deltaTime);
                yield return null;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if ( Path == null || Path.Count == 0 ) return;
        
        foreach (Vector3 vec in Path)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(vec, 0.5f);
        }
    }
}
