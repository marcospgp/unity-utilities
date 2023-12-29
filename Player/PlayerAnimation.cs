using UnityEngine;
using UnityEngine.Events;

namespace UnityUtilities
{
    public class PlayerAnimation : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent<Vector2> OnPlayerVelocity;

        [SerializeField]
        private Animator animator;

        /// <summary>
        /// Sync movement animation.
        /// The blend tree should be set up such that the root motion has a speed
        /// of 1 unit/second in each direction when the magnitude of the
        /// (MoveX, MoveY) vector is 1.
        /// We then set the animation speed of the blend tree using a parameter,
        /// in order to match the real player velocity.
        /// </summary>
        // private void UpdateAnimator()
        // {
        //     float speed = new Vector2(velocity.x, velocity.z).magnitude;

        //     if (speed < MIN_SPEED)
        //     {
        //         animator.SetBool("IsIdle", true);
        //     }
        //     else
        //     {
        //         animator.SetBool("IsIdle", false);
        //     }

        //     Vector3 moveDirection = transform.InverseTransformVector(
        //         new Vector3(velocity.x, 0f, velocity.z)
        //     );

        //     animator.SetFloat("MoveX", moveDirection.x);
        //     animator.SetFloat("MoveY", moveDirection.z);
        //     animator.SetFloat("MoveSpeedMetersPerSecond", speed);
        // }
    }
}
