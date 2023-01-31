using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalRenderSettings
{
    public bool useSRPBatcher = true;
    public bool useDynamicBatching = true;
    public bool useGPUInstancing = true;
    public ShadowSettings shadowSettings;
    public PostFXSettings postFXSettings;

    public GlobalRenderSettings(
        bool useSRPBatcher,
        bool useDynamicBatching,
        bool useGPUInstancing,
        ShadowSettings shadowSettings,
        PostFXSettings postFXSettings
    )
    {
        this.useSRPBatcher = useSRPBatcher;
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        this.shadowSettings = shadowSettings;
        this.postFXSettings = postFXSettings;
    }
}
