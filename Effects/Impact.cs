using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarcosPereira.UnityUtilities {
    [RequireComponent(typeof(Collider))]
    public class Impact : MonoBehaviour {
        [SerializeField]
        [Range(0f, 1f)]
        private float maxVolume = 0.5f;

        [SerializeField]
        [Range(0f, 10f)]
        [Tooltip(
            "The collision relative speed at which the sound is played at max volume (defined " +
            "above)."
        )]
        private float maxVolumeSpeed = 10f;

        [SerializeField]
        [Tooltip("Particle systems to play upon collision.")]
        private ParticleSystem[] particles;

        [SerializeField]
        [Tooltip("Sounds to play upon impact. Only one is played each time, picked randomly.")]
        private AudioClip[] audioClips;

        public void OnCollisionEnter(Collision collision) {
            float relativeSpeed = collision.relativeVelocity.magnitude;

            float ratio = Mathf.Clamp01(relativeSpeed / this.maxVolumeSpeed);

            float volume = ratio * this.maxVolume;

            // TODO: continue
        }
    }
}
