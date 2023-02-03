using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{

    const string bufferName = "Shadows";

    CommandBuffer cmd = new CommandBuffer
    {
        name = bufferName
    };

    ScriptableRenderContext context;

    CullingResults cullingResults;

    ShadowSettings settings;


    // 阴影参数
    # region Shadow Parameters
    const int maxShadowedDirectionalLightCount = 4,
        maxCascades = 4;

    int ShadowedDirectionalLightCount;
    struct ShadowedDirectionalLight
    {
        public int visibleLightIndex;
        public float slopeScaleBias;
        public float nearPlaneOffset;
    }

    ShadowedDirectionalLight[] ShadowedDirectionalLights =
        new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

    static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas"),
        dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices"),
        cascadeCountId = Shader.PropertyToID("_CascadeCount"),
        cascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres"),
        cascadeDataId = Shader.PropertyToID("_CascadeData"),
        shadowDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade"),
        debugCascadeId = Shader.PropertyToID("_DebugCascade");

    // 阴影split数量：一个shadowmap可以split成4个，最多4个光源的shadowmap，最多有16个
    static Matrix4x4[] dirShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount * maxCascades];

    // 阴影裁剪球
    static Vector4[] cascadeCullingSpheres = new Vector4[maxCascades],
        cascadeData = new Vector4[maxCascades];

    #endregion


    public void Setup(
        ScriptableRenderContext context, CullingResults cullingResults,
        ShadowSettings settings
    )
    {
        this.context = context;
        this.cullingResults = cullingResults;
        this.settings = settings;
        ShadowedDirectionalLightCount = 0;
    }

    public Vector3 ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
        if (
            ShadowedDirectionalLightCount < maxShadowedDirectionalLightCount &&
            light.shadows != LightShadows.None && light.shadowStrength > 0f &&
            cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b)
        )
        {
            ShadowedDirectionalLights[ShadowedDirectionalLightCount] =
                new ShadowedDirectionalLight
                {
                    visibleLightIndex = visibleLightIndex,
                    slopeScaleBias = light.shadowBias,
                    nearPlaneOffset = light.shadowNearPlane
                };
            return new Vector3(
                light.shadowStrength,
                settings.directional.cascadeCount * ShadowedDirectionalLightCount++,
                light.shadowNormalBias
            );
        }
        return Vector3.zero;
    }

    public void Render()
    {
        if (ShadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
        else
        {
            // cmd.GetTemporaryRT(
            //     dirShadowAtlasId, 1, 1,
            //     32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap
            // );
            // cmd.SetRenderTarget(
            //     dirShadowAtlasId,
            //     RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
            // );
            // cmd.ClearRenderTarget(true, false, Color.clear);
            // ExecuteBuffer();
        }
    }

    // 渲染方向光阴影
    void RenderDirectionalShadows()
    {
        // 获取阴影Atlas的大小
        int atlasSize = (int)settings.directional.atlasSize;

        // 创建阴影RenderTexture并设置其为渲染目标
        cmd.GetTemporaryRT(
            dirShadowAtlasId, atlasSize, atlasSize,
            32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap
        );
        cmd.SetRenderTarget(
                dirShadowAtlasId,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
            );
        cmd.ClearRenderTarget(true, false, Color.clear);

        cmd.BeginSample(bufferName);
        ExecuteBuffer();

        // 判断是否需要分割ShadowMap
        int tiles = ShadowedDirectionalLightCount * settings.directional.cascadeCount;
        int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;

        int tileSize = atlasSize / split;

        // 渲染每个方向光的阴影
        for (int i = 0; i < ShadowedDirectionalLightCount; i++)
        {
            RenderDirectionalShadows(i, split, tileSize);
        }

        if (settings.directional.debugCascade)
        {
            cmd.SetGlobalInt(debugCascadeId, 1);
        }
        else
        {
            cmd.SetGlobalInt(debugCascadeId, 0);
        }

        // 将级联数量和包围球数据发送到GPU着色器
        cmd.SetGlobalInt(cascadeCountId, settings.directional.cascadeCount);

        cmd.SetGlobalVectorArray(cascadeCullingSpheresId, cascadeCullingSpheres);
        cmd.SetGlobalVectorArray(cascadeDataId, cascadeData);


        // 设置阴影矩阵
        cmd.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);

        // 设置阴影最大距离和过渡
        float oneMinusCascadeFade = 1f - settings.directional.cascadeFade;
        cmd.SetGlobalVector(shadowDistanceFadeId,
            new Vector4(
                1f / settings.maxDistance,
                1f / settings.distanceFade,
                1f / (1f - oneMinusCascadeFade * oneMinusCascadeFade)
            )
        );

        cmd.EndSample(bufferName);
        ExecuteBuffer();
    }

    // 调整渲染视口来渲染单个图块
    Vector2 SetTileViewport(int index, int split, float tileSize)
    {
        Vector2 offset = new Vector2(index % split, index / split);
        cmd.SetViewport(new Rect(
            offset.x * tileSize, offset.y * tileSize, tileSize, tileSize
        ));

        return offset;
    }

    Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
    {
        if (SystemInfo.usesReversedZBuffer)
        {
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;
        }

        float scale = 1f / split;
        m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
        m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
        m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
        m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
        m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
        m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
        m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
        m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
        m.m20 = 0.5f * (m.m20 + m.m30);
        m.m21 = 0.5f * (m.m21 + m.m31);
        m.m22 = 0.5f * (m.m22 + m.m32);
        m.m23 = 0.5f * (m.m23 + m.m33);

        return m;
    }

    void SetCascadeData(int index, Vector4 cullingSphere, float tileSize)
    {
        float texelSize = 2f * cullingSphere.w / tileSize;

        cullingSphere.w *= cullingSphere.w;
        cascadeCullingSpheres[index] = cullingSphere;
        cascadeData[index] = new Vector4(
            1f / cullingSphere.w,
            texelSize * 1.4142136f
        );
    }

    void RenderDirectionalShadows(int index, int split, int tileSize)
    {
        ShadowedDirectionalLight light = ShadowedDirectionalLights[index];

        // 设置阴影相机为正交投影
        BatchCullingProjectionType projectionType = BatchCullingProjectionType.Orthographic;

        // 设置阴影相机的绘制设置
        var shadowDrawingSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex, projectionType);


        int cascadeCount = settings.directional.cascadeCount;
        int tileOffset = index * cascadeCount;
        Vector3 cascadeRatios = settings.directional.CascadeRatios;

        for (int i = 0; i < cascadeCount; i++)
        {
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                light.visibleLightIndex, i, cascadeCount, cascadeRatios, tileSize, light.nearPlaneOffset,
                out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
                out ShadowSplitData splitData
            );

            // 设置阴影相机的裁剪球。因为所有的阴影相机都是同一个设置，所以只需要设置一次
            if (index == 0)
            {
                SetCascadeData(i, splitData.cullingSphere, tileSize);
            }

            Vector4 cullingSphere = splitData.cullingSphere;
            // w是半径，平方得到半径的平方值
            cullingSphere.w *= cullingSphere.w;
            cascadeCullingSpheres[i] = cullingSphere;

            // 设置阴影相机的split
            shadowDrawingSettings.splitData = splitData;

            // 设置tile的索引
            int tileIndex = tileOffset + i;

            // 设置阴影相机的视图投影矩阵
            dirShadowMatrices[tileIndex] = ConvertToAtlasMatrix(
                projectionMatrix * viewMatrix,
                SetTileViewport(tileIndex, split, tileSize),
                split
            );

            // 设置阴影相机的视图投影矩阵
            cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

            cmd.SetGlobalDepthBias(0, light.slopeScaleBias);

            // 绘制阴影
            ExecuteBuffer();
            context.DrawShadows(ref shadowDrawingSettings);
            cmd.SetGlobalDepthBias(0f, 0f);
        }
    }

    // 释放阴影RenderTexture内存
    public void Cleanup()
    {
        cmd.ReleaseTemporaryRT(dirShadowAtlasId);
        ExecuteBuffer();
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }
}