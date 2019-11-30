using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Path))]
public class PathEditor : Editor
{
    private int lineSteps = 10;

    private void OnSceneGUI()
    {
        Path path = target as Path;
        if (path == null || path.segments == null) return;

        for (int i = 0; i < path.segments.Count; i++)
        {
            Segment currSeg = path.segments[i];
            Node segmentStart = currSeg.n0;
            Node segmentEnd = currSeg.n1;
            Handles.color = new Color(1, 1, 1, currSeg.weight * 0.3f);

            Vector3 p0 = segmentStart.transform.position;

            for (int j = 1; j <= lineSteps; j++)
            {
                Vector3 p1 = path.GetPoint(currSeg, j / (float)lineSteps);
                Handles.DrawLine(p0, p1);
                p0 = p1;
            }
        }
    }
}
