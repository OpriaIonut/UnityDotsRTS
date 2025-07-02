using Unity.Cinemachine;
using UnityEngine;

namespace DotsRTS
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 30.0f;
        [SerializeField] private float rotationSpeed = 200.0f;
        [SerializeField] private float zoomSpeed = 4f;
        [SerializeField] private float zoomSpeedLerp = 10f;
        [SerializeField] private float fovMin = 20f;
        [SerializeField] private float fovMax = 60f;
        [SerializeField] private CinemachineCamera camera;

        private float targetFOV;

        private void Awake()
        {
            targetFOV = camera.Lens.FieldOfView;
        }

        private void Update()
        {
            Vector3 moveDir = Vector3.zero;
            float rotationAmount = 0f;

            moveDir.x = Input.GetAxisRaw("Horizontal");
            moveDir.z = Input.GetAxisRaw("Vertical");

            Transform cam = Camera.main.transform;
            moveDir = cam.forward * moveDir.z + cam.right * moveDir.x;
            moveDir.y = 0f;
            moveDir.Normalize();

            if (Input.GetKey(KeyCode.Q))
                rotationAmount = 1f;
            else if (Input.GetKey(KeyCode.E))
                rotationAmount = -1f;

            transform.position += moveDir * moveSpeed * Time.deltaTime;
            transform.eulerAngles += Vector3.up * rotationAmount * rotationSpeed * Time.deltaTime;

            if (Input.mouseScrollDelta.y > 0)
                targetFOV -= zoomSpeed;
            if(Input.mouseScrollDelta.y < 0)
                targetFOV += zoomSpeed;

            targetFOV = Mathf.Clamp(targetFOV, fovMin, fovMax);
            camera.Lens.FieldOfView = Mathf.Lerp(camera.Lens.FieldOfView, targetFOV, zoomSpeedLerp * Time.deltaTime);
        }
    }
}