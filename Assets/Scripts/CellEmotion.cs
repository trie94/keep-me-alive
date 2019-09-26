using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/Emotion")]
public class CellEmotion : ScriptableObject
{
    public List<Texture2D[]> emotions;
    public Texture2D[] neutral;
    public Texture2D[] surprise;
    private float tick = 0;
    private int randomIndex = 0;

    public void InitEmotions()
    {
        emotions = new List<Texture2D[]>();
        emotions.Add(neutral);
        emotions.Add(surprise);
    }

    // TODO: make it different between cells
    public Texture2D[] GetEmotion(float pickTime)
    {
        if (tick > pickTime)
        {
            tick = 0;
            randomIndex = Random.Range(0, emotions.Count);
            return emotions[randomIndex];
        }
        tick += Time.deltaTime;
        return emotions[randomIndex];
    }
}
