using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Post FX Settings")]
public class PostFXSettings : ScriptableObject
{
    [SerializeField]
    Shader shader = default;

    [System.NonSerialized]
    Material material;

    public Material Material
    {
        get
        {
            if (material == null && shader != null)
            {
                material = new Material(shader);
                material.hideFlags = HideFlags.HideAndDontSave;
            }
            return material;
        }
    }

    [System.Serializable]
    public struct BloomSettings
    {
        [Range(0f, 16f)]
        public int maxIterations;
        [Min(1f)]
        public int downscaleLimit;
        [Min(0f)]
        public float threshold;
        [Range(0f, 1f)]
        public float thresholdKnee;
        [Range(0f, 8f)]
        public float intensity;
        [Range(0f, 8f)]
        public float scatter;
    }
    [SerializeField]
    BloomSettings bloom = default;

    public BloomSettings Bloom => bloom;
}
