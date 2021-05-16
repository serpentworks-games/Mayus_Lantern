using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class Decal : MonoBehaviour
{
    public Material m_Material;
    public Texture2D texture;
    public Color tinting = Color.white;
  //  Camera cam;
    public Mesh m_CubeMesh;
    MaterialPropertyBlock props;

    // private void Start()
    // {
    //     cam = Camera.main;
    // }
    public void OnEnable()
    {
        props = new MaterialPropertyBlock();
  Camera.onPreCull -= DrawWithCamera;
        Camera.onPreCull += DrawWithCamera;
    }

#if UNITY_EDITOR
    private void DrawGizmo(bool selected)
    {
        var col = new Color(0.0f, 0.7f, 1f, 1.0f);
        col.a = selected ? 0.3f : 0.1f;
        Gizmos.color = col;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        col.a = selected ? 0.5f : 0.2f;
        Gizmos.color = col;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        Handles.matrix = transform.localToWorldMatrix;
        Handles.DrawBezier(Vector3.zero, Vector3.down, Vector3.zero, Vector3.down, Color.red, null, selected ? 4f : 2f);
    }
#endif
    // void LateUpdate()
    // {
      
    //     // set up property block for decal texture and tinting
    //     if (texture != null)
    //     {
    //         props.SetTexture("_MainTex", texture);
    //     }
    //     props.SetColor("_Tint", tinting);
    //     // draw the decal on the main camera
    //     Graphics.DrawMesh(m_CubeMesh, transform.localToWorldMatrix, m_Material, 0, cam, 0, props, false, true, false);
    // }

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        DrawGizmo(false);
    }
    public void OnDrawGizmosSelected()
    {
        DrawGizmo(true);
    }
#endif

 
    private void OnDisable() {
 
        Camera.onPreCull -= DrawWithCamera;
    }
 
    private void DrawWithCamera(Camera camera) {
 
        if (camera) {
 
            Draw(camera);
        }
    }
 
    private void Draw(Camera camera) {
 
       
 
  // set up property block for decal texture and tinting
        if (texture != null)
        {
            props.SetTexture("_MainTex", texture);
        }
        props.SetColor("_Tint", tinting);
        // draw the decal on the main camera
        Graphics.DrawMesh(m_CubeMesh, transform.localToWorldMatrix, m_Material, 0, camera, 0, props, false, true, false);
           // Graphics.DrawMesh(mesh, matrix, material, gameObject.layer, camera);
        
    }
}
