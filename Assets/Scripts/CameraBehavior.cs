using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class CameraBehavior : MonoBehaviour
{
    private CommandBuffer commandBuffer;
    [SerializeField]
    private Material material;

    private static CameraBehavior s_instance;
    public static CameraBehavior Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = FindObjectOfType<CameraBehavior>();
            }
            return s_instance;
        }
    }

    private void Awake()
    {
        s_instance = this;
        commandBuffer = new CommandBuffer();
        commandBuffer.name = "command buffer";
        commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);

        BuildCommandBuffer();
        this.GetComponent<Camera>().AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
    }

    private void BuildCommandBuffer()
    {
        commandBuffer.Clear();
        commandBuffer.Blit(null as RenderTexture, BuiltinRenderTextureType.CurrentActive, material);
    }
}
