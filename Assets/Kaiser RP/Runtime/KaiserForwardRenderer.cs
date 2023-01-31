using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class KaiserForwardRenderer
{
    ScriptableRenderContext context;
    Camera camera;
    const string bufferName = "Kaiser Render Camera";
    string SampleName { get; set; }
    CommandBuffer cmd = new CommandBuffer
    {
        name = bufferName
    };

    CullingResults cullingResults;

    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    static ShaderTagId litShaderTagId = new ShaderTagId("KaiserForward");
    Lighting lighting = new Lighting();


    public void Render(ScriptableRenderContext context, Camera camera, GlobalRenderSettings globalRenderSettings)
    {
        this.context = context;
        this.camera = camera;

        if (!Cull(globalRenderSettings.shadowSettings.maxDistance)) return;

        PrepareBuffer();
        PrepareForSceneWindow();

        cmd.BeginSample(SampleName);
        ExecuteBuffer();

        // 配置光照、阴影
        lighting.Setup(context, cullingResults, globalRenderSettings.shadowSettings);
        cmd.EndSample(SampleName);

        // 正式开始渲染，配置相机等
        Setup();

        // 绘制可见物
        DrawVisibleGeometry(globalRenderSettings);

        DrawUnsupportedShaders();
        DrawGizmos();

        // 释放阴影RenderTexture内存
        lighting.Cleapup();
        Submit();
    }

    partial void PrepareBuffer();
    partial void PrepareForSceneWindow();
    public void Setup()
    {
        // 设置相机MVP
        context.SetupCameraProperties(camera);

        // Clear
        CameraClearFlags flags = camera.clearFlags;
        cmd.ClearRenderTarget(
            flags <= CameraClearFlags.Depth,
            flags == CameraClearFlags.Color,
            flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.black
        );

        cmd.BeginSample(SampleName);
        ExecuteBuffer();
    }

    public void DrawVisibleGeometry(GlobalRenderSettings globalRenderSettings)
    {
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };

        var drawingSettings = new DrawingSettings(
            unlitShaderTagId,
            sortingSettings
        )
        {
            enableDynamicBatching = globalRenderSettings.useDynamicBatching,
            enableInstancing = globalRenderSettings.useGPUInstancing
        };
        drawingSettings.SetShaderPassName(1, litShaderTagId);

        // 绘制不透明物体
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        context.DrawRenderers(
            cullingResults,
            ref drawingSettings,
            ref filteringSettings
        );

        // 绘制天空盒
        context.DrawSkybox(camera);

        // 绘制透明物体
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(
            cullingResults,
            ref drawingSettings,
            ref filteringSettings
        );


    }

    partial void DrawUnsupportedShaders();
    partial void DrawGizmos();
    public void Submit()
    {
        cmd.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }

    public void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    public bool Cull(float maxShadowDistance)
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }

}

