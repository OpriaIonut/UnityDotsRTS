using UnityEngine;

namespace DotsOverview
{
    public class ShootPopup : MonoBehaviour
    {
        [SerializeField] private float destroyTimer = 1f;
        [SerializeField] private float speed = 2f;

        private void Update()
        {
            transform.position += Vector3.up * speed * Time.deltaTime;

            destroyTimer -= Time.deltaTime;
            if (destroyTimer < 0f)
                Destroy(gameObject);
        }
    }
}