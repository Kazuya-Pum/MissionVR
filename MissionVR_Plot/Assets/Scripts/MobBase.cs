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

    private Rigidbody rbCache;
    #endregion


    protected override void Awake()
    {
        base.Awake();
        moveSpeed = ( moveSpeed <= 0 ) ? 1 : moveSpeed;
        dashRate = ( dashRate < 1 ) ? 1 : dashRate;
        rbCache = GetComponent<Rigidbody>();
    }

    protected override void Update()
    {
        base.Update();

        rbCache.velocity = Vector3.Lerp( rbCache.velocity, Vector3.zero, 0.6f );
    }

    [PunRPC]
    protected virtual void Move( float x, float z )
    {
        if ( dashFlag )
        {
            x *= dashRate;
            z *= dashRate;
        }

        rbCache.AddForce( Vector3.Normalize( tfCache.forward * z + tfCache.right * x ) * moveSpeed, ForceMode.VelocityChange );
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
    public override void RotateToTarget( Vector3 to )
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