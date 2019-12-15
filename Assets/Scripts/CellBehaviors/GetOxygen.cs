using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/GetOxygen")]
public class GetOxygen : CellBehavior
{
    public override Vector3 CalculateVelocity(Cell cell, List<Transform> neighbors)
    {
        // only check the oxygens
        if (neighbors == null || neighbors.Count == 0) return Vector3.zero;
        Vector3 velocity = Vector3.zero;

        // check cap
        if (cell.currOxygen >= cell.oxygenCapacity)
        {
            cell.cellState = CellState.ExitOxygen;
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

            if (Vector3.SqrMagnitude(curr.position - cell.transform.position)
                < Vector3.SqrMagnitude(closest.position - cell.transform.position))
            {
                closest = curr;
            }
        }

        if (closest == null) return velocity;

        // we catch the closest oxygen
        if (Vector3.SqrMagnitude(closest.position - cell.transform.position) < 0.2f)
        {
            closest.GetComponent<Oxygen>().master = cell;
            cell.currOxygen++;
            Debug.Log("catch the oxygen");
            return velocity;
        }

        // get close to the closest oxygen
        velocity += closest.position;
        velocity -= cell.transform.position;
        Debug.Log("catching " + closest.name);

        return velocity;
    }
}
