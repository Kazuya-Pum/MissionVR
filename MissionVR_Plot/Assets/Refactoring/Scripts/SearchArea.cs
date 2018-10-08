using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public class SearchArea : Photon.MonoBehaviour
    {

        public event System.Action<EntityBase> onFound = ( entity ) => { };
        public event System.Action<EntityBase> onLost = ( entity ) => { };

        private void OnTriggerEnter( Collider other )
        {
            EntityBase entity = other.GetComponent<EntityBase>();

            onFound( entity );
        }

        private void OnTriggerExit( Collider other )
        {
            EntityBase entity = other.GetComponent<EntityBase>();

            onLost( entity );
        }
    }
}
