using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// Footstep sounds. Sound is 3D for multiplayer.
// Creates its own audio source.

namespace UnityUtilities
{
    public class FootstepSound : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Distance between footsteps.")]
        [Range(0f, 2f)]
        private float stride = 1f;

        [SerializeField]
        [Range(0f, 0.5f)]
        private float minStepInterval = 0.2f;

        [SerializeField]
        [Tooltip(
            "Define footstep sounds based on the layer of the object being stepped on. "
                + "Trigger colliders override this, so that water footsteps are always played as long "
                + "as the player is touching a collider on a \"Water\" layer, for example."
        )]
        private FootstepType[] footsteps;

        [SerializeField]
        [Tooltip(
            "If set, the audio source is placed at this point. "
                + "Otherwise, it is placed at this object's origin."
        )]
        private Transform feetPosition;

        [SerializeField]
        [Range(0f, 1f)]
        private float volume = 0.5f;

        [SerializeField]
        private AudioMixerGroup audioMixer;

        private AudioSource audioSource;
        private float timeSinceLastFootstep;
        private Vector3 lastFootstepPosition;
        private CharacterController characterController;

        // Used to override footsteps when inside a trigger collider.
        // Leaving any trigger collider will set this back to null, so be careful with nested
        // trigger colliders.
        private AudioClip[] clipsOverride = null;

        private enum DetectionMode
        {
            CharacterController,
            FootCollision,
            AnimationEvent
        }

        public void Awake()
        {
            this.characterController = this.GetComponentStrict<CharacterController>();

            // Create audio source

            Transform audioSourceParent = this.transform;

            if (this.feetPosition != null)
            {
                audioSourceParent = this.feetPosition;
            }

            this.audioSource = audioSourceParent.gameObject.AddComponent<AudioSource>();
            this.audioSource.spatialBlend = 1f; // 3D sound
            this.audioSource.volume = 1f; // Will be scaled by PlayOneShot
            this.audioSource.playOnAwake = false;
            this.audioSource.outputAudioMixerGroup = this.audioMixer;
        }

        public void OnControllerColliderHit(ControllerColliderHit hit)
        {
            this.timeSinceLastFootstep += Time.deltaTime;

            if (
                this.characterController.isGrounded
                && this.timeSinceLastFootstep >= this.minStepInterval
            )
            {
                float distance = Vector3.Distance(
                    this.lastFootstepPosition,
                    this.transform.position
                );

                if (distance < this.stride)
                {
                    return;
                }

                AudioClip[] clips = null;

                if (this.clipsOverride != null)
                {
                    clips = this.clipsOverride;
                }
                else
                {
                    foreach (FootstepType footstep in this.footsteps)
                    {
                        if (footstep.layerMask.HasLayer(hit.gameObject.layer))
                        {
                            clips = footstep.audioClips;
                            break;
                        }
                    }
                }

                this.OnFootstep(clips);
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            foreach (FootstepType footstep in this.footsteps)
            {
                if (footstep.layerMask.HasLayer(other.gameObject.layer))
                {
                    this.clipsOverride = footstep.audioClips;
                    break;
                }
            }
        }

        public void OnTriggerExit()
        {
            this.clipsOverride = null;
        }

        private void OnFootstep(AudioClip[] audioClips)
        {
            AudioClip clip = audioClips[Random.Range(0, audioClips.Length)];
            this.audioSource.PlayOneShot(clip, this.volume);

            this.timeSinceLastFootstep = 0f;
            this.lastFootstepPosition = this.transform.position;
        }

        [System.Serializable]
        private struct FootstepType
        {
            public LayerMask layerMask;
            public AudioClip[] audioClips;
        }
    }
}
