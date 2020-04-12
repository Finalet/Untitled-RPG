///////////////////////////////////////////////////////////////////////////
//  Sheath Script 1.1 - MonoBehaviour Component Script					 //
//  Kevin Iglesias - https://www.keviniglesias.com/     			     //
//  Contact Support: support@keviniglesias.com                           //
//  Documentation: 														 //
//  https://www.keviniglesias.com/assets/SheathScript/Documentation.pdf  //
///////////////////////////////////////////////////////////////////////////

/*
 This is the main script. Add this MonoBehaviour as component to your character 
 Game Object that has the Animator. Check the Documentation for more information.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KevinIglesias {

	public enum SheathActions {Sheathe, Unsheathe}

	[System.Serializable]
	public class Sheaths{
		
		[Tooltip("Name for identifying this sheath")]
		public string sheathName;
		
		[Tooltip("Weapon to sheath/unsheath")]
		public Transform weapon;
		
		[Tooltip("Weapon root in hand bone")]
		public Transform hand;
		
		[Tooltip("Sheath position and rotation")]
		public Transform sheath;
		
	}	
	
	[RequireComponent(typeof(Animator))]	
	public class SheathComponentScript : MonoBehaviour {

		[Tooltip("Add sheaths needed for your character")]
		public List<Sheaths> sheaths;
		
		//Function called by the SheathScript StateMachine Behaviour.
		public void Sheathe(SheathActions sheathAction, int sheathId)
		{
			//Avoid errors if List is null.
			if(sheaths == null)
			{
				Debug.Log("No sheaths founds in Sheath List. Add at least one sheath using the Unity inspector.");
				return;
			}
			
			//Avoid errors if any Transform is missing.
			if(sheaths.Count <= sheathId)
			{
				Debug.Log("Weapon to sheathe/unsheathe in sheath number "+sheathId+" not found.");
				return;
			}
			
			if(sheaths[sheathId].weapon == null)
			{
				Debug.Log("Weapon to sheathe/unsheathe in sheath number "+sheathId+" not found.");
				return;
			}
			if(sheaths[sheathId].hand == null)
			{
				Debug.Log("Weapon root Transform in sheath number "+sheathId+" not found.");
				return;
			}
			if(sheaths[sheathId].sheath == null)
			{
				Debug.Log("Sheath Transform in sheath number "+sheathId+" not found.");
				return;
			}
			
			//Sheath hand matches with the sheath hand called by the animator SheathScript.
			Transform weapon = sheaths[sheathId].weapon;
			Transform hand = sheaths[sheathId].hand;
			Transform sheath = sheaths[sheathId].sheath;
			
			if(sheathAction == SheathActions.Sheathe)
			{//Sheathe.
				weapon.SetParent(sheath);
				weapon.localEulerAngles = Vector3.zero;
				weapon.localPosition = Vector3.zero;
			}else{
			//Unsheathe.
				weapon.SetParent(hand);
				weapon.localEulerAngles = Vector3.zero;
				weapon.localPosition = Vector3.zero;
			}
		}
		
		//Alternative Function for Animation Events (Sheath)
		public void Sheath(int sheathId)
		{
			//Avoid errors if List is null.
			if(sheaths == null)
			{
				Debug.Log("No sheaths founds in Sheath List. Add at least one sheath using the Unity inspector.");
				return;
			}
			
			//Avoid errors if any Transform is missing.
			if(sheaths.Count <= sheathId)
			{
				Debug.Log("Weapon to sheathe/unsheathe in sheath number "+sheathId+" not found.");
				return;
			}
			
			if(sheaths[sheathId].weapon == null)
			{
				Debug.Log("Weapon to sheathe/unsheathe in sheath number "+sheathId+" not found.");
				return;
			}
			if(sheaths[sheathId].hand == null)
			{
				Debug.Log("Weapon root Transform in sheath number "+sheathId+" not found.");
				return;
			}
			if(sheaths[sheathId].sheath == null)
			{
				Debug.Log("Sheath Transform in sheath number "+sheathId+" not found.");
				return;
			}

			Transform weapon = sheaths[sheathId].weapon;
			Transform sheath = sheaths[sheathId].sheath;
			
			//Sheathe.
			weapon.SetParent(sheath);
			weapon.localEulerAngles = Vector3.zero;
			weapon.localPosition = Vector3.zero;
		}
		
		//Alternative Function for Animation Events (Unsheath)
		public void Unsheath(int sheathId)
		{
			//Avoid errors if List is null.
			if(sheaths == null)
			{
				Debug.Log("No sheaths founds in Sheath List. Add at least one sheath using the Unity inspector.");
				return;
			}
			
			//Avoid errors if any Transform is missing.
			if(sheaths.Count <= sheathId)
			{
				Debug.Log("Weapon to sheathe/unsheathe in sheath number "+sheathId+" not found.");
				return;
			}
			
			if(sheaths[sheathId].weapon == null)
			{
				Debug.Log("Weapon to sheathe/unsheathe in sheath number "+sheathId+" not found.");
				return;
			}
			if(sheaths[sheathId].hand == null)
			{
				Debug.Log("Weapon root Transform in sheath number "+sheathId+" not found.");
				return;
			}
			if(sheaths[sheathId].sheath == null)
			{
				Debug.Log("Sheath Transform in sheath number "+sheathId+" not found.");
				return;
			}

			Transform weapon = sheaths[sheathId].weapon;
			Transform hand = sheaths[sheathId].hand;
			
			//Unsheathe.
			weapon.SetParent(hand);
			weapon.localEulerAngles = Vector3.zero;
			weapon.localPosition = Vector3.zero;
		}
	}
}
