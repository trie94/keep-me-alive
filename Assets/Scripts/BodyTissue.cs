using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO - tissue type will be needed if planning to add different behaviors
// based on the different tissues.

public class BodyTissue : MonoBehaviour
{
    #region oxygen
    private int oxygenCapacity = 1;
    private int oxygenNumber;
    #endregion

    #region carbon dioxide
    private int carbonDioxideCapacity = 2;
    private int carbonDioxideNumber;
    [SerializeField]
    private float carbonDioxideSpawnInterval = 10f;
    private float tick;
    #endregion

    #region visual
    private float length;
    private float speed;
    #endregion

    private void SpawnCarbonDioxide()
    {

    }
}
