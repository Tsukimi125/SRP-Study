using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class KaiserRenderPipeline : RenderPipeline
{
    // G-buffer Settings
    RenderTexture gdepth;
    RenderTexture[] gbuffers = new RenderTexture[4];
    RenderTargetIdentifier[] gbuffersID = new RenderTargetIdentifier[4];

    // LightPass 
    LightPass lightPass = new LightPass();

    public string renderPipelineType;
    KaiserForwardRenderer forwardRenderer = new KaiserForwardRenderer();
    KaiserDeferredRenderer deferredRenderer = new KaiserDeferredRenderer();

    GlobalRenderSettings globalRenderSettings;
    public KaiserRenderPipeline(GlobalRenderSettings globalRenderSettings)
    {
        // 全局设置

        GraphicsSettings.useScriptableRenderPipelineBatching = globalRenderSettings.useSRPBatcher;
        this.globalRenderSettings = globalRenderSettings;
        
        GraphicsSettings.lightsUseLinearIntensity = true;

        // 设置渲染管线类型

    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        if (renderPipelineType == "Deferred")
        {
            foreach (var camera in cameras)
            {
                deferredRenderer.Render(context, camera);
            }
        }
        else
        {
            foreach (var camera in cameras)
            {
                forwardRenderer.Render(context, camera, globalRenderSettings);
            }
        }


    }
}
