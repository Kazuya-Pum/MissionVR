using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public class SearchArea : MonoBehaviour
    {

        public event System.Action<EntityBase> onFound = ( entity ) => { };
        public event System.Action<EntityBase> onLost = ( entity ) => { };

        private void OnTriggerEnter( Collider other )
        {
            
        }

        private void OnTriggerExit( Collider other )
        {

        }
    }
}
