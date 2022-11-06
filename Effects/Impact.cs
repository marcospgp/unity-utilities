using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// Plays 3D audio upon collision. Sound is always 3D to make multiplayer simpler.
// Creates its own audio source.
// Requires having a collider on the same GameObject as this.
// A rigidbody is also needed if one wants collisions with non-trigger colliders to be registered.

namespace MarcosPereira.UnityUtilities {
    [RequireComponent(typeof(Collider))]
    public class Impact : MonoBehaviour {
        [SerializeField]
        [Range(0f, 1f)]
        private float maxVolume = 0.5f;

        [SerializeField]
        [Range(0f, 10f)]
        [Tooltip(
            "The collision relative speed at which the sound is played at the max volume " +
            "(defined above)."
        )]
        private float maxVolumeSpeed = 10f;

        [SerializeField]
        private AudioMixerGroup audioMixer;

        [SerializeField]
        [Tooltip("Sounds to play upon impact. Only one is played each time, picked randomly.")]
        private AudioClip[] audioClips;

        [SerializeField]
        [Tooltip("Particle systems to play upon collision.")]
        private ParticleSystem[] particleSystems;

        private AudioSource audioSource;

        public void Awake() {
            this.audioSource = this.gameObject.AddComponent<AudioSource>();
            this.audioSource.spatialBlend = 1f; // 3D sound
            this.audioSource.volume = 0f;
            this.audioSource.playOnAwake = false;
            this.audioSource.outputAudioMixerGroup = this.audioMixer;
        }

        public void OnCollisionEnter(Collision collision) {
            float relativeSpeed = collision.relativeVelocity.magnitude;

            float ratio = Mathf.Clamp01(relativeSpeed / this.maxVolumeSpeed);

            float volume = ratio * this.maxVolume;

            // TODO: continue

            UnityEngine.Debug.Log(
                $"Impact OnCollisionEnter: {collision.collider.name}"
            );
        }

        public void OnTriggerEnter(Collider other) {
            UnityEngine.Debug.Log(
                $"Impact OnTriggerEnter: {other.name}"
            );
        }

        private void PlayRandomClip(AudioClip[] clips, float volume) {
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            this.audioSource.PlayOneShot(clip, volume);
        }
    }
}
