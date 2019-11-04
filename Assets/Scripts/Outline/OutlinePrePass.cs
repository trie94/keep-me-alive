using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class OutlinePrePass : MonoBehaviour
{
    public RenderTexture prepass;
    public RenderTexture blurred;
    private Material blurMat;
    private Camera cam;

    private void Awake()
    {
        prepass = new RenderTexture(Screen.width, Screen.height, 24);
        prepass.antiAliasing = QualitySettings.antiAliasing;
        blurred = new RenderTexture(Screen.width >> 1, Screen.height >> 1, 0);

        cam = GetComponent<Camera>();
        cam.targetTexture = prepass;
		
        Shader outlineShader = Shader.Find("Unlit/OutlineReplace");
        Shader.SetGlobalColor("_OutlineColor", new Color(71f / 225f, 63f / 225f, 52f / 225f, 1));
        cam.SetReplacementShader(outlineShader, "OutlineTarget");

        Shader.SetGlobalTexture("_PrePassTex", prepass);
        Shader.SetGlobalTexture("_BlurredTex", blurred);

        blurMat = new Material(Shader.Find("Unlit/Blur"));
        blurMat.SetVector("_BlurSize", new Vector2(blurred.texelSize.x * 1.5f, blurred.texelSize.y * 1.5f));
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        Graphics.Blit(src, dst);

        Graphics.SetRenderTarget(blurred);
        GL.Clear(false, true, Color.clear);

        Graphics.Blit(src, blurred);

        for (int i = 0; i < 4; i++)
        {
            var temp = RenderTexture.GetTemporary(blurred.width, blurred.height);
            Graphics.Blit(blurred, temp, blurMat, 0);
            Graphics.Blit(temp, blurred, blurMat, 1);
            RenderTexture.ReleaseTemporary(temp);
        }
    }
}
