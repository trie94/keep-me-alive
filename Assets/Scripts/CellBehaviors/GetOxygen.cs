using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Erythrocyte/GetOxygen")]
public class GetOxygen : CellMovement
{
    public override Vector3 CalculateVelocity(Cell creature, List<Transform> neighbors)
    {
        // only check the oxygens
        if (neighbors == null || neighbors.Count == 0
            || !(creature is Erythrocyte)) return Vector3.zero;
        Vector3 velocity = Vector3.zero;
        Erythrocyte erythrocyte = (Erythrocyte)creature;

        // check cap
        if (erythrocyte.currOxygen >= erythrocyte.oxygenCapacity)
        {
            erythrocyte.cellState = CellState.ExitOxygen;
            return velocity;
            // when exit oxygen, the cell moves towards the exit node --> in the other behavior
        }

        // find the closest oxygen and grab it
        Transform closest = null;
        for (int i = 0; i < neighbors.Count; i++)
        {
            var curr = neighbors[i];
            if (curr.tag != "Oxygen" || curr.GetComponent<Oxygen>().master != null)
            {
                continue;
            }

            if (closest == null) 
            {
                closest = curr;
                Debug.Log(closest);
                continue;
            }

            if (Vector3.SqrMagnitude(curr.position - erythrocyte.transform.position)
                < Vector3.SqrMagnitude(closest.position - erythrocyte.transform.position))
            {
                closest = curr;
            }
        }

        if (closest == null) return velocity;

        // we catch the closest oxygen
        if (Vector3.SqrMagnitude(closest.position - erythrocyte.transform.position) < 0.2f)
        {
            closest.GetComponent<Oxygen>().master = erythrocyte;
            erythrocyte.currOxygen++;
            Debug.Log("catch the oxygen");
            return velocity;
        }

        // get close to the closest oxygen
        velocity += closest.position;
        velocity -= erythrocyte.transform.position;
        Debug.Log("catching " + closest.name);

        return velocity;
    }
}
