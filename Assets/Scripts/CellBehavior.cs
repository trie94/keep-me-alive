using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CellBehavior : ScriptableObject
{
    public abstract Vector3 CalculateMove(Cell cell, List<Transform> context);
}
