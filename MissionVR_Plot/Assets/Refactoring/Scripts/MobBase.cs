using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobBase : EntityBase {

    #region variable
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float dashRate;
    protected bool dashFlag = false;
    #endregion


    protected override void Awake()
    {
        base.Awake();
        moveSpeed = ( moveSpeed <= 0 ) ? 1 : moveSpeed;
        dashRate = ( dashRate < 1 ) ? 1 : dashRate;

        head = tfCache.Find( "Head" );
    }

    [PunRPC]
    protected virtual void Move( float x, float z )
    {
        if ( dashFlag )
        {
            x *= dashRate;
            z *= dashRate;
        }

        tfCache.Translate( x * moveSpeed, 0, z * moveSpeed );
    }

    [PunRPC]
    protected void SetDashFlag( bool flag )
    {
        dashFlag = flag;
    }

    [PunRPC]
    protected virtual void Rotate( float x = 0, float y = 0 )
    {
        tfCache.localEulerAngles = new Vector3( 0, tfCache.localEulerAngles.y + x, 0 );
        head.localEulerAngles = new Vector3( head.localEulerAngles.x - y, 0, 0 );   // TODO 角度の上限作成
    }
}
