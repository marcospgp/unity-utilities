using UnityUtilities;
using UnityEngine;

namespace MarcosPereira
{
    [RequireComponent(typeof(Collider))]
    public class FogZone : MonoBehaviour
    {
        [SerializeField]
        [TagSelect]
        private string playerCameraTag;

        [SerializeField]
        [Tooltip(
            "Optionally hide some objects when the fog kicks in, such as "
                + "hiding the water surface when entering underwater mode."
        )]
        private Renderer[] objectsToHide;

        [SerializeField]
        private Color color;

        [SerializeField]
        private float density;

        [SerializeField]
        private FogMode mode;

        private FogSettings previousFog;

        private bool[] objectsHidden;

        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(this.playerCameraTag))
            {
                this.OnEnter();
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(this.playerCameraTag))
            {
                this.OnLeave();
            }
        }

        private void OnEnter()
        {
            this.objectsHidden = new bool[this.objectsToHide.Length];

            for (int i = 0; i < this.objectsToHide.Length; i++)
            {
                var renderer = this.objectsToHide[i];

                if (renderer.enabled)
                {
                    renderer.enabled = false;
                    this.objectsHidden[i] = true;
                }
            }

            this.previousFog = new FogSettings()
            {
                enabled = RenderSettings.fog,
                color = RenderSettings.fogColor,
                density = RenderSettings.fogDensity,
                mode = RenderSettings.fogMode,
                startDistance = RenderSettings.fogStartDistance,
                endDistance = RenderSettings.fogEndDistance
            };

            RenderSettings.fog = true;
            RenderSettings.fogColor = this.color;
            RenderSettings.fogDensity = this.density;
            RenderSettings.fogMode = this.mode;
        }

        private void OnLeave()
        {
            for (int i = 0; i < this.objectsToHide.Length; i++)
            {
                var renderer = this.objectsToHide[i];

                if (this.objectsHidden[i])
                {
                    renderer.enabled = true;
                }
            }

            this.objectsHidden = null;

            this.previousFog.Restore();
        }

        private struct FogSettings
        {
            public bool enabled;
            public Color color;
            public float density;
            public FogMode mode;
            public float startDistance;
            public float endDistance;

            public void Restore()
            {
                RenderSettings.fog = this.enabled;
                RenderSettings.fogColor = this.color;
                RenderSettings.fogDensity = this.density;
                RenderSettings.fogMode = this.mode;
                RenderSettings.fogStartDistance = this.startDistance;
                RenderSettings.fogEndDistance = this.endDistance;
            }
        }
    }
}
