using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderDebug : MonoBehaviour
{
    [SerializeField]
    Material shaderMat;
    // Start is called before the first frame update
    void Start()
    {
        shaderMat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        shaderMat.SetFloat("_DisAmount", Mathf.PingPong(Time.time / 2, 4) - 2);
    }
}