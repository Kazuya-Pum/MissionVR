using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    /// <summary>
    /// AIのステート
    /// </summary>
    public enum AI_STATE
    {
        /// <summary>
        /// 敵を発見しておらず、移動しているステート
        /// </summary>
        MOVE,
        /// <summary>
        /// 敵を検知したが、現在視界に入っておらず、視界に入るよう移動しているステート
        /// </summary>
        WARNING,
        /// <summary>
        /// 敵が視界に入っているステート
        /// </summary>
        DISCOVER
    }

    [RequireComponent( typeof( EntityBase ) )]
    public class AIBase : Photon.MonoBehaviour
    {
        EntityBase entityBase;
        SearchArea searchArea;
        public AI_STATE aiState;
        public List<EntityBase> entities;

        private void Awake()
        {
            entityBase = GetComponent<EntityBase>();
            searchArea = transform.Find( "SearchArea" ).GetComponent<SearchArea>();
        }

        public void OnFound( EntityBase target )
        {
            if ( target.team != entityBase.team )
            {
                entities.Add( target );
            }
        }

        public void OnLost( EntityBase target )
        {
            entities.Remove( target );

            if ( entities.Count == 0 )
            {
                aiState = AI_STATE.MOVE;
            }
        }
    }
}