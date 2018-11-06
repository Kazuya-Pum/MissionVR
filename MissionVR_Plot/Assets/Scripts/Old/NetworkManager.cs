#define Debug
//#define Play
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState
{
    idle, Game, Result,Dead
}

public class NetworkManager : Photon.MonoBehaviour
{
    [SerializeField] private float respawnTime;//リスポーンにかかる時間
    private float respawmTimer;
    private bool respawned;
    Chara chara;


    private int blackPeople;//ブラックチームの人数
    private int whitePeople;//ホワイトチームの人数
    private int selectedCharaID;//選択されたキャラのID
    [SerializeField] private int playPeople;//プレイ人数
    private float gameEndTime;//ゲームを終わる時間
    private float gameTimer;//計測器
    private float x, y, z;
    private GameObject player;
    private GameObject whiteProjector;//最終破壊目標
    private GameObject blackProjector;//最終破壊目標
    private TeamColor teamColorPlayerSave;//チームカラー保存用
    private ObjectKind objectKindSave;//オブジェクトの種類保存用
    private Vector3 pos;

    public bool isStart;//ゲームが開始されたかどうか
//    private bool respawned;
    public bool inRoom;
//    private bool respawnFlag = false;
//    [SerializeField]
//    private const float respawnTime = 5;
//    private float respawnTimer;
    GameState gameStatePlayer;
    [SerializeField]
    GameObject[] WhiteTower;
    [SerializeField]
    GameObject[] BlackTower;
//    Chara chara;
    public SubLocalVariables[] subLocalVariables = new SubLocalVariables[10];
    [SerializeField]
    private int expRange = 40;
    [SerializeField]
    private int killPlayerMoney = 300;//プレイヤーをキルしたときに入るmoney
    [SerializeField]
    private int killPlayerExp = 100;//プレイヤーをキルしたときに入るexp

    #region Announce
    [SerializeField] private float timeTextVisible = 1.0f; // テキストが表示される時間
//    [SerializeField] private Text text1;  // アナウンスを表示するTextオブジェクト
//    [SerializeField] private Text text2;    // 同時にアナウンスを表示するテキスト(空でも動作する)
    private Queue<string> announceTask = new Queue<string>();   // テキストをため込むQueue
    private bool isRunning = false; // テキストが表示中か否か
    #endregion

    [PunRPC]
    public void SendDamage(int playerID,int otherPlayerID,int otherPhotonViewID)
    {
        if (PhotonNetwork.isMasterClient)
        {
            int damage = (int)subLocalVariables[playerID - 1].PhysicalOffence;
            photonView.RPC("RaceiveDamage", PhotonTargets.All, playerID, otherPlayerID,damage,otherPhotonViewID);
        }
    }

    [PunRPC]
    public void SendSkillDamage(int playerID , int otherPlayerID ,int skillDmage, int otherPhotonViewID)
    {
        if (PhotonNetwork.isMasterClient)
        {
            int damage = (int)skillDmage - (int)subLocalVariables[playerID - 1].MagicalDefence;
            photonView.RPC("ReceiveSkillDamage", PhotonTargets.All, playerID, otherPlayerID, damage, otherPhotonViewID);
        }
    }

    [PunRPC]
    public void Send_Exp_Money(int playerID, int sendExp, int sendMoney, int destroyID)
    {
        player.GetComponent<LocalVariables>().photonView.RPC("Recieve_Money_NPCExp", PhotonTargets.All, playerID, sendExp, sendMoney, destroyID);
    }

    //[PunRPC]
    //public void SendMoney(int playerID, int sendMoney)
    //{
    //    player.GetComponent<LocalVariables>().photonView.RPC("RecieveMoney", PhotonTargets.All, playerID, sendMoney);
    //}


