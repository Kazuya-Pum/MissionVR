using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticObjBase : EntityBase
{
    [PunRPC]
    public override void RotateToTarget( Vector3 to )
    {
        head.LookAt( to );
    }

    protected override void Death()
    {
        if ( entityType == EntityType.PROJECTOR )
        {
            GameManager.instance.photonView.RPC( "ToResult", PhotonTargets.AllBuffered, team );
        }

        GameManager.instance.photonView.RPC( "SetAnounce", PhotonTargets.All, AnounceType.DESTROY, team );
        base.Death();
    }
}
