namespace ML.Core
{
    using System;
    using Cinemachine;
    using UnityEngine;
    using ML.Characters.Player;

    public class CameraSettings : MonoBehaviour
    {
        public enum InputChoice
        {
            KeyboardAndMouse, Controller,
        }

        public Transform follow, lookAt;
        public CinemachineVirtualCamera keyboardAndMouseCamera, controllerCamera;
        public Vector3 followOffSet;
        public float fieldOfView;

        public InputChoice inputChoice;
        public bool allowRuntimeCameraSettingChanges;

        public CinemachineVirtualCamera Current
        {
            get
            {
                return inputChoice == InputChoice.KeyboardAndMouse ? keyboardAndMouseCamera : controllerCamera;
            }
        }

        private void Reset()
        {
            Transform keyboardAndMouseCameraTransform = transform.Find("KeyboardAndMouseRig");
            if (keyboardAndMouseCameraTransform != null)
            {
                keyboardAndMouseCamera = keyboardAndMouseCameraTransform.GetComponent<CinemachineVirtualCamera>();
            }

            Transform controllerCameraTranform = transform.Find("ControllerRig");
            if (controllerCameraTranform != null)
            {
                controllerCamera = controllerCameraTranform.GetComponent<CinemachineVirtualCamera>();
            }

            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                follow = player.transform;

                lookAt = follow.Find("HeadTarget");

                if (player.cameraSettings == null)
                    player.cameraSettings = this;
            }
        }

        private void Awake()
        {
            UpdateCameraSettings();
        }

        private void Update()
        {
            if (allowRuntimeCameraSettingChanges)
                UpdateCameraSettings();
        }

        void UpdateCameraSettings()
        {
            keyboardAndMouseCamera.Follow = follow;
            keyboardAndMouseCamera.LookAt = lookAt;
            keyboardAndMouseCamera.m_Lens.FieldOfView = fieldOfView;
            keyboardAndMouseCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = followOffSet;

            controllerCamera.Follow = follow;
            controllerCamera.LookAt = lookAt;
            controllerCamera.m_Lens.FieldOfView = fieldOfView;
            controllerCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = followOffSet;

            keyboardAndMouseCamera.Priority = inputChoice == InputChoice.KeyboardAndMouse ? 1 : 0;
            controllerCamera.Priority = inputChoice == InputChoice.Controller ? 1 : 0;
        }
    }
}