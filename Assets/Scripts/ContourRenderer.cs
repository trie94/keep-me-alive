using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ContourRenderer : MonoBehaviour
{
    [SerializeField]
    Material mat;

    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest, mat);
    }
}
