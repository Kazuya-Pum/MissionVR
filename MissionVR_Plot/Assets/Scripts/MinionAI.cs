using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum MinionLane : byte { TOP, MID, BOT }

[RequireComponent( typeof( NavMeshAgent ) )]
public class MinionAI : AIBase, IPunObservable
{
    public List<Vector3> lanePoints;
    public NavMeshAgent agent;
    public int destPoint = 0;
    public MinionLane minionLane;

    protected override void Awake()
    {
        base.Awake();

        agent = GetComponent<NavMeshAgent>();
    }

    [PunRPC]
    protected void SetValue( MinionLane lane, Vector3 point, Team team )
    {
        minionLane = lane;
        lanePoints.Clear();

        switch ( minionLane )
        {
            case MinionLane.TOP:
                foreach ( Transform item in MinionSpawnController.instance.rootTopPoints )
                {
                    if ( team == Team.WHITE )
                    {
                        lanePoints.Add( item.position );
                    }
                    else
                    {
                        lanePoints.Insert( 0, item.position );
                    }
                }
                break;
            case MinionLane.MID:
                break;
            case MinionLane.BOT:
                foreach ( Transform item in MinionSpawnController.instance.rootBotPoints )
                {
                    if ( team == Team.WHITE )
                    {
                        lanePoints.Add( item.position );
                    }
                    else
                    {
                        lanePoints.Insert( 0, item.position );
                    }
                }
                break;
        }

        if ( team == Team.WHITE )
        {
            lanePoints.Add( MinionSpawnController.instance.blackProjector.position );
        }
        else
        {
            lanePoints.Add( MinionSpawnController.instance.whileProjector.position );
        }

        agent.Warp( point );
        agent.destination = lanePoints[0];
        destPoint = 0;
    }

    protected override void Update()
    {
        base.Update();

        if ( aiState == AI_STATE.MOVE && !agent.pathPending && agent.remainingDistance < 0.5f )
        {
            GotoNextPoint();
        }
    }

    [PunRPC]
    protected override void RpcChangeState( AI_STATE to )
    {
        base.RpcChangeState( to );

        if ( GameManager.instance.gameState == GameState.GAME )
        {
            switch ( to )
            {
                case AI_STATE.MOVE:
                    agent.destination = lanePoints[destPoint];
                    agent.isStopped = false;
                    break;
                case AI_STATE.WARNING:
                    if ( tmpTarget )
                    {
                        agent.destination = tmpTarget.tfCache.position;
                    }
                    agent.isStopped = false;
                    break;
                case AI_STATE.DISCOVER:
                    agent.isStopped = true;
                    break;
            }
        }
    }

    void GotoNextPoint()
    {
        // 地点がなにも設定されていないときに返す
        if ( lanePoints.Count == 0 || destPoint >= lanePoints.Count - 1 )
        {
            return;
        }

        destPoint++;

        // エージェントが現在設定された目標地点に行くように設定
        agent.destination = lanePoints[destPoint];
    }

    public void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
    {
        if ( stream.isWriting )
        {
            stream.SendNext( minionLane );
            stream.SendNext( agent.destination );
        }
        else
        {
            minionLane = (MinionLane)stream.ReceiveNext();
            agent.destination = (Vector3)stream.ReceiveNext();
        }
    }
}
