using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Kaiser Render Pipeline")]
public class KaiserRenderPipelineAssets : RenderPipelineAsset
{

    public enum RenderPipelineType
    {
        Forward,
        Deferred
    }
    [SerializeField]
    bool useSRPBatcher = true,
    useDynamicBatching = true,
    useGPUInstancing = true;

    [SerializeField]
    public RenderPipelineType renderPipelineType = RenderPipelineType.Forward;

    [SerializeField]
    ShadowSettings shadowSettings = default;

    protected override RenderPipeline CreatePipeline()
    {
        GlobalRenderSettings globalRenderSettings = new GlobalRenderSettings(
            useSRPBatcher,
            useDynamicBatching,
            useGPUInstancing,
            shadowSettings
        );
        return new KaiserRenderPipeline(globalRenderSettings);
    }
}