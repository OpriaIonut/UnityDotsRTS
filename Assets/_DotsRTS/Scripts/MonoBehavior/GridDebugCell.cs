using UnityEngine;

namespace DotsRTS
{
    public class GridDebugCell : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer rend;

        private int x;
        private int y;

        public void Setup(int x, int y, float gridNodeSize)
        {
            this.x = x;
            this.y = y;

            transform.position = GridSystem.GetWorldPosition(x, y, gridNodeSize);
        }

        public void SetColor(Color col)
        {
            rend.color = col;
        }
    }
}