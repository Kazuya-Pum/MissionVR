using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionBase : EntityBase
{

    protected Vector3 prev;

    protected override void Awake()
    {
        base.Awake();
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

        if ( diff.magnitude > 0.01 )
        {
            tfCache.rotation = Quaternion.LookRotation( diff );
        }

        prev = tfCache.position;
    }
}
