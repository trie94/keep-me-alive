using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leukocyte : Cell
{
    public override void Awake()
    {
        base.Awake();
        cellType = CellType.Leukocyte;
    }
}
