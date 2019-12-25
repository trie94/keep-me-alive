using UnityEngine;

public class PathTester : MonoBehaviour
{
    private float progress = 0f;
    private float duration = 10f;
    private float speed;
    private Segment currSeg;

    private void Start()
    {
        //speed = Random.Range(0.5f, 2f);
        speed = 0.3f;
        int segIndex = Random.Range(0, Path.Instance.segments.Count);
        currSeg = Path.Instance.segments[segIndex];
        // initial position
        transform.position = Path.Instance.GetPoint(currSeg, progress);
    }

    private void Update()
    {
        if (Path.Instance == null || Path.Instance.segments == null) return;

        progress += Time.deltaTime / duration * speed;
        if (progress >= 1f)
        {
            progress = 0f;
            currSeg = GetNextSegment();
        }

        Vector3 target = Path.Instance.GetPoint(currSeg, progress);
        Vector3 lookDir = target - transform.position;

        if (lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        }
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * speed);
    }

    private Segment GetNextSegment()
    {
        var nextSeg = currSeg.n1.nextSegments;
        if (nextSeg.Count > 1)
        {
            float weightSum = 0f;
            for (int i = 0; i < nextSeg.Count; i++)
            {
                weightSum += nextSeg[i].weight;
            }

            float rnd = Random.Range(0, weightSum);
            for (int i = 0; i < nextSeg.Count; i++)
            {
                if (rnd < nextSeg[i].weight)
                    return nextSeg[i];
                rnd -= nextSeg[i].weight;
            }
        }
        else
        {
            return currSeg.n1.nextSegments[0];
        }

        return currSeg.n1.nextSegments[0];
    }
}
