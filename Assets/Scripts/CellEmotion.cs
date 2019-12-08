using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Emotions
{
    Neutral, Happy, Sad, Surprised    
}

[CreateAssetMenu(menuName = "Cell/Emotion")]
public class CellEmotion : ScriptableObject
{
    public Texture2D[] neutral;
    public Texture2D[] happy;
    public Texture2D[] sad;
    public Texture2D[] surprise;

    public Texture2D[] MapEnumWithTexture(Emotions emotion)
    {
        // TODO: this will be returning correct array
        switch(emotion)
        {
            case Emotions.Neutral:
            case Emotions.Happy:
            case Emotions.Sad:
            case Emotions.Surprised:
            default:
                return neutral;
        }
    }
}
