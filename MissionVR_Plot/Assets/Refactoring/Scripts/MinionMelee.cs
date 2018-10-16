using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Refactoring
{
    public class MinionMelee : MinionBase
    {
        public Transform goal;

        protected override void Start()
        {
            base.Start();
            //GetComponent<NavMeshAgent>().destination = goal.position;
        }
    }
}