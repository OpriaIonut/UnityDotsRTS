using UnityEngine;

namespace DotsRTS
{
    public class FogOfWarPersistent : MonoBehaviour
    {
        [SerializeField] private RenderTexture fow;
        [SerializeField] private RenderTexture fowPersistent;
        [SerializeField] private RenderTexture fowPersistent2;
        [SerializeField] private Material blitMat;

        private bool isInit = false;
        private int counter = 0;

        private void Update()
        {
            if (!isInit)
                return;

            Graphics.Blit(fow, fowPersistent, blitMat, 0);
            Graphics.CopyTexture(fowPersistent, fowPersistent2);
        }

        private void LateUpdate()
        {
            if (isInit)
                return;

            counter++;
            if (counter < 5)
                return;

            isInit = true;
            Graphics.Blit(fow, fowPersistent);
            Graphics.Blit(fow, fowPersistent2);
        }
    }
}