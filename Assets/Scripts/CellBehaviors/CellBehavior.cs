using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CellBehavior : ScriptableObject
{
    public abstract Vector3 CalculateVelocity(Cell cell, List<Transform> neighbors);
    public virtual void DrawGizmos(Cell cell, List<Transform> context){}
}
