using UnityEngine;
using Cinemachine;

public class CameraSettings_NEW : MonoBehaviour
{
    public Transform follow, lookAt;
    public CinemachineVirtualCamera cameraVC;
    public Vector3 followOffSet;
    public float fieldOfView;

    private void Reset()
    {
        Transform camTransform = transform.Find("CamRig");
        if (camTransform != null) cameraVC = camTransform.GetComponent<CinemachineVirtualCamera>();

        PlayerController_NEW player = FindObjectOfType<PlayerController_NEW>();
        if (player != null)
        {
            follow = player.transform;
            lookAt = follow.Find("HeadTarget");

            if (player.cameraSettings == null) player.cameraSettings = this;
        }
    }

    private void Awake()
    {
        cameraVC.Follow = follow;
        cameraVC.LookAt = lookAt;
        cameraVC.m_Lens.FieldOfView = fieldOfView;
        cameraVC.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = followOffSet;
    }
}

