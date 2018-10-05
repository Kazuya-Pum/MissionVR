﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public class MinionBase : MobBase
    {
        // TODO ミニオンのAIを作成


        protected Vector3 prev;

        protected override void Awake()
        {
            base.Awake();
            entityType = EntityType.MINION;
        }

        protected override void Start()
        {
            base.Start();
            prev = tfCache.position;
        }

        protected override void Update()
        {
            base.Update();
            Move( 0, 0 );
            //photonView.RPC( "Move", PhotonTargets.AllViaServer, 0f, 0f );
        }

        [PunRPC]
        protected override void Move( float x, float z )
        {
            base.Move( x, z );

            Vector3 diff = tfCache.position - prev;

            Rotate( diff );

            prev = tfCache.position;
        }

        //[PunRPC]
        protected void Rotate( Vector3 diff )
        {
            if ( diff.magnitude > 0.01 )
            {
                tfCache.localRotation = Quaternion.LookRotation( diff );
            }
        }
    }
}