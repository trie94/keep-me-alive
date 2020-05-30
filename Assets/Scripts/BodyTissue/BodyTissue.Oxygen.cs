using UnityEngine;

public partial class BodyTissue : MonoBehaviour
{
    private int oxygenCapacity = 1;
    private int oxygenNumber;

    private void FindClosestOxygen()
    {
        if (NeedOxygen())
        {
            Oxygen closest = null;
            float min = float.MaxValue;
            for (int i = 0; i < OxygenController.Instance.oxygenList.Count; i++)
            {
                Oxygen curr = OxygenController.Instance.oxygenList[i];
                Vector3 headToOxygen = curr.transform.position - head.position;
                if (curr.state == MoleculeState.Released || curr.state == MoleculeState.OxygenArea) continue;

                if (headToOxygen.sqrMagnitude < min)
                {
                    closest = curr;
                    min = headToOxygen.sqrMagnitude;
                }
            }

            if (closest != null)
            {
                if (min < grabOxygenRadius * grabOxygenRadius)
                {
                    // check if it is already grabbed
                    GrabOxygen(closest);
                }

                if (min < attractRadius * attractRadius)
                {
                    SetTarget(closest.transform);
                }
                else if (target.targetToFollow != null)
                {
                    SetTarget(null);
                }
            }
        }
    }

    public bool NeedOxygen()
    {
        return oxygenNumber < oxygenCapacity;
    }

    private void GrabOxygen(Oxygen oxygen)
    {
        oxygen.carrier.ReleaseOxygen(oxygen, this);
    }
}