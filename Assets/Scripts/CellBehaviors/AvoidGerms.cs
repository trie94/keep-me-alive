using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/AvoidGerms")]
public class AvoidGerms : CellMovement
{
    public CreatureTypes type = CreatureTypes.Germ;
    public Vector3 debugVelocity = Vector3.zero;

    public override Vector3 CalculateVelocity(Cell creature, Dictionary<CreatureTypes, List<Transform>> groups, Vector3? target)
    {
        List<Transform> neighbors = groups[type];
        if (neighbors == null || neighbors.Count == 0) return creature.transform.forward;
        Vector3 velocity = Vector3.zero;
        float distFactor = 0f;

        for (int i = 0; i < neighbors.Count; i++)
        {
            var currWorm = neighbors[i];
            Vector3 cellToWorm = currWorm.position - creature.transform.position;
            distFactor = cellToWorm.sqrMagnitude;
            Vector3 segmentToProject = creature.currSeg.end.transform.position
                                               - creature.currSeg.start.transform.position;
            Vector3 projectedVector = Vector3.Project(cellToWorm, segmentToProject);
            Vector3 projectedVectorToWorm = cellToWorm - projectedVector;
            Vector3 singleVelocity = -projectedVectorToWorm;
            velocity += singleVelocity;
        }

        velocity /= neighbors.Count;
        debugVelocity = velocity;
        velocity = Vector3.Lerp(velocity, creature.transform.forward, 0.7f);

        return velocity;
    }


    public override void DrawGizmos(Cell creature, Dictionary<CreatureTypes, List<Transform>> groups)
    {
        Gizmos.DrawLine(creature.transform.position, creature.transform.position + debugVelocity);
    }
}
