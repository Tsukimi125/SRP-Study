
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class KaiserDeferredRenderer
{
    ScriptableRenderContext context;
    Camera camera;
    const string bufferName = "Kaiser Deferred Render Camera";
    string SampleName { get; set; }
    CommandBuffer cmd = new CommandBuffer
    {
        name = bufferName
    };

    CullingResults cullingResults;
    static ShaderTagId gbufferShaderTagId = new ShaderTagId("KRPgbuffer");
    static ShaderTagId lightPassShaderTagId = new ShaderTagId("KRPlightpass");

    Vector2Int bufferSize;
    RenderTexture gdepth;
    RenderTexture[] gbuffers = new RenderTexture[4];
    RenderTargetIdentifier[] gbuffersID = new RenderTargetIdentifier[4];
    Lighting lighting = new Lighting();

    public KaiserDeferredRenderer()
    {

        // bufferSize.x = camera.pixelWidth;
        // bufferSize.y = camera.pixelHeight;
        bufferSize.x = Screen.width;
        bufferSize.y = Screen.height;

        gdepth = new RenderTexture(bufferSize.x, bufferSize.y, 24, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
        gbuffers[0] = new RenderTexture(bufferSize.x, bufferSize.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        gbuffers[1] = new RenderTexture(bufferSize.x, bufferSize.y, 0, RenderTextureFormat.ARGB2101010, RenderTextureReadWrite.Linear);
        gbuffers[2] = new RenderTexture(bufferSize.x, bufferSize.y, 0, RenderTextureFormat.ARGB64, RenderTextureReadWrite.Linear);
        gbuffers[3] = new RenderTexture(bufferSize.x, bufferSize.y, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);


    }

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;

        for (int i = 0; i < 4; i++)
        {
            gbuffersID[i] = gbuffers[i];
        }

        if (!Cull()) return;

        PrepareBuffer();
        PrepareForSceneWindow();
        cmd.name = "Kaiser G-Buffer Pass";

        #region G-Buffer Pass

        context.SetupCameraProperties(camera);
        cmd.SetGlobalTexture("_gdepth", gdepth);
        for (int i = 0; i < 4; i++)
        {
            cmd.SetGlobalTexture("_GT" + i, gbuffers[i]);
        }
        cmd.SetRenderTarget(gbuffersID, gdepth);
        cmd.ClearRenderTarget(true, true, Color.black);
        ExecuteBuffer();

        cmd.BeginSample(SampleName);
        ExecuteBuffer();

        // Culling
        camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParameters);
        var cullingResults = context.Cull(ref cullingParameters);

        // Config settings
        SortingSettings sortingSettings = new SortingSettings(camera);
        DrawingSettings drawingSettings = new DrawingSettings(gbufferShaderTagId, sortingSettings);
        FilteringSettings filteringSettings = FilteringSettings.defaultValue;

        // Draw
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        context.DrawSkybox(camera);
        DrawUnsupportedShaders();
        DrawGizmos();
        cmd.EndSample(SampleName);
        ExecuteBuffer();
        # endregion

        # region Light Pass

        cmd = new CommandBuffer() { name = "Kaiser Light Pass" };
        // lighting.Setup(context, cullingResults);

        Material mat = new Material(Shader.Find("Kaiser RP/LightPass"));
        cmd.Blit(gbuffersID[0], BuiltinRenderTextureType.CameraTarget, mat);
        ExecuteBuffer();

        #endregion
        context.Submit();

    }
    partial void PrepareBuffer();
    partial void PrepareForSceneWindow();

    partial void DrawUnsupportedShaders();
    partial void DrawGizmos();

    public void GBufferPass()
    {

    }

    public void LightPass()
    {

    }


    public void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    public bool Cull()
    {
        ScriptableCullingParameters p;
        if (camera.TryGetCullingParameters(out p))
        {
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;

    }

}