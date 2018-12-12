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
    private Vector3 vector;
    #endregion


    protected override void Awake()
    {
        base.Awake();
        moveSpeed = ( moveSpeed <= 0 ) ? 1 : moveSpeed;
        dashRate = ( dashRate < 1 ) ? 1 : dashRate;
        rbCache = GetComponent<Rigidbody>();
    }

    protected void FixedUpdate()
    {
        if ( vector.magnitude > 0 || rbCache.velocity.magnitude > 0 )
        {
            rbCache.AddForce( vector * moveSpeed - rbCache.velocity, ForceMode.VelocityChange );
        }
    }

    [PunRPC]
    protected virtual void Move( Vector3 vector )
    {
        if ( dashFlag )
        {
            vector *= dashRate;
        }
        this.vector = vector;
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