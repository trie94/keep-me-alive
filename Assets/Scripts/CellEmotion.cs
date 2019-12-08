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
    public Texture2D[] surprised;

    public Texture2D[] MapEnumWithTexture(Emotions emotion)
    {
        switch(emotion)
        {
            case Emotions.Neutral:
                return neutral;
            case Emotions.Happy:
                return happy;
            case Emotions.Sad:
                return sad;
            case Emotions.Surprised:
                return surprised;
            default:
                return neutral;
        }
    }
}
