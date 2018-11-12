﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// AIのステート
/// </summary>
public enum AI_STATE : byte
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
    protected EntityBase entityBase;
    protected EntityBase tmpTarget;
    [SerializeField] private float attackRange;
    public AI_STATE aiState;
    public HashSet<EntityBase> entities = new HashSet<EntityBase>();
    private int entityLayer;

    protected virtual void Awake()
    {
        entityBase = GetComponent<EntityBase>();
        entityLayer = LayerMask.NameToLayer( "Entity" );
    }

    protected virtual void Update()
    {
        if ( PhotonNetwork.isMasterClient && GameManager.instance.gameState == GameState.GAME && aiState != AI_STATE.MOVE )
        {
            entities.RemoveWhere( ( EntityBase e ) => e == null );
            if ( entities.Any() )
            {
                if ( tmpTarget == null || !entities.Contains( tmpTarget ) )
                {
                    tmpTarget = entities.FirstOrDefault();
                }

                if ( Ray( tmpTarget ) )
                {
                    ChangeState( AI_STATE.DISCOVER );
                    entityBase.trigger = true;
                }
                else
                {
                    entities.Remove( tmpTarget );
                    if ( entities.Any() )
                    {
                        tmpTarget = entities.FirstOrDefault();
                    }

                    ChangeState( AI_STATE.WARNING );
                    entityBase.trigger = false;
                }

                entities.Clear();
            }
            else
            {
                ChangeState( AI_STATE.MOVE );
                entityBase.trigger = false;
            }
        }

        if ( aiState == AI_STATE.DISCOVER )
        {
            entityBase.RotateToTarget( tmpTarget.head.position );
        }
    }

    protected void ChangeState( AI_STATE to )
    {
        if ( aiState != to && GameManager.instance.gameState == GameState.GAME )
        {
            photonView.RPC( "RpcChangeState", PhotonTargets.All, to );
        }
    }

    [PunRPC]
    protected virtual void RpcChangeState( AI_STATE to )
    {
        aiState = to;
    }

    public void OnCheck( EntityBase target )
    {
        if ( target.team != entityBase.team )
        {
            entities.Add( target );
            if ( aiState == AI_STATE.MOVE )
            {
                ChangeState( AI_STATE.WARNING );
            }
        }
    }

    private bool Ray( EntityBase target )
    {
        float distance = Vector3.Distance( target.head.position, entityBase.muzzle.position );

        if ( distance <= attackRange )
        {
            RaycastHit[] hits = Physics.RaycastAll( entityBase.muzzle.position, entityBase.head.forward, distance );

            foreach ( RaycastHit hit in hits )
            {
                if ( hit.collider.gameObject.layer != entityLayer )
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    public void OnLost( EntityBase target )
    {
        entities.Remove( target );

        if ( entities.Any() )
        {
            ChangeState( AI_STATE.MOVE );
            entityBase.trigger = false;
        }
    }
}