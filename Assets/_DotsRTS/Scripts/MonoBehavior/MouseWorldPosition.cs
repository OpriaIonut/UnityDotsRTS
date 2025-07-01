using UnityEngine;

namespace DotsRTS
{
    public class MouseWorldPosition : MonoBehaviour
    {
        #region Singleton
        public static MouseWorldPosition Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("More than one singleton of type MouseWorldPosition in the scene; deleting from: " + gameObject.name);
                Destroy(this);
            }
            else
                Instance = this;
        }
        #endregion

        public Vector3 GetPosition()
        {
            Ray mouseCameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            if(plane.Raycast(mouseCameraRay, out float distance))
                return mouseCameraRay.GetPoint(distance);
            return Vector3.zero;
        }
    }
}