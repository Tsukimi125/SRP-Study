using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    const string bufferName = "Lighting";
    CommandBuffer cmd = new CommandBuffer
    {
        name = bufferName
    };
    Shadows shadows = new Shadows();


    #region Directional Light
    const int maxDirLightCount = 4;
    static int
        dirLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
        dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors"),
        dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections"),
        dirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData");

    static Vector4[]
        dirLightColors = new Vector4[maxDirLightCount],
        dirLightDirections = new Vector4[maxDirLightCount],
        dirLightShadowData = new Vector4[maxDirLightCount];
    #endregion

    #region Main Light
    static int
        mainLightIndexId = Shader.PropertyToID("_MainLightIndex"),
        mainLightPositionId = Shader.PropertyToID("_MainLightPosition"),
        mainLightColorId = Shader.PropertyToID("_MainLightColor");
    static VisibleLight[] dirVisibleLights = new VisibleLight[maxDirLightCount];
    static int mainLightIndex = -1;
    static Vector4 mainLightPosition = new Vector4(0.0f, 0.0f, 1.0f, 0.0f);
    public Vector4 MainLightPosition
    {
        get
        {
            return mainLightPosition;
        }
    }
    static Color mainLightColor = Color.black;
    #endregion

    CullingResults cullingResults;
    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        this.cullingResults = cullingResults;
        cmd.BeginSample(bufferName);

        // 配置阴影
        shadows.Setup(context, cullingResults, shadowSettings);

        // 配置光照
        SetupLights();

        // 渲染阴影
        shadows.Render();
        cmd.EndSample(bufferName);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    void SetupLights()
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        int dirLightCount = 0;
        for (int i = 0; i < visibleLights.Length; i++)
        {
            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType == LightType.Directional)
            {
                SetupDirectionalLight(dirLightCount++, ref visibleLight);
                if (dirLightCount >= maxDirLightCount)
                {
                    break;
                }
            }
        }

        cmd.SetGlobalInt(dirLightCountId, dirLightCount);
        cmd.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
        cmd.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
        cmd.SetGlobalVectorArray(dirLightShadowDataId, dirLightShadowData);
    }

    public void SetupMainLight()
    {
        Light light = RenderSettings.sun;

        cmd.SetGlobalVector(mainLightPositionId, -light.transform.forward);
        cmd.SetGlobalVector(mainLightColorId, light.color.linear * light.intensity);

    }

    void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
    {
        // 配置方向光
        dirLightColors[index] = visibleLight.finalColor;
        dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
        dirLightShadowData[index] = shadows.ReserveDirectionalShadows(visibleLight.light, index); // 配置方向光的阴影
    }

    public void Cleapup()
    {
        shadows.Cleapup();
    }
}