    [PunRPC]
    public void RaceiveDamage(int playerID,int otherPlayerID,int damage,int otherPhotonViewID)
    {
        if(player.GetPhotonView().ownerId == otherPlayerID)
        {
            LocalVariables localVariables = player.GetComponent<LocalVariables>();
            if (localVariables.Hp - damage <= 0)
            {
                if(localVariables.Hp > 0)
                {
                    photonView.RPC("CallIfPlayerKilled", PhotonTargets.All, (int)PhotonView.Find(otherPhotonViewID).gameObject.GetComponent<LocalVariables>().team);
                    photonView.RPC("SendMoney", PhotonTargets.All, playerID, killPlayerMoney);
                    photonView.RPC("AttackEnemy", PhotonTargets.All, PhotonView.Find(otherPhotonViewID).transform.position, killPlayerExp, otherPlayerID);
                }
                localVariables.Hp = 0;
            }
            else
            {
                localVariables.Hp -= damage;
            }

            photonView.RPC("RecieveChangedHP", PhotonTargets.MasterClient, player.gameObject.GetPhotonView().ownerId, localVariables.Hp);
        }
    }

    [PunRPC]
    public void ReceiveSkillDamage(int playerID , int otherPlayerID , int damage, int otherPhotonViewID)
    {
        if (player.GetPhotonView().ownerId == otherPlayerID)
        {
            LocalVariables localVariables = player.GetComponent<LocalVariables>();
            if (localVariables.Hp - damage <= 0)
            {
                if (localVariables.Hp > 0)
                {
                    photonView.RPC("CallIfPlayerKilled", PhotonTargets.All, (int)PhotonView.Find(otherPhotonViewID).gameObject.GetComponent<LocalVariables>().team);
                    photonView.RPC("SendMoney", PhotonTargets.All, playerID, killPlayerMoney);
                    photonView.RPC("AttackEnemy", PhotonTargets.All, PhotonView.Find(otherPhotonViewID).transform.position, killPlayerExp, otherPlayerID);
                }
                localVariables.Hp = 0;
            }
            else
                localVariables.Hp -= damage;

            photonView.RPC("RecieveChangedHP", PhotonTargets.MasterClient, player.gameObject.GetPhotonView().ownerId, player.GetComponent<LocalVariables>().Hp);
        }
    }

    [PunRPC]
    void AttackEnemy(Vector3 otherPlayerDeadPoint, int exp, int deadPlayerID)
    {
        if ((player.gameObject.GetPhotonView().ownerId + deadPlayerID) % 2 == 1)
        {
            if (Vector3.Distance(player.transform.position, otherPlayerDeadPoint) < expRange)
                player.GetComponent<LocalVariables>().photonView.RPC("RecieveExp", PhotonTargets.All, exp); ;
        }
    }

    void Start()
    {
        blackPeople = 0;
        whitePeople = 0;
        selectedCharaID = PlayerPrefs.GetInt("SelectChara" + photonView.ownerId);
 //       playPeople = 6;
        gameEndTime = 1200;
#if Debug
        PhotonNetwork.ConnectUsingSettings(null);
#endif
        //DontDestroyOnLoad(this.gameObject);
        //roomFlag = false;
        isStart = true;
        inRoom = false;
        //        respawnTimer = 0;
        respawned = false;
        respawnTime = 5f;
#if Play
        JoinTeam((PhotonNetwork.room.PlayerCount % 2 == 0) ? "Black" : "White");
#endif
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SaveInfo();
            isStart = false;
            SceneManager.LoadScene("Result");
        }

        if (!inRoom) return;
        //ルームに存在するプレイヤーの数を数えて、一定値になったら開始
        if (PhotonNetwork.playerList.Length == playPeople)
        {
            isStart = true;
        }
        else
        {
            isStart = false;
        }

