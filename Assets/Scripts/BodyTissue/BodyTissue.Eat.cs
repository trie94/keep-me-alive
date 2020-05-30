using UnityEngine;

public partial class BodyTissue : MonoBehaviour
{
    // instant eat
    private float oxygenInstantEatingTime = 3f;
    private float eatingTick = 0f;

    private float oxygenHoldTime = 20f;
    private float oxygenHoldStartTime;

    private float fullToHungryLerpTime = 3f;
    private float fullToHungryTick = 0f;

    [SerializeField]
    private Color headColor;
    [SerializeField]
    private Color paleColor;
    private float headColorFactor = 0f;
    private float headColorFactorTemp = 0f;
    private int headColorId;
    private int eatingProgressId;

    private void EatOxygenAndDigest()
    {
        if (state == BodyTissueState.IDLE)
        {
            if (oxygenNumber > 0 && Time.time - oxygenHoldStartTime > oxygenHoldTime)
            {
                Digest();
            }
        }
        else if (state == BodyTissueState.EAT)
        {
            Eating();
        }

        UpdateHeadColor();
    }

    private void Eating()
    {
        if (eatingTick < oxygenInstantEatingTime)
        {
            eatingTick += Time.deltaTime;
            float eatingProgress = eatingTick / oxygenInstantEatingTime;
            headColorFactor = Mathf.Clamp01(headColorFactorTemp + eatingProgress);
            mat.SetFloat(eatingProgressId, eatingProgress);
        }
        else
        {
            eatingTick = 0f;
            headColorFactorTemp = 0f;
            headColorFactor = 1f;
            mat.SetFloat(eatingProgressId, 0f);
            Debug.Log("nom nom");
            state = BodyTissueState.IDLE;
        }
    }

    private void Digest()
    {
        Debug.Assert(oxygenNumber > 0);
        if (fullToHungryTick < fullToHungryLerpTime)
        {
            fullToHungryTick += Time.deltaTime;
            headColorFactor = (fullToHungryLerpTime - fullToHungryTick) / fullToHungryLerpTime;
        }
        else
        {
            oxygenNumber--;
            fullToHungryTick = 0f;
            headColorFactor = 0f;
            Debug.Log("fully digest");
        }
    }

    private void UpdateHeadColor()
    {
        mat.SetColor(headColorId, Color.Lerp(paleColor, headColor, headColorFactor));
    }

    public void EatOxygen()
    {
        SetTarget(null);
        headColorFactorTemp = headColorFactor;
        state = BodyTissueState.EAT;
        oxygenNumber++;
        Debug.Log("start eating");

        oxygenHoldStartTime = Time.time;
    }
}