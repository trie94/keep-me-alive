using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Oxygen/HitHeart")]
public class HitHeart : OxygenMovement
{
    public override Vector3 CalculateVelocity(Oxygen creature, List<Transform> neighbors)
    {
        Vector3 velocity = OxygenController.Instance.heart.position - creature.transform.position;
        return velocity;
    }
}