        //制限時間管理
        if (isStart)
        {
            //プレイヤーが存在していて
            if (player != null)
            {
                if (!player.GetComponent<Chara>().photonView.isMine) return;
                //死んだらリスポーン
                if (player.GetComponent<LocalVariables>().Hp <= 0 && !respawned)
                {
                    respawned = true;
                    chara.gameState = GameState.Dead;
                    //ここでデス数を＋１したい
                    chara.IsShooted = false;
                }
                if (respawned)
                {
                    respawmTimer += Time.deltaTime;
                    if (respawmTimer > respawnTime)
                    {
                        Respawn();
                        respawmTimer = 0;
                        respawned = false;
                    }
                }

                //if (PhotonNetwork.isMasterClient)
                //{
                //プロジェクター破壊でゲームオーバー
                if (whiteProjector.GetComponent<LocalVariables>().Hp <= 0 || blackProjector.GetComponent<LocalVariables>().Hp <= 0)
                {
                    isStart = false;
                    //情報保存
                    SaveInfo();
                    SceneManager.LoadScene("Result");
                }
                    //                if (projector.GetComponent<LocalVariables>().Hp < 0) GameOver("Result");
                //}

                //制限時間が経過したらゲームエンド
                gameTimer += Time.deltaTime;
                if (gameTimer > gameEndTime)
                {
                    

                    SaveInfo();
                    isStart = false;
                    SceneManager.LoadScene("Result");
                }
            }
        }
    }

    private void SaveInfo()
    {
        for (int index = 0; index < subLocalVariables.Length; index++)
        {
            //獲得金額保存
            PlayerPrefs.SetInt("Money" + index, subLocalVariables[index].money);
            //キル数保存
            PlayerPrefs.SetInt("KillCount" + index, subLocalVariables[index].killCount);
            //デス数保存
            PlayerPrefs.SetInt("DeathCount" + index, subLocalVariables[index].deathCount);

            
        }
        //勝敗を決める必要があったので、暫定で総キル数の多い方を勝者にしました
        int killCountWhite = 1;
        int killCountBlack = 0;
        for (int index = 0; index < subLocalVariables.Length; index++)
        {
            if (subLocalVariables[index].team == TeamColor.White) killCountWhite += subLocalVariables[index].killCount;
            else
                killCountBlack += subLocalVariables[index].killCount;
        }

        //チームに応じて勝敗を分けて保存
        if (teamColorPlayerSave == TeamColor.White)
        {
            PlayerPrefs.SetString("WinOrLose", (killCountWhite > killCountBlack) ? "Win" : "Lose");
        }
        else
        {
            PlayerPrefs.SetString("WinOrLose", (killCountBlack > killCountWhite) ? "Win" : "Lose");
        }
    }

    [PunRPC]
    public void MinionDamage(int toPlayerID,int dmg)//Allでminionから呼ばれる
    {
        if(player.GetPhotonView().ownerId == toPlayerID)
        {
            LocalVariables localVariavles = player.GetComponent<LocalVariables>();
            if (localVariavles.Hp - dmg <= 0)
                localVariavles.Hp = 0;
            else
                localVariavles.Hp -= dmg;
            photonView.RPC("RecieveChangedHP", PhotonTargets.MasterClient, player.gameObject.GetPhotonView().ownerId, localVariavles.Hp);
        }
    }

    [PunRPC]
    public void MinionKillMinion(int destroyViewID)
    {
        player.GetComponent<LocalVariables>().photonView.RPC("DestroyMinion", PhotonTargets.All, destroyViewID);
    }

    [PunRPC]
    public void RecieveChangedHP(int changedPlayerID, int changedHP)
    {
        if (PhotonNetwork.isMasterClient)
        {
            this.subLocalVariables[changedPlayerID - 1].Hp = changedHP;
        }
    }

    void OnJoinedLobby()//ロビーに入ると呼ばれる
    {
        Debug.Log("ロビーに入った");
        //flag++;
        //ルームに入室する
        PhotonNetwork.JoinRandomRoom();
    }

    void OnJoinedRoom()//ルームに入室すると呼ばれる
    {
        Debug.Log("ルームに入室した");
        inRoom = true;
        /**ルームに入ったら、チーム分けを行う
         * 現在フィールドにいるプレイヤーを取得し、
         * 各チームの人数の状況に応じてどちらのチームに配属させるかを決める
         * 生成地点を２つ設けて、どちらのチームに配属されるかによって変える　
         */
        //プレイヤーを探す
        JoinTeam((PhotonNetwork.room.PlayerCount % 2 == 0) ? "Black" : "White");//if-elseの簡略化式
    }

    void OnPhotonRandomJoinFailed()//ルームの入室に失敗すると呼ばれる。失敗するのは、ルームがないから
    {
        Debug.Log("ルームの入室に失敗した");

        RoomOptions roomOption = new RoomOptions();

        //ルームの最大に人数指定
        roomOption.MaxPlayers = 10;

        //ルーム入室の許可
        roomOption.IsOpen = true;

        //このルームが一覧に表示されるか否か
        roomOption.IsVisible = true;

        //ルームを作成する　引数でルーム名を指定可能
        PhotonNetwork.CreateRoom("Room1", roomOption, null);
    }

    void JoinTeam(string team)
    {
        subLocalVariables = new SubLocalVariables[10];
        for (int n = 0; n < subLocalVariables.Length; n++)
        {
            subLocalVariables[n] = new SubLocalVariables();
        }


        Debug.Log(team);
        //Debug.Log("生成完了");
        //Vector3 pos;
        //位置をずらす度合をランダムに決定
        x = Random.Range(-5f, 5f);
        y = 2f;
        z = Random.Range(-5f, 5f);

        switch (team)
        {
            case "Black":
                //Playerのオブジェクトを生成
                //選択されたキャラのIDに応じて、生成するモデルを変える必要あり IDを入れる変数は宣言済み
                player = (GameObject)PhotonNetwork.Instantiate("CapsulePlayer", GameObject.Find("PlayerSpawnPos1").transform.position, Quaternion.identity, 0);
//                Chara playerChara = player.GetComponent<Chara>();
                chara = player.GetComponent<Chara>();
                chara.enabled = true;
                chara.initTeamColorPlayer = TeamColor.Black;
                teamColorPlayerSave = TeamColor.Black;
                player.GetComponent<LocalVariables>().team = chara.initTeamColorPlayer;
                chara.objectKind = ObjectKind.Player;
                objectKindSave = ObjectKind.Player;
                chara.StartSetting();
                //chara.hpBar_OnHead.gameObject.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = Color.red;


                //位置調整
                pos = GameObject.Find("PlayerSpawnPos1").transform.position;
                pos.x += x;
                pos.z += z;
                player.transform.position = pos;

                //角度調整
                Quaternion angle = GameObject.Find("PlayerSpawnPos1").transform.rotation;
                angle.y = -90;
                player.transform.rotation = angle;

//                projector = GameObject.Find("BlackProjector");

                break;
            case "White":
                //Playerのオブジェクトを生成
                player = (GameObject)PhotonNetwork.Instantiate("CapsulePlayer", GameObject.Find("PlayerSpawnPos1").transform.position, Quaternion.identity, 0);
//                Chara playerCharaa = player.GetComponent<Chara>();
                chara = player.GetComponent<Chara>();
                chara.enabled = true;
                chara.initTeamColorPlayer = TeamColor.White;
                teamColorPlayerSave = TeamColor.White;
                player.GetComponent<LocalVariables>().team = chara.initTeamColorPlayer;
                chara.objectKind = ObjectKind.Player;
                objectKindSave = ObjectKind.Player;
                chara.StartSetting();
                //chara.hpBar_OnHead.gameObject.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = Color.blue;

                //位置調整
                pos = GameObject.Find("PlayerSpawnPos2").transform.position;
                pos.x += x;
                pos.z += z;
                player.transform.position = pos;

//                projector = GameObject.Find("WhiteProjector");
                //スポーン地点２は角度調整が不要
                break;
        }

        whiteProjector = GameObject.Find("WhiteProjector");
        blackProjector = GameObject.Find("BlackProjector");
        gameStatePlayer = player.GetComponent<Chara>().gameState;
        MiniMapManager minimapManager = GameObject.Find("MiniMap").GetComponent<MiniMapManager>();
        minimapManager.Player = player.GetComponent<Transform>();
        chara = player.GetComponent<Chara>();

        //roomFlag = true;

    }

    void Respawn()
    {
        //ステータスの初期化
        LocalVariables playerLocalVariables = player.GetComponent<LocalVariables>();
        playerLocalVariables.Hp = playerLocalVariables.MaxHp;
        playerLocalVariables.Mana = playerLocalVariables.ManaMax;

        photonView.RPC("RecieveChangedHP", PhotonTargets.MasterClient, player.gameObject.GetPhotonView().ownerId, playerLocalVariables.Hp);

        chara.gameState = GameState.Game;

        switch (teamColorPlayerSave)
        {
            case TeamColor.White:
                //位置調整
                pos = GameObject.Find("PlayerSpawnPos2").transform.position;
                pos.x += x;
                pos.z += z;
                player.transform.position = pos;
                break;

            case TeamColor.Black:
                //位置調整
                pos = GameObject.Find("PlayerSpawnPos1").transform.position;
                pos.x += x;
                pos.z += z;
                player.transform.position = pos;

                //角度調整
                Quaternion angle = GameObject.Find("PlayerSpawnPos1").transform.rotation;
                angle.y = -90;
                player.transform.rotation = angle;
                break;
        }
    }

    void GameOver(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

#region AnnounceMethod
    /// <summary>
    /// アナウンスを開始する関数
    /// </summary>
    /// <param name="str">追加するテキスト</param>
    public void StartAnnounce(string str)
    {
        announceTask.Enqueue(str);
        if (!isRunning)
        {
            StartCoroutine(Announce());
        }
    }

    /// <summary>
    /// アナウンスを表示する関数
    /// </summary>
    /// <returns>time変数の長さ毎に表示を更新</returns>
    private IEnumerator Announce()
    {
        isRunning = true;

        while (announceTask.Count > 0)
        {
            string str = announceTask.Dequeue();
             chara.text1.text = str;
            //Debug.Log( "Announce : " + str );

            if (chara.text2 != null)
            {
                chara.text2.text = str;
            }

            yield return new WaitForSeconds(timeTextVisible);
            if (announceTask.Count <= 0)
            {
                HideText();
                isRunning = false;
                yield break;
            }
        }
    }

    /// <summary>
    /// アナウンスを非表示にする関数
    /// </summary>
    private void HideText()
    {
        chara.text1.text = null;
        if (chara.text2 != null)
        {
            chara.text2.text = null;
        }
    }

    ///<summary>
    ///ここからアナウンスの内容
    /// </summary>

    [PunRPC]//破壊されたタワーがどちらのチームに属するかでメッセージが変わる
    public void CallIfTowerDestroyed(int teamColor)
    {
        string message = ((TeamColor)teamColor == player.GetComponent<LocalVariables>().team) ? "味方のタワーが破壊されました" : "敵のタワーを破壊しました";
        StartAnnounce(message);
    }

    [PunRPC]//キルされたプレイヤーがどちらのチームに属するかでメッセージが変わる
    public void CallIfPlayerKilled(int teamColorOfPlayer)
    {
        string message = ((TeamColor)teamColorOfPlayer == player.GetComponent<LocalVariables>().team) ? "味方が倒されました" : "敵を倒しました";
        StartAnnounce(message);
    }

    public void CallIfPlayerLevelUpped()
    {
        string message = "レベルが上がりました";
        StartAnnounce(message);
    }
#endregion

}