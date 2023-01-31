using UnityEngine;
using UnityEngine.Rendering;

public class PostFXStack
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

    public void Setup(
        ScriptableRenderContext context, Camera camera, PostFXSettings settings
    )
    {
        this.context = context;
        this.camera = camera;
        this.settings = settings;
    }

    public void Render(int sourceId)
    {
        // if (!IsActive) return;
        cmd.Blit(sourceId, BuiltinRenderTextureType.CameraTarget);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }
}