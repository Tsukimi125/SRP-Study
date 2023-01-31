using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LightPass
{
    public RenderTexture tempBuffer;



    public void Setup(ScriptableRenderContext context, Camera camera, CullingResults cullingResults)
    {

    }
    public void Render(ScriptableRenderContext context, Camera camera, CullingResults cullingResults)
    {
        CommandBuffer cmd = new CommandBuffer();
        cmd.name = "lightpass";

        Material mat = new Material(Shader.Find("Kaiser RP/lightpass"));
        cmd.Blit(tempBuffer, BuiltinRenderTextureType.CameraTarget, mat);
        context.ExecuteCommandBuffer(cmd);
    }
}
