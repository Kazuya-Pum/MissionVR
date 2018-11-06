using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MinionSearchAndAttack : Photon.MonoBehaviour {

    #region Variables
    public List<GameObject> searchTask = new List<GameObject>();
    bool isRunning;
    NavMeshAgent agent;
    private TeamColor team;
    private TeamColor otherteam;
    private GameObject[] goal = new GameObject[2];
    private TeamColor opponent;
    private GameObject target;
    [SerializeField]
    private int tracespeed = 5;
    LocalVariables localVariables;
    [SerializeField]
    private float searchRange = 30;
    MinionMovement minionMovement;
    LocalVariables targetLocalVariables;
    SubLocalVariables playerLocalVariables;

    GameObject[] players;
    #endregion

    // NavMeshAgentに設定するmask
    private int topMask = 1 << 3, midMask = 1 << 4, botMask = 1 << 5;

    private void Start()
    {
        agent = this.gameObject.transform.root.gameObject.GetComponent<NavMeshAgent>();

        localVariables = this.gameObject.transform.root.GetComponent<LocalVariables>();
        goal[0] = GameObject.FindWithTag( "WhiteProjector");
        goal[1] = GameObject.FindWithTag( "BlackProjector");
        players = GameObject.FindGameObjectsWithTag( "Player");
        minionMovement = this.gameObject.transform.parent.GetComponent<MinionMovement>();
    }

    //違うチームが範囲に入ってきたとき
    private void OnTriggerEnter( Collider other)
    {
        //Minionの場合、rootのLocalVariablesを取得
        if ( other.gameObject.tag == "Minion")
        {
            otherteam = other.gameObject.GetComponent<LocalVariables>().team;//攻撃対象のチーム取得
            team = this.gameObject.transform.root.gameObject.GetComponent<LocalVariables>().team;
            TeamDistinguish( other);
        }

        if ( other.gameObject.tag == "Player") {
            otherteam = ( other.gameObject.GetPhotonView().ownerId % 2 == 0 ? TeamColor.Black : TeamColor.White);
            team = this.gameObject.transform.root.gameObject.GetComponent<LocalVariables>().team;
            TeamDistinguish( other);
        }

        //ステージオブジェクトの場合、そのオブジェクト自身のLocalVariablesを取得
        if ( other.gameObject.tag == "Tower" || other.gameObject.tag == "Projector")
        {
            otherteam = other.gameObject.GetComponent<LocalVariables>().team;
            team = this.gameObject.transform.root.gameObject.GetComponent<LocalVariables>().team;
            TeamDistinguish( other);
        }
    }

    void TeamDistinguish( Collider other)
    {
        if ( team != otherteam)
        {
            if ( other.gameObject.tag == "Player")
            {
                if ( other.gameObject.GetComponent<Chara>().gameState == GameState.Dead) return;
            }
            searchTask.Add( other.gameObject);
            Debug.Log( "searchTaskに入った　： " + other.gameObject);
            //navmeshを一時停止して目的を対象者へ変更
            target = other.gameObject;
            if ( !this.gameObject.transform.root.gameObject)
                return;

            agent.destination = target.transform.position;
        }
    }

    //範囲外に出たとき
    private void OnTriggerExit( Collider other)
    {
        if ( !PhotonNetwork.isMasterClient) return;

        if ( team != otherteam)
        {
            for ( int i = 0; i < searchTask.Count; i++)
            {
                if ( searchTask[i] == other.gameObject.transform.root.gameObject)
                {
                    searchTask.Remove( searchTask[i]);
//                    Debug.Log("searchTaskから出ていった　： " + other.gameObject);
                }
            }
        }
    }

    private void OnTriggerStay( Collider other)
    {
        if ( searchTask.Count != 0)
        {
            if ( other.gameObject == searchTask[0]
                && isRunning == false)//searchTaskに要素が残っていたら第一要素に対して攻撃コルーチン開始
            {
                if ( AttackRange( other.gameObject, this.gameObject) <= searchRange * searchRange)
                {
                    if ( searchTask[0] == null)
                    {
                        searchTask.RemoveAt(0);
                        return;
                    }
                    StartCoroutine( AttackCoroutine( other.gameObject));
                    isRunning = true;
                    if(agent)
                    agent.isStopped = true;
                }
                else
                {
                    searchTask.RemoveAt(0);
                }
            }
            if ( !agent.isActiveAndEnabled) return;
            if ( !agent.isStopped)//searchTaskに要素が残っていたら攻撃を続けるために移動を停止する
            {
                agent.isStopped = true;
            }
        }
    }

    private float AttackRange( GameObject target, GameObject thisObject)
    {
        if ( target == null) return 999999;
        if ( thisObject == null) return 99999999;

        float xRange, zRange, range;

        xRange = ( target.transform.position.x - thisObject.transform.position.x);
        zRange = ( target.transform.position.z - thisObject.transform.position.z);
        range = xRange * xRange + zRange * zRange;

        return range;
    }

    IEnumerator AttackCoroutine( GameObject attackTarget)//攻撃コルーチン
    {
        targetLocalVariables = null;
        playerLocalVariables = null;

        //対象のlocalな変数管理スクリプト取得を取得
        if ( attackTarget.tag == "Tower")
        {
            if ( PhotonNetwork.isMasterClient)
            targetLocalVariables = attackTarget.GetComponent<LocalVariables>();
        }
        else if ( attackTarget.tag == "Projector")
        {
            if ( PhotonNetwork.isMasterClient)
            targetLocalVariables = attackTarget.GetComponent<LocalVariables>();
        }
        else if ( attackTarget.tag == "Minion" )
        {
            targetLocalVariables = attackTarget.GetComponent<LocalVariables>();
        }
        else if ( attackTarget.tag == "Player")
        {
            if ( PhotonNetwork.isMasterClient)
            {
                playerLocalVariables = GameObject.Find( "NetworkManager").GetComponent<NetworkManager>().subLocalVariables[ attackTarget.GetPhotonView().ownerId - 1];
            }
        }

        /*
         * ダメージ計算式(とりあえず簡単にしておきます)
         * 与えるダメージは、{(自身の物理攻撃力 - 相手の物理防御力) + (自身の魔法攻撃力 - 相手の魔法防御力)}
         */
        if ( targetLocalVariables == null && playerLocalVariables == null)
        {
            searchTask.Remove( searchTask[0]);
            isRunning = false;
            yield break;
        }
        if ( localVariables == null)
        {
            searchTask.Remove( searchTask[0]);
            isRunning = false;
            yield break;
        }
        if ( !PhotonNetwork.isMasterClient && attackTarget.tag != "Minion")
            yield break;

        int dmg = 0;

        if ( playerLocalVariables == null)//攻撃対象がplayer以外のとき
        {
            dmg = (int)(( localVariables.PhysicalOffence - targetLocalVariables.PhysicalDefence)
                + ( localVariables.MagicalOffence - targetLocalVariables.MagicalDefence));
        }
        else//攻撃対象がplayerのとき
        {
            dmg = (int)( ( localVariables.PhysicalOffence - playerLocalVariables.PhysicalDefence)
                + ( localVariables.MagicalOffence - playerLocalVariables.MagicalDefence));
           // Debug.Log(dmg);
        }

        //回復しないように
        if (dmg < 0)
        {
            dmg = 0;
        }

        if(agent)
        agent.isStopped = false;

        //引数はダメージ量(int値)
        if ( playerLocalVariables == null)//攻撃対象がplayer以外のとき
        {
            targetLocalVariables.Damage( dmg);
            if(targetLocalVariables.Hp <= 0)
            {
                NetworkManager networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
                networkManager.photonView.RPC("MinionKillMinion", PhotonTargets.MasterClient, attackTarget.GetPhotonView().viewID);
            }
        }
        else//攻撃対象がplayerのとき
        {
            GameObject.Find( "NetworkManager").GetComponent<NetworkManager>().photonView.RPC( "MinionDamage", PhotonTargets.All, attackTarget.GetPhotonView().ownerId, dmg);
        }

        yield return new WaitForSeconds( 3.0f / localVariables.AttackSpeed);

        isRunning = false;

        yield break;
    }


    private void Update()
    {
        //searchTaskのlistに要素がない場合目的地に向かって動き出す
        if ( searchTask.Count == 0)
        {
            if ( !this.gameObject.transform.root.gameObject)
                return;
            agent.isStopped = false;
        }

        //searchTaskのListの第一要素がnullの場合removeする
        if ( searchTask.Count != 0 && searchTask[0] == null)
        {
            searchTask.RemoveAt(0);
        }

        if ( searchTask.Count != 0 && searchTask[0] != null)
        {
            transform.LookAt( searchTask[0].transform.position);
        }

        if ( searchTask.Count >= 1)
        {
            if ( AttackRange(searchTask[0], this.gameObject) >= searchRange * searchRange)
            {
                searchTask.Remove( searchTask[0]);
            }
        }
    }
}