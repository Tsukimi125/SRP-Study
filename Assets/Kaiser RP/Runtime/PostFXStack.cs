using UnityEngine;
using UnityEngine.Rendering;

public partial class PostFXStack
{

    const string bufferName = "Post FX";

    CommandBuffer cmd = new CommandBuffer
    {
        name = bufferName
    };

    ScriptableRenderContext context;

    Camera camera;

    PostFXSettings settings;

    public bool IsActive => settings != null;

    enum Pass
    {
        BloomHorizontal,
        BloomVertical,
        BloomCombine,
        BloomPrefilter,
        Copy
    }

    int fxSourceId = Shader.PropertyToID("_PostFXSource");
    int fxSource2Id = Shader.PropertyToID("_PostFXSource2");
    int bloomPrefilterId = Shader.PropertyToID("_BloomPrefilter");
    int bloomThresholdId = Shader.PropertyToID("_BloomThreshold");
    int bloomIntensityId = Shader.PropertyToID("_BloomIntensity");


    #region bloom

    const int maxBloomPyramidLevels = 16;
    int bloomPyramidId;// = Shader.PropertyToID("_BloomPyramid");

    void DoBloom(int sourceId)
    {
        cmd.BeginSample("Bloom");

        PostFXSettings.BloomSettings bloom = settings.Bloom;
        int width = camera.pixelWidth / 2;
        int height = camera.pixelHeight / 2;



        if (
            bloom.maxIterations == 0 || bloom.intensity <= 0f ||
            height < bloom.downscaleLimit * 2 ||
            width < bloom.downscaleLimit * 2
        )
        {
            Draw(sourceId, BuiltinRenderTextureType.CameraTarget, Pass.Copy);
            cmd.EndSample("Bloom");
            return;
        }

        Vector4 threshold;
        threshold.x = Mathf.GammaToLinearSpace(bloom.threshold);
        threshold.y = -threshold.x + threshold.x * bloom.thresholdKnee;
        threshold.z = 2f * threshold.x * bloom.thresholdKnee;
        threshold.w = 0.25f / (threshold.y + threshold.x + 0.00001f);
        cmd.SetGlobalVector(bloomThresholdId, threshold);

        RenderTextureFormat format = RenderTextureFormat.Default;
        cmd.GetTemporaryRT(bloomPrefilterId, width, height, 0, FilterMode.Bilinear, format);
        Draw(sourceId, bloomPrefilterId, Pass.BloomPrefilter);
        width /= 2;
        height /= 2;
        int srcId = bloomPrefilterId;
        int dstId = bloomPyramidId + 1;
        int i;
        for (i = 0; i < bloom.maxIterations; i++)
        {
            if (width < bloom.downscaleLimit || height < bloom.downscaleLimit) break;

            int midId = dstId - 1;

            cmd.GetTemporaryRT(midId, width, height, 0, FilterMode.Bilinear, format);
            cmd.GetTemporaryRT(dstId, width, height, 0, FilterMode.Bilinear, format);

            Draw(srcId, midId, Pass.BloomHorizontal);
            Draw(midId, dstId, Pass.BloomVertical);

            srcId = dstId;
            dstId += 2;
            width /= 2;
            height /= 2;
        }
        cmd.ReleaseTemporaryRT(bloomPrefilterId);
        // Draw(srcId, BuiltinRenderTextureType.CameraTarget, Pass.Copy);
        // Draw(srcId, BuiltinRenderTextureType.CameraTarget, Pass.BloomVertical);
        cmd.SetGlobalFloat(bloomIntensityId, 1f);
        if (i > 1)
        {
            cmd.ReleaseTemporaryRT(srcId - 1);
            dstId -= 5;

            for (i -= 1; i > 0; i--)
            {
                cmd.SetGlobalTexture(fxSource2Id, dstId + 1);
                Draw(srcId, dstId, Pass.BloomCombine);
                cmd.ReleaseTemporaryRT(srcId);
                cmd.ReleaseTemporaryRT(dstId + 1);
                srcId = dstId;
                dstId -= 2;
            }
        }
        else
        {
            cmd.ReleaseTemporaryRT(bloomPyramidId);
        }
        cmd.SetGlobalFloat(bloomIntensityId, bloom.intensity);

        cmd.SetGlobalTexture(fxSource2Id, sourceId);
        Draw(bloomPyramidId, BuiltinRenderTextureType.CameraTarget, Pass.BloomCombine);
        cmd.ReleaseTemporaryRT(srcId);

        cmd.EndSample("Bloom");
    }

    #endregion

    public PostFXStack()
    {
        bloomPyramidId = Shader.PropertyToID("_BloomPyramid0");
        for (int i = 1; i < maxBloomPyramidLevels * 2; i++)
        {
            Shader.PropertyToID("_BloomPyramid" + i);
        }
    }

    public void Setup(
        ScriptableRenderContext context, Camera camera, PostFXSettings settings
    )
    {
        this.context = context;
        this.camera = camera;
        this.settings = camera.cameraType <= CameraType.SceneView ? settings : null;
        ApplySceneViewState();
    }

    public void Render(int sourceId)
    {
        // if (!IsActive) return;
        DoBloom(sourceId);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    void Draw(RenderTargetIdentifier source, RenderTargetIdentifier destination, Pass pass)
    {
        cmd.SetGlobalTexture(fxSourceId, source);
        cmd.SetRenderTarget(destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        cmd.DrawProcedural(Matrix4x4.identity, settings.Material, (int)pass, MeshTopology.Triangles, 3);
    }

    partial void ApplySceneViewState();
}