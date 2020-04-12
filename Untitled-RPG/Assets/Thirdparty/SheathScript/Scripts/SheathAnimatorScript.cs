///////////////////////////////////////////////////////////////////////////
//  Sheath Script 1.1 - StateMachineBehaviour Script				     //
//  Kevin Iglesias - https://www.keviniglesias.com/     			     //
//  Contact Support: support@keviniglesias.com                           //
//  Documentation: 														 //
//  https://www.keviniglesias.com/assets/SheathScript/Documentation.pdf  //
///////////////////////////////////////////////////////////////////////////

/*
 This script is optional. Use this as a StateMachineBehaviour for sheathe 
 or unsheathe weapons from the Animator. If you want to use Animation Events or
 calls from your custom scripts you don't need this.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KevinIglesias {

	public class SheathAnimatorScript : StateMachineBehaviour {

		[Tooltip("Point in the animation for sheath/unsheath to happen (in %, 0.5 means at the middle of the animation)")]
		public float timePoint;
		[Tooltip("Sheathe or unsheath animation")]
		public SheathActions sheathAction;
		[Tooltip("ID of the sheath")]
		public int sheathId;
		
		SheathComponentScript sheathComponent;
		bool sheathDone;
		
		override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
		{	
			if(sheathComponent == null)
			{
				//Get SheathComponentScript from the character gameObject.
				sheathComponent = animator.GetComponent<SheathComponentScript>();
			}
			
			//Reset sheathDone bool for future uses.
			sheathDone = false;
			
			//Avoid errors if SheathComponentScript is null or not found.
			if(sheathComponent == null)
			{
				Debug.Log("Sheath MonoBehaviour Script not found in the character gameObject. Add the SheathScript MonoBehaviour Component to the character gameObject with the Animator Component.");
				sheathDone = true;
			}
		}

		override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
		{
			if(!sheathDone)
			{//If sheathe/unsheathe is not done yet:
				if(stateInfo.normalizedTime >= timePoint)
				{//If the animation point matches with the timePoint:
					//Call the sheathe/unsheathe function
					sheathComponent.Sheathe(sheathAction, sheathId);
					sheathDone = true;
				}
			}

		}

		//Avoid timePoint being more than 1 and less than 0. It is a %, 0.5 means middle of the animation.
		void OnValidate()
		{
			if(timePoint > 1)
			{
				timePoint = 1;
			}
			
			if(timePoint < 0)
			{
				timePoint = 0;
			}
		}
	}
}