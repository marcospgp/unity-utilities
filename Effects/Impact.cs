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
        [Tooltip("The layers that will trigger this component upon collision.")]
        private LayerMask targetLayers;

        [SerializeField]
        [Range(0f, 1f)]
        private float maxVolume = 0.5f;

        [SerializeField]
        [Tooltip(
            "The relative collision speed at which the sound is played at the max volume " +
            "(defined above)."
        )]
        private float maxVolumeSpeed = 10f;

        [SerializeField]
        [Tooltip("The minimum relative collision speed for an impact to be triggered.")]
        private float minSpeed;

        [SerializeField]
        [Tooltip(
            "Use this to determine which components of the collision's relative velocity " +
            "are taken into account when determining its relative speed.\n" +
            "For example, a vertical water splash can be set with (0, 1, 0)."
        )]
        private Vector3 relativeVelocityMultiplier = Vector3.one;

        [SerializeField]
        private AudioMixerGroup audioMixer;

        [SerializeField]
        [Tooltip("Sounds to play upon impact. Only one is played each time, picked randomly.")]
        private AudioClip[] audioClips;

        [SerializeField]
        [Tooltip("Particle systems to play upon collision.")]
        private ParticleSystem[] particleSystems;

        private AudioSource audioSource;

        private CharacterController charController;
        private new Rigidbody rigidbody;

        public void Awake() {
            var child = new GameObject("Audio Source");
            child.transform.SetParent(this.transform);

            this.audioSource = child.AddComponent<AudioSource>();
            this.audioSource.spatialBlend = 1f; // 3D sound
            this.audioSource.volume = 0f;
            this.audioSource.playOnAwake = false;
            this.audioSource.outputAudioMixerGroup = this.audioMixer;

            this.charController = this.GetComponent<CharacterController>();
            this.rigidbody = this.GetComponent<Rigidbody>();
        }

        public void OnCollisionEnter(Collision collision) {
            if (IsLayerInMask(collision.gameObject.layer, this.targetLayers)) {
                var contact = collision.GetContact(0);
                var scaledVelocity =
                    Vector3.Scale(collision.relativeVelocity, this.relativeVelocityMultiplier);

                this.OnImpact(contact.point, contact.normal, scaledVelocity.magnitude);
            }
        }

        public void OnTriggerEnter(Collider other) {
            if (IsLayerInMask(other.gameObject.layer, this.targetLayers)) {
                Vector3 velocity = Vector3.zero;

                if (other.TryGetComponent(out Rigidbody otherRigidbody)) {
                    // Negative because we want relative velocity from this object to the other.
                    velocity -= otherRigidbody.velocity;
                }

                if (this.charController != null) {
                    velocity += this.charController.velocity;
                } else {
                    velocity += this.rigidbody.velocity;
                }

                this.OnImpact(this.transform.position, this.transform.up, velocity.magnitude);
            }
        }

        private static bool IsLayerInMask(int layer, int layerMask) =>
            ((1 << layer) & layerMask) != 0;

        private void OnImpact(Vector3 position, Vector3 normal, float speed) {
            if (speed < this.minSpeed) {
                return;
            }

            this.PlayRandomClip(position, speed);

            foreach (var x in this.particleSystems) {
                x.Stop();
                x.transform.SetPositionAndRotation(
                    position,
                    x.transform.rotation * Quaternion.FromToRotation(x.transform.up, normal)
                );
                x.Play();
            }
        }

        private void PlayRandomClip(Vector3 position, float speed) {
            float volume = this.GetVolume(speed);

            AudioClip clip = this.audioClips[Random.Range(0, this.audioClips.Length)];

            this.audioSource.transform.position = position;
            this.audioSource.PlayOneShot(clip, volume);
        }

        private float GetVolume(float speed) {
            float ratio = Mathf.Clamp01(speed / this.maxVolumeSpeed);

            return Mathf.Pow(ratio, 2f) * this.maxVolume;
        }
    }
}
