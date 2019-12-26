using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Movement<T> : ScriptableObject
{
    public abstract Vector3 CalculateVelocity(T creature, Dictionary<CreatureTypes, List<Transform>>groups, Vector3? target = null);
    public virtual void DrawGizmos(T creature, Dictionary<CreatureTypes, List<Transform>> groups){}
}


[System.Serializable]
public abstract class CellMovement : Movement<Cell> { }

[System.Serializable]
public abstract class OxygenMovement : Movement<Oxygen> { }
