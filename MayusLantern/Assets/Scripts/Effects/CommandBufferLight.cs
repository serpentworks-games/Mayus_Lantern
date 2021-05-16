using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class CommandBufferLight : MonoBehaviour
{
    CommandBuffer cmd;
    Light lightC;
    public string shadowTexName = "_DirectionalShadowMap";
    // Start is called before the first frame update
    void Start()
    {
        lightC = GetComponent<Light>();
        cmd = new CommandBuffer();
        cmd.name = "Shadows";
        cmd.SetGlobalTexture(shadowTexName, BuiltinRenderTextureType.CurrentActive);
        lightC.AddCommandBuffer(LightEvent.AfterScreenspaceMask, cmd);
    }
}
