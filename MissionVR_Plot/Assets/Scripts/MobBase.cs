using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobBase : EntityBase
{

    #region variable
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float dashRate;
    protected bool dashFlag = false;

    [SerializeField] protected Transform modelRotate;
    #endregion


    protected override void Awake()
    {
        base.Awake();
        moveSpeed = ( moveSpeed <= 0 ) ? 1 : moveSpeed;
        dashRate = ( dashRate < 1 ) ? 1 : dashRate;
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
    protected override void Rotate( float x = 0, float y = 0 )
    {
        base.Rotate( x, y );
        modelRotate.localRotation = head.localRotation;
    }

    [PunRPC]
    protected override void RotateToTarget( Vector3 to )
    {
        base.RotateToTarget( to );
        modelRotate.localRotation = head.localRotation;
    }

    protected override void UpdateRotation()
    {
        base.UpdateRotation();
        modelRotate.localRotation = head.localRotation;
    }
}