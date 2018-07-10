using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MinionSpawnController : Photon.MonoBehaviour {

    #region Variables
    //private string[] Minion = new string[3]{"MeleeMinion","MiddleMinion","RangedMinion"};//出すミニオン

    [SerializeField]
    private GameObject[] Minion;

    [SerializeField]//ミニオンがスポーンする位置の空オブジェクト
    private GameObject[] Minion_SpawnPointObject_White;

    [SerializeField]//ミニオンがスポーンする位置の空オブジェクト
    private GameObject[] Minion_SpawnPointObject_Black;

    [SerializeField]
    private Transform[] Projector;//プロジェクター

    [SerializeField]
    private int spawnNum;//1レーンの1ウェーブでスポーンするミニオンの数

    [SerializeField]
    private float spawnInterval;//ミニオン'ウェーブ'のスポーン間隔

    [SerializeField]
    private float spawnContinuityInterval;//ミニオン'単体'が出る間隔
    
    // 0:Red 1:Blue
    [SerializeField]
    private Material[] minionMat;

    private bool inNetworkFlag = false;

    // NavMeshAgentに設定するmask
    private int topMask = 1 << 3, midMask = 1 << 4, botMask = 1 << 5;

    #endregion

    // Use this for initialization
    void Start () {
        //スポーンのコルーチン開始
//        StartCoroutine(Spawn());
    }

    private void Update()
    {
        NetworkManager networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        if (networkManager.isStart)
        {
            if (!inNetworkFlag)
            {
                if (!PhotonNetwork.isMasterClient) return;
                if (GameObject.FindGameObjectWithTag("Player").GetComponent<Chara>().GetGameState != GameState.Game) return;
                StartCoroutine(Spawn());
                inNetworkFlag = !inNetworkFlag;
            }
        }
    }

    //スポーンする処理
    IEnumerator Spawn()
    {
        int count = 1;
        for (;;)
        {
            // どちらのチームか
            for (int n = 0; n < 2; n++)
            {
                // どのレーンか
                for (int m = 0; m < 3; m++)
                {
                    int minionViewID = PhotonNetwork.AllocateViewID();
                    photonView.RPC( "SpawnMinion", PhotonTargets.AllBuffered, (TeamColor)n, (Lane)m, count, minionViewID);
                }
            }
            if(count < spawnNum)//ミニオン単体の湧き感覚
            {
                yield return new WaitForSeconds(this.spawnContinuityInterval);
                count += 1;
            }
            else//1ウェーブの湧き感覚
            {
                count = 1;
                yield return new WaitForSeconds(this.spawnInterval);
            }
        }
    }

    //ミニオンをInstantiateするメソッド
    //引数は(このミニオンのチーム,このミニオンのレーン,一つのウェーブの数)
    [PunRPC]
    void SpawnMinion(TeamColor team, Lane lane,int count, int minionViewID)
    {
        int playSide = (int)team;
        int playLane = (int)lane;
        GameObject obj;

        // minionを生成する＆とりあえず色を変える
        switch (playSide)
        {
            case 0:
                obj = Instantiate( Minion[count - 1], this.Minion_SpawnPointObject_White[playLane].transform.position, Quaternion.identity);
                obj.GetComponent<PhotonView>().viewID = minionViewID;
                break;

            case 1:
                obj = Instantiate( Minion[count - 1], this.Minion_SpawnPointObject_Black[playLane].transform.position, Quaternion.identity);
                obj.GetComponent<PhotonView>().viewID = minionViewID;
                break;

            default:
                return;
        }

        obj.GetComponent<Renderer>().material = minionMat[playSide];

        // minionのagentmask
        LocalVariables teamAndLane = obj.GetComponent<LocalVariables>();
        teamAndLane.team = team;
        teamAndLane.lane = lane;

    }

    //コルーチン終了Method
    void StopCoroutine()
    {
        StopCoroutine(Spawn());
    }
}
