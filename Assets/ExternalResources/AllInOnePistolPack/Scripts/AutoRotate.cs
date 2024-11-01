using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllInOnePistolPack
{
    public class AutoRotate : MonoBehaviour
    {
        [SerializeField]
        private Transform target;
        [SerializeField]
        private Animator anim;

        private bool isRotating;
        private float targetAngle;

        void Update()
        {
            if (!isRotating)
            {
                float yRotation = target.transform.localRotation.eulerAngles.y;

                if (yRotation > 90 && yRotation < 180)
                {
                    anim.SetTrigger("RotateRight");
                    isRotating = true;
                    StartCoroutine(RotateTo(true));
                }
                else if (yRotation > 180 && yRotation < 270)
                {
                    anim.SetTrigger("RotateLeft");
                    isRotating = true;
                    StartCoroutine(RotateTo(false));
                }
            }
        }

        /// <summary>
        /// Rotate Character
        /// </summary>
        /// <param name="isRight">direction to rotate</param>
        /// <returns></returns>
        public IEnumerator RotateTo(bool isRight)
        {
            Vector3 newPos = new Vector3(0, transform.localRotation.eulerAngles.y + (isRight ? 90f : 270f), 0);

            float timeout = 0.33f;
            while (isRotating)
            {
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(newPos), Time.deltaTime * 12f);

                timeout -= Time.deltaTime;
                if (timeout < 0)
                {
                    isRotating = false;
                    transform.localRotation = Quaternion.Euler(newPos);
                }
                yield return null;
            }
        }
    }

}