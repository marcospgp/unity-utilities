using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

// Plays audio based on a collider. Different modes available.
// Requires having a collider on the same GameObject as this.
// Does not require an audio source - this component adds its own.

namespace MarcosPereira.UnityUtilities {
    [RequireComponent(typeof(Collider))]
    [DisallowMultipleComponent]
    public class AudioZone : MonoBehaviour {
        [SerializeField, TagSelect]
        [Tooltip("The tag of the collider that should trigger the sound from this component.")]
        private string colliderTag;

        [SerializeField]
        private AudioClip audioClip;

        [SerializeField]
        private AudioMixerGroup audioMixer;

        [SerializeField]
        [Range(0f, 1f)]
        private float volume = 0.5f;

        [SerializeField]
        private Mode mode = Mode.PlayWhileInside;

        [Header("PlayWhileInside")]

        [SerializeField]
        [Range(0f, 5f)]
        [Tooltip("How much the volume can change per second.")]
        private float volumeFadeSpeed = 2f;

        [Header("PlayOnEnter/PlayOnExit")]

        [SerializeField]
        [Tooltip(
            "Set whether the other collider's speed is taken into account when determining the " +
            "audio volume.\nEnabling this requires there to be a character controller in " +
            "one of the player camera's parent gameObjects."
        )]
        private bool scaleVolume;

        [SerializeField]
        [Tooltip(
            "Use this to determine which components of the player's movement " +
            "cause the volume to increase. For example, a vertical water " +
            "splash can be set with (0, -1, 0)")]
        private Vector3 velocityMultiplier = Vector3.one;

        [SerializeField]
        [Tooltip(
            "The collision speed at which the sound is played at max volume (defined above)."
        )]
        private float maxVolumeSpeed = 10f;

        private AudioSource audioSource;

        private Coroutine volumeCoroutine;

        private CharacterController charController;

        private enum Mode {
            PlayWhileInside,
            PlayOnEnter,
            PlayOnExit
        }

        public void Awake() {
            if (!this.GetComponent<Collider>().isTrigger) {
                throw new System.Exception("Trigger collider required.");
            }

            this.audioSource = this.gameObject.AddComponent<AudioSource>();
            this.audioSource.spatialBlend = 0f; // 2D sound
            this.audioSource.volume = 0f;
            this.audioSource.playOnAwake = false;
            this.audioSource.outputAudioMixerGroup = this.audioMixer;

            if (this.scaleVolume) {
                this.charController = GameObject
                    .FindGameObjectWithTag(this.colliderTag)
                    .GetComponentInParent<CharacterController>();

                if (this.charController == null) {
                    throw new System.Exception(
                        "Missing character controller in player camera's parents."
                    );
                }
            }
        }

        public void OnTriggerEnter(Collider other) {
            if (!other.CompareTag(this.colliderTag)) {
                return;
            }

            if (this.mode == Mode.PlayWhileInside) {
                this.audioSource.clip = this.audioClip;
                this.audioSource.loop = true;

                this.volumeCoroutine =
                    this.StartCoroutine(this.FadeVolume(this.volume));
            } else if (this.mode == Mode.PlayOnEnter) {
                this.audioSource.volume = this.GetVolume();
                this.audioSource.PlayOneShot(this.audioClip);
            }
        }

        public void OnTriggerExit(Collider other) {
            if (!other.CompareTag(this.colliderTag)) {
                return;
            }

            if (this.mode == Mode.PlayWhileInside) {
                this.volumeCoroutine =
                    this.StartCoroutine(this.FadeVolume(0f));
            } else if (this.mode == Mode.PlayOnExit) {
                this.audioSource.volume = this.GetVolume();
                this.audioSource.PlayOneShot(this.audioClip);
            }
        }

        private IEnumerator FadeVolume(float targetVolume) {
            if (this.volumeCoroutine != null) {
                this.StopCoroutine(this.volumeCoroutine);
            }

            if (targetVolume > 0f) {
                this.audioSource.Play();
            }

            float volume01 = Mathf.Clamp01(targetVolume);

            while (this.audioSource.volume != volume01) {
                this.audioSource.volume =
                    Mathf.MoveTowards(
                        this.audioSource.volume,
                        volume01,
                        this.volumeFadeSpeed * Time.deltaTime
                    );

                yield return null;
            }

            if (this.audioSource.volume == 0f) {
                this.audioSource.Stop();
            }
        }

        private float GetVolume() {
            if (!this.scaleVolume) {
                return this.volume;
            }

            float speed = Vector3.Magnitude(
                Vector3.Scale(
                    this.charController.velocity, this.velocityMultiplier
                )
            );

            float ratio = Mathf.Clamp01(speed / this.maxVolumeSpeed);

            return ratio * this.volume;
        }
    }
}
