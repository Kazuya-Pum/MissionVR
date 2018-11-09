using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//[RequireComponent(typeof(NavMeshAgent))]

//ミニオンの移動を管理するスクリプト
public class MinionMovement : MonoBehaviour
{
    #region Variables
    [SerializeField]
    NavMeshAgent agent;

    TeamColor opponent; //相手のチームカラー

    /*
     * ミニオンがレーンの真ん中を移動するようにポイントを指定した際に利用するオブジェクト
     * 1つめはBotのwhite側、2つめはBotのBlack側、3つ目はTopのwhite側、４つめはTopのBlack側
     * Top、Mid、Botの順に上から見えるようにステージを眺めたとき左下がwhite拠点
     */
    [SerializeField]
    private Transform[] points;

    private int destPoint = 0;  //上記のポイントの何番目にいるかを数えるためのカウント
    LocalVariables localVariables;
    // NavMeshAgentに設定するmask
    private int topMask = 1 << 3, midMask = 1 << 4, botMask = 1 << 5;
    private PhotonTransformView photonTransformView;

    #endregion

    void Start()
    {
        destPoint = 0;
        this.gameObject.AddComponent<NavMeshAgent>();
        agent = this.gameObject.GetComponent<NavMeshAgent>();
        agent.enabled = true;
        localVariables = this.gameObject.GetComponent<LocalVariables>();

        switch (localVariables.lane)
        {
            case Lane.Top:
                agent.areaMask = topMask;
                break;
            case Lane.Mid:
                agent.areaMask = midMask;
                break;
            case Lane.Bot:
                agent.areaMask = botMask;
                break;
            default:
                break;
        }

        //初めのポイントへSetDistinationする
        GoToNextPoint();
    }

    void Update()
    {
        if (!agent)
            return;
        //目的のポイントの近くになると次のポイントを目指す
        if (agent.remainingDistance <= 0.5f)
        {
            GoToNextPoint();
        }
    }

    public void CallGoToPoint()
    {
        GoToNextPoint();
    }

    //チームとレーンによってどう移動するかを決めてSetDistinationするメソッド
    private void GoToNextPoint()
    {
        switch (localVariables.lane)
        {
            case Lane.Top:
                switch (localVariables.team)
                {
                    case TeamColor.White:
                        switch (destPoint)
                        {
                            case 0:
                                agent.destination = points[2].position;
                                destPoint = 1;
                                break;
                            case 1:
                                agent.destination = points[3].position;
                                destPoint = 2;
                                break;
                            case 2:
                                agent.destination = points[5].position;
                                destPoint = 3;
                                break;
                            default:
                                break;
                        }
                        break;
                    case TeamColor.Black:
                        switch (destPoint)
                        {
                            case 0:
                                agent.destination = points[3].position;
                                destPoint = 1;
                                break;
                            case 1:
                                agent.destination = points[2].position;
                                destPoint = 2;
                                break;
                            case 2:
                                agent.destination = points[8].position;
                                destPoint = 3;
                                break;
                            default:
                                break;
                        }
                        break;
                }
                break;
            case Lane.Mid:
                switch (localVariables.team)
                {
                    case TeamColor.White:
                        agent.destination = points[4].position;
                        break;
                    case TeamColor.Black:
                        agent.destination = points[7].position;
                        break;
                    default:
                        break;
                }
                break;
            case Lane.Bot:
                switch (localVariables.team)
                {
                    case TeamColor.White:
                        switch (destPoint)
                        {
                            case 0:
                                agent.destination = points[1].position;
                                destPoint = 1;
                                break;
                            case 1:
                                agent.destination = points[0].position;
                                destPoint = 2;
                                break;
                            case 2:
                                agent.destination = points[6].position;
                                destPoint = 3;
                                break;
                        }
                        break;
                    case TeamColor.Black:
                        switch (destPoint)
                        {
                            case 0:
                                agent.destination = points[0].position;
                                destPoint = 1;
                                break;
                            case 1:
                                agent.destination = points[1].position;
                                destPoint = 2;
                                break;
                            case 2:
                                agent.destination = points[9].position;
                                destPoint = 3;
                                break;
                        }
                        break;
                }
                break;
        }

    }

}