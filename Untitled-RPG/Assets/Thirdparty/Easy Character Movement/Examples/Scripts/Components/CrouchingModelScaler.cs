using ECM.Controllers;
using UnityEngine;

namespace ECM.Examples
{
    /// <summary>
    /// Basic example to show, how use the BaseCharacterController isCrouching state to scale a character model.
    /// </summary>

    public sealed class CrouchingModelScaler : MonoBehaviour
    {
        #region EDITOR EXPOSED FIELDS

        /// <summary>
        /// The parent ECM CharacterController
        /// </summary>

        public BaseCharacterController characterController;

        /// <summary>
        /// The character model to scale-down while crouching
        /// </summary>

        public Transform modelTransform;

        /// <summary>
        /// How fast will the scale animation occur.
        /// </summary>

        public float scaleSpeed = 8.0f;

        #endregion

        #region METHODS

        /// <summary>
        /// Perform model scale based on BaseCharacterController isCrouching property.
        /// </summary>

        private void ModelScale()
        {
            var yScale = characterController.isCrouching
                ? Mathf.Clamp01(characterController.crouchingHeight / characterController.standingHeight)
                : 1.0f;

            modelTransform.localScale =
                Vector3.MoveTowards(modelTransform.localScale, new Vector3(1.0f, yScale, 1.0f),
                    scaleSpeed * Time.deltaTime);
        }

        #endregion

        #region MONOBEHAVIOUR

        /// <summary>
        /// Use the BaseCharacterController isCrouching property to modify the character's model size.
        /// </summary>

        private void LateUpdate()
        {
            if (characterController.isPaused)
                return;

            ModelScale();
        }

        #endregion
    }
}
