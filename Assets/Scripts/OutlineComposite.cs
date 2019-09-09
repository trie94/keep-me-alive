using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineComposite : MonoBehaviour
{
    [SerializeField]
    Material mat;

    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest, mat);
    }
}
