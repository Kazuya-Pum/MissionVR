using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        prev = tfCache.position;
    }

    private void Update()
    {
        Move( 0, 0 );
        //photonView.RPC( "Move", PhotonTargets.All, 0f, 0f );
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
            tfCache.rotation = Quaternion.LookRotation( diff );
        }
    }
}
