using UnityEngine;
using System.Collections;
using MalbersAnimations.Controller;
using UnityEngine.AI;

namespace MalbersAnimations.HAP
{
    public class MountAI : MAnimalAIControl
    {
        public bool canBeCalled;
        protected Mount mount;               //The Animal Mount Script
        protected bool isBeingCalled;

        public bool CanBeCalled
        {
            get { return canBeCalled; }
            set { canBeCalled = value; }
        }

        void Start()
        {
            mount = GetComponent<Mount>();
            StartAgent();
        }

        void Update()
        {
            Updating();
        }

        public virtual void CallAnimal(Transform target, bool call)
        {
            if (!CanBeCalled) return;           //If the animal cannot be called ignore this
            isBeingCalled = call;

            if (Agent)
            {
                Agent.enabled = true;

                if (Agent.isOnNavMesh)
                {
                    if (isBeingCalled) SetTarget(target);
                    Agent.isStopped = !isBeingCalled; //If isBeingCalled == true then isStopped = false;
                }
            }
        }
    }
}
