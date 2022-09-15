using UnityEngine;

namespace MarcosPereira.UnityUtilities {
    public class MaxFPS : MonoBehaviour {
        [SerializeField]
        private int targetFramerate = 120;

        // Start is called before the first frame update
        public void Start() {
            Application.targetFrameRate = this.targetFramerate;
        }
    }
}
