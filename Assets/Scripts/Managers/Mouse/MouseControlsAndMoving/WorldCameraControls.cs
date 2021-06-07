using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;
using UnityEngine.Events;

namespace HOMM_BM
{
    [Serializable]
    public class MoveInputEvent : UnityEvent<float, float> { }
    public class WorldCameraControls : MonoBehaviour
    {
        MouseControls cameraControls;
        public MoveInputEvent moveInputEvent;

        [SerializeField]
        private float panSpeed = 2f;
        [SerializeField]
        private float zoomSpeed = 3f;
        [SerializeField]
        private float zoomInMax = 15f;
        [SerializeField]
        private float zoomOutMax = 40f;

        //Camera properties
        public float standardSpeed;
        public float increasedSpeed;
        float speed;

        public float time;

        Vector3 newPosition;
        Quaternion newRotation;

        Vector3 rotateStartPosition;
        Vector3 rotateCurrentPosition;

        Vector3 dragStartPosition;
        Vector3 dragCurrentPosition;

        float horizontal;
        float vertical;

        private CinemachineInputProvider inputProvider;
        private CinemachineVirtualCamera virtualCamera;

        private Transform cameraTransform;

        private void Awake()
        {
            cameraControls = new MouseControls();

            inputProvider = GetComponentInChildren<CinemachineInputProvider>();
            virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();

            cameraTransform = virtualCamera.VirtualCameraGameObject.transform;
        }
        private void Start()
        {
            newPosition = transform.position;
            newRotation = transform.rotation;
        }
        private void LateUpdate()
        {
            float xAxis = inputProvider.GetAxisValue(0);
            float zAxis = inputProvider.GetAxisValue(1);
            float yAxis = inputProvider.GetAxisValue(2);

            if (GameManager.instance.IsMouseOverGameWindow)
            {
                if (xAxis != 0 || zAxis != 0)
                {
                    PanScreen(xAxis, zAxis);
                }

                if (yAxis != 0)
                {
                    ZoomScreen(yAxis);
                }
                HandleCameraMovement();
                HandleCameraRotation();
                HandleMouseDrag();
            }

            transform.position = Vector3.Lerp(transform.position, newPosition, time * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, time * Time.deltaTime);
        }

        #region New Input system, wasd movement
        private void OnEnable()
        {
            cameraControls.Keyboard.Enable();

            cameraControls.Keyboard.Move.performed += OnMove;
            cameraControls.Keyboard.Move.canceled += OnMove;
        }
        private void OnMove(InputAction.CallbackContext context)
        {
            Vector2 moveInput = context.ReadValue<Vector2>();
            moveInputEvent.Invoke(moveInput.x, moveInput.y);
        }
        public void OnMoveInput(float horizontal, float vertical)
        {
            this.horizontal = horizontal;
            this.vertical = vertical;
        }
        void HandleCameraMovement()
        {
            if (GameManager.instance.Keyboard.shiftKey.isPressed)
            {
                speed = increasedSpeed;
            }
            else
            {
                speed = standardSpeed;
            }
            newPosition += Vector3.forward * speed * vertical + Vector3.right * speed * horizontal;
        }
        #endregion

        #region DragAndRotate
        void HandleCameraRotation()
        {
            if (GameManager.instance.Mouse.rightButton.wasPressedThisFrame)
            {
                rotateStartPosition = GameManager.instance.Mouse.position.ReadValue();
            }
            if (GameManager.instance.Mouse.rightButton.isPressed)
            {
                rotateCurrentPosition = GameManager.instance.Mouse.position.ReadValue();

                Vector3 difference = rotateStartPosition - rotateCurrentPosition;
                rotateStartPosition = rotateCurrentPosition;

                newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 10f));
            }
        }
        void HandleMouseDrag()
        {
            if (GameManager.instance.Mouse.middleButton.wasPressedThisFrame)
            {
                Plane plane = new Plane(Vector3.up, Vector3.zero);

                Ray ray = WorldManager.instance.MainCamera.ScreenPointToRay(GameManager.instance.Mouse.position.ReadValue());

                float raycastEntryPoint;

                if (plane.Raycast(ray, out raycastEntryPoint))
                {
                    dragStartPosition = ray.GetPoint(raycastEntryPoint);
                }
            }

            if (GameManager.instance.Mouse.middleButton.isPressed)
            {
                Plane plane = new Plane(Vector3.up, Vector3.zero);

                Ray ray = WorldManager.instance.MainCamera.ScreenPointToRay(GameManager.instance.Mouse.position.ReadValue());

                float raycastEntryPoint;

                if (plane.Raycast(ray, out raycastEntryPoint))
                {
                    dragCurrentPosition = ray.GetPoint(raycastEntryPoint);

                    newPosition = transform.position + dragStartPosition - dragCurrentPosition;
                }
            }
        }
        #endregion

        #region PanAndZoom
        void ZoomScreen(float increment)
        {
            float fieldOfView = virtualCamera.m_Lens.FieldOfView;
            float targetZoom = Mathf.Clamp(fieldOfView + increment, zoomInMax, zoomOutMax);

            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(fieldOfView, targetZoom, zoomSpeed * Time.deltaTime);
        }
        Vector3 PanDirection(float xAxis, float zAxis)
        {
            Vector3 direction = Vector3.zero;
            direction.y = 0;

            if (zAxis >= Screen.height * .95f)
            {
                direction = transform.forward;
            }
            else if (zAxis <= Screen.height * 0.05f)
            {
                direction = -transform.forward;
            }

            if (xAxis >= Screen.width * .95f)
            {
                direction = transform.right;
            }
            else if (xAxis <= Screen.width * 0.05f)
            {
                direction = -transform.right;
            }

            return direction;
        }
        void PanScreen(float xAxis, float zAxis)
        {
            Vector3 direction = PanDirection(xAxis, zAxis);

            cameraTransform.position = Vector3.Lerp(cameraTransform.position,
                cameraTransform.position + direction, panSpeed * Time.deltaTime);
        }

        #endregion
    }
}