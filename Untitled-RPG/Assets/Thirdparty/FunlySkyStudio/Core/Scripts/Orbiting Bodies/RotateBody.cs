using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio {
	// Rotates along the z-axis to spin the sun or moon textures.
	public class RotateBody : MonoBehaviour {

		private float m_SpinSpeed;
		public float SpinSpeed { 
			get { return m_SpinSpeed; }
			set {
				m_SpinSpeed = value;
				UpdateOrbitBodyRotation();
			}
		}

		private bool m_AllowSpinning;
		public bool AllowSpinning {
			get { return m_AllowSpinning; }
			set {
				m_AllowSpinning = value;
				UpdateOrbitBodyRotation();
			}
		}
		
		public void UpdateOrbitBodyRotation() {
			float allowOrClear = m_AllowSpinning ? 1 : 0;

			Vector3 rot = this.transform.localRotation.eulerAngles;
			Vector3 rotSpin = new Vector3(
				0.0f,
				-180.0f,
				(rot.z + (-10.0f * SpinSpeed * Time.deltaTime)) * allowOrClear
			);

			this.transform.localRotation = Quaternion.Euler(rotSpin);
		}
	}
}
