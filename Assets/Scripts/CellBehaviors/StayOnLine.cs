using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/StayOnLine")]
public class StayOnLine : CellMovement
{
    public override Vector3 CalculateVelocity(Cell creature, Dictionary<CreatureTypes, List<Transform>> groups, Vector3? target)
    {
        if (creature.currSeg == null) return Vector3.zero;
        Vector3 cellToClosestPointOnLine =
            CurrentManager.Instance.GetClosestPointOnLine(creature.currSeg, creature.transform.position);
        float distFactor = cellToClosestPointOnLine.sqrMagnitude;
        distFactor = Mathf.Pow(Mathf.Clamp01(distFactor), 3f) * distFactor;
        Vector3 velocity = Vector3.Lerp(cellToClosestPointOnLine - creature.transform.position, creature.transform.forward, 0.5f);
        return velocity * distFactor;
    }
}
