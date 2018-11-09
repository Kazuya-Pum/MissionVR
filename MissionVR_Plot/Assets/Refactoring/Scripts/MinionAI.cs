using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Refactoring
{
    [RequireComponent( typeof( NavMeshAgent ) )]
    public class MinionAI : AIBase
    {
        public Transform[] lanePoints;
        private NavMeshAgent agent;
        [SerializeField] private int destPoint = 0;

        protected override void Awake()
        {
            base.Awake();

            agent = GetComponent<NavMeshAgent>();
        }

        void Start()
        {
            agent.destination = lanePoints[destPoint].position;
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
                        agent.destination = lanePoints[destPoint].position;
                        agent.isStopped = false;
                        break;
                    case AI_STATE.WARNING:
                        if ( tmpTarget )
                        {
                            agent.destination = tmpTarget.transform.position;
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
            if ( lanePoints.Length == 0 || destPoint >= lanePoints.Length - 1 )
            {
                return;
            }

            destPoint++;

            // エージェントが現在設定された目標地点に行くように設定
            agent.destination = lanePoints[destPoint].position;
        }
    }

}
