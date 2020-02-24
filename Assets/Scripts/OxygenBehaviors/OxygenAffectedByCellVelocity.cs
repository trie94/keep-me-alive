using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Oxygen/AffectedByCellVelocity")]
public class OxygenAffectedByCellVelocity : OxygenMovement
{
    public override Vector3 CalculateVelocity(Oxygen creature, Dictionary<CreatureTypes, List<Transform>> groups, Vector3? target = null)
    {
        if (creature.master == null)
        {
            return creature.playerMaster.Velocity * -1f * creature.velocitySensitivity;
        }
        else
        {
            Debug.Assert(creature.master is Erythrocyte);
            return ((Erythrocyte)creature.master).Velocity * -1f * creature.velocitySensitivity;
        }
    }
}
