using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineComposite : MonoBehaviour
{
    [Range (0, 10)]
    [SerializeField]
	private float Intensity = 2;

	private Material compositeMat;

	void OnEnable()
	{
		compositeMat = new Material(Shader.Find("Unlit/OutlineComposite"));
    }

	void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		compositeMat.SetFloat("_Intensity", Intensity);
        Graphics.Blit(src, dst, compositeMat, 0);
	}
}
