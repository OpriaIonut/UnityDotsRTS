using UnityEngine;

namespace DotsRTS
{
    public class FogOfWarPersistent : MonoBehaviour
    {
        [SerializeField] private RenderTexture fow;
        [SerializeField] private RenderTexture fowPersistent;
        [SerializeField] private RenderTexture fowPersistent2;
        [SerializeField] private Material blitMat;

        private void Start()
        {
            Graphics.Blit(fow, fowPersistent);
            Graphics.Blit(fow, fowPersistent2);
        }

        private void Update()
        {
            Graphics.Blit(fow, fowPersistent, blitMat, 0);
            Graphics.CopyTexture(fowPersistent, fowPersistent2);
        }
    }
}