using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GBufferPass : MonoBehaviour
{

    public RenderTexture tempBuffer;


    public void Setup(ScriptableRenderContext context, Camera camera, CullingResults cullingResults)
    {

    }
    public void Render(ScriptableRenderContext context, Camera camera, CullingResults cullingResults)
    {
        CommandBuffer cmd = new CommandBuffer();
        cmd.name = "gbufferpass";


    }
}
