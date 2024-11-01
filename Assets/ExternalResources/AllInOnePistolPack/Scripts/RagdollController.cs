using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllInOnePistolPack
{
    public class RagdollController : MonoBehaviour
    {
        public Rigidbody[] rigidbodies;
        public Collider[] colliders;

        public bool isRagdollEnabled = false;
        private bool currentState = false;

        void Start()
        {
            // Disable all rigidbodies and colliders at start
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = true;
            }

            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }
            currentState = isRagdollEnabled;
        }

        /// <summary>
        /// Activate Ragdoll
        /// </summary>
        void ActivateRagdoll()
        {
            // Enable all rigidbodies and colliders when called
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = false;
            }

            foreach (Collider col in colliders)
            {
                col.enabled = true;
            }

            // Disable the animator
            GetComponent<Animator>().enabled = false;

            isRagdollEnabled = true;
        }

        /// <summary>
        /// Deactivate Ragdoll
        /// </summary>
        void DeactivateRagdoll()
        {
            // Disable all rigidbodies and colliders when called
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = true;
            }

            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }

            // Re-enable the animator
            GetComponent<Animator>().enabled = true;

            isRagdollEnabled = false;
        }

        void Update()
        {
            if (currentState != isRagdollEnabled)
            {
                currentState = isRagdollEnabled;
                if (currentState == true)
                {
                    ActivateRagdoll();
                }
                else
                {
                    DeactivateRagdoll();
                }
            }
        }

    }
}


