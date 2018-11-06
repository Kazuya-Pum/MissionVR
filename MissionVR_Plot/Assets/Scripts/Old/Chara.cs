/**開発メモ
 
 */


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MOBAEngine.Skills;


//操作キャラのスクリプト

//腰撃ちとADSを切り替えるためのenum
public enum CameraState
{
    Hip , ADS
}

//プレイヤーが操作しているかAIかを識別するためのenum
public enum ObjectKind
{
    Player , AI
}

public class Chara : Photon.MonoBehaviour ,IPlayer{

    [HideInInspector] public ShopManager shopManager;

    private Vector2 usePosition;//スキル(アイテム)を使う際に置く場所
    private Vector2 standbyPosition;//スキル(アイテム)を使わない際に置く場所
    private Vector2 useScale;//スキル(アイテム)を使う際のスケール   
    private Vector2 standbyScale;//スキル(アイテム)を使わない際のスケール
    private bool skillMode;//スキルモード　true：スキル　false：アイテム


    [SerializeField] private Image[] skills = new Image[4];//スキルの使用法を変えるために用意
    [SerializeField] private Image[] items = new Image[4];//アイテムの使用法を変えるために用意
    [SerializeField] private GameObject ImageSkillParent;//スキルのImageの親
    [SerializeField] private GameObject ImageItemParent;//アイテムのImageの親


    #region Skills
    [Header("Area Skill")]
    [SerializeField] protected AreaSkillBase flameRadiation;
    [SerializeField] protected AreaSkillBase request;
    [Space(5)]
    [Header("Lay Skill")]
    [SerializeField] protected LaySkillBase railGun;
    [SerializeField] protected LaySkillBase tazaGun;
    [Space(5)]
    [Header("Throw Skill")]
    [SerializeField] protected ThrowSkillBase armor_PiercingAmmunition;
    [SerializeField] protected ThrowSkillBase explosionBomb;
    [SerializeField] protected ThrowSkillBase flameBullet;
    [SerializeField] protected ThrowSkillBase grenade;
    [SerializeField] protected ThrowSkillBase heavyBullet;
    [SerializeField] protected ThrowSkillBase missile;
    [SerializeField] protected ThrowSkillBase pierceShot;
    [SerializeField] protected ThrowSkillBase shot;
    [Space(5)]
    [Header("Token Skill")]
    [SerializeField] protected TokenSkillBase mine;
    [SerializeField] protected TokenSkillBase drone;
    private IPlayer IP;

    #endregion

    private float isFiredDamageTime;//燃焼状態でダメージを受ける間隔
    private float timeFire;//燃焼状態になっている時間

    public bool isFired;//燃焼状態のときTrue
    private bool isShopMode;


    #region Paramaters
#if UNITY_ANDROID
    [SerializeField, Range(1, 10)] private float senci;//右スティックの感度
    private bool r2Downed;//R2スティックを押したかどうか
    private bool r2Active;
#endif

    [SerializeField] protected GameObject muzzle;/*銃口の空オブジェクト*/
    [SerializeField] protected GameObject explainerBoard;/*装備の説明Textを載せるImage*/
    [SerializeField] protected Camera Hip;/*腰撃ち用カメラ*/
    [SerializeField] protected Camera ADS;/*ADS用カメラ*/
    [SerializeField] protected float force;/*弾速*/
    [SerializeField] protected float fireRate;/*連射速度*/
    [SerializeField] protected float[] coolDownSkill;/*スキルのクールダウン時間*/
    [SerializeField] protected float[] coolDownItem;/*各アイテムのクールダウン時間*/
    [SerializeField] protected bool[] skillIsActive;/*各スキルが使用可能かどうか*/
    [SerializeField] protected bool[] itemIsActive;/*各アイテムが使用可能かどうか*/
    [SerializeField] protected LayerMask layerMask;/*Rayを飛ばすLayerを指定*/
    [SerializeField] protected string[] equipmentRefferrence;/*IDに応じて、装備の説明を表示*/
    [SerializeField] protected Text[] equipmentExplainer;/*装備説明Text*/
    [SerializeField] protected Slider hpBar;/*hp表示のSlider*/
    [SerializeField] protected Slider manaBar;/*mana表示のSlider*/
    [SerializeField] protected Button[] skill = new Button[4];
    public Button[] item = new Button[4];
    //[SerializeField] public Slider hpBar_OnHead;/*頭上のhp表示のSlider*/

    //private FirstPersonController fpc;
    protected CameraState cameraState;
    protected int selectStateCount;/*選択するボタンの種類を変えるための変数*/
    public int initHpMax;/*最大体力*/
    public int initManaMax;/*最大マナ*/
    protected int[] equipmentId = new int[7];/*チャンピオンの装備のID　複数の装備を持てるようにしている*/
    protected float time;/*計測時間*/
    protected float recoil;//リコイル値
    protected float[] skillTime;/*スキルのクールダウン計測時間*/
    protected float[] itemTime;/*アイテムのクールダウン計測時間*/
    //protected bool firstBullet;/*初弾を撃ったかどうか*/
//    public PhotonView photonView;
    protected Vector3 cameraCenter;//画面の中央
    protected List<int> equipmentListEnemy = new List<int>();/*敵の装備のIDを格納するためのリスト*/
    protected float poisonTime;//ポイズン状態になっている時間
    protected float isPoisonDamagedTime;//ポイズン状態でダメージを受ける間隔
    protected float timePoison;//スローになっている時間
    protected float timeStun;//スタンになっている時間
    protected float timeSlow;//スローになっている時間

    private int startCount;
    private float runSpeed;
    private float gameStartTime;//ゲームスタートまでの時間
    private float gameStartTimer;//計測するやつ
    private bool isShooted;//銃を撃ったかどうか
    private bool isShooting;//射撃中かどうか
    private bool isHealig;//回復中かどうか
    private float shootTimer;//計測時間
    public GameState gameState;
    public GameState GetGameState { get { return gameState; } }
    public NetworkManager networkManager;
    public bool IsShooted { get { return isShooted; } set { isShooted = value; } }
    private int[] skillUseCount;//スキル使用回数
    private int[] skillUseCountToLevelUp;//レベルアップに必要なスキル使用回数
    private int[] skillLevel;//スキルのレベル
    

    [SerializeField] private Transform verticalRotate;//プレイヤー
    [SerializeField] private Transform horizontaoRotate;//カメラ
    //[SerializeField] private GameObject canvasUI;
    [SerializeField] private Canvas canvasUI;
    [SerializeField] private Button startTimer;
    [SerializeField] private CriAtomListener audioListner;
//    [SerializeField] private CriAtomSource audioPlayerBGM;//BGM再生用AudioSource
//    [SerializeField] private CriAtomSource runSE;
//    [SerializeField] private AudioClip battleBGM;//戦闘BGM

    public int id;
    public int championID;/*チャンピオン管理用ID : 0,1,2,3,4,5,・・・*/
    //public int killCount;
    //public int deathCount;
    public int[] havingItemID;//所持しているアイテムのIDを格納
    //public int[] havingItemCount;//所持しているアイテムの個数を格納
    private float[] manaToUseSkill;/*スキル使用に必要なマナ*/
//    public float hpPlayer;/*プレイヤーの体力*/
    [HideInInspector] public float initMana;/*プレイヤーのマナ　スキルを使うと消費する*/
    [HideInInspector] public float initMoveSpeed;
    [HideInInspector] public float manaRecoverCount;
    //public int money;/*お金*/
    //public List<int> equipmentList = new List<int>();/*自キャラの装備のIDを格納するためのリスト*/
    public bool isStun;//スタン状態かどうか
    public bool isPoson;//ポイズン状態かどうか
    public bool isSlow;//スロー状態かどうか
    public TeamColor initTeamColorPlayer;//自分のチームカラー
    public ObjectKind objectKind;

    [HideInInspector] public LocalVariables playerLocalVariables;
    //int nowLevel = 1;
    private CriAtomSource source;
    private CriAtomSource runSE;
    private CriAtomSource BGM;
    Rigidbody rigidbody;

    [SerializeField]
    private Text LevelText;

    [SerializeField]
    private Image Status_PhysicalAttack;
    [SerializeField]
    private Image Status_SpecialAttack;
    [SerializeField]
    private Image Status_PhysicalDefence;
    [SerializeField]
    private Image Status_SpecialDefence;
    [SerializeField]
    private Image Status_Speed;

    public float sliderHpPlayer = 0;

    float manaDeclien_StartTime;
    float manaAutoRecoveryinterval;
    float hpDeclien_StartTime;
    float hpAutoRecoveryinterval;

    #endregion

    #region Announce
    //[SerializeField] private float timeTextVisible = 1.0f; // テキストが表示される時間
    public Text text1;  // アナウンスを表示するTextオブジェクト
    public Text text2;    // 同時にアナウンスを表示するテキスト(空でも動作する)
    //private Queue<string> announceTask = new Queue<string>();   // テキストをため込むQueue
    //private bool isRunning = false; // テキストが表示中か否か
    #endregion


    void Start () {
        
//        StartSetting();
        
    }


    public void StartSetting()
    {
        if (photonView.isMine)//thisオブジェクトが自分自身が生成したものだったら
        {
            //初動で選択されたキャラのIDによってパラメータを変える必要あり
            playerLocalVariables = this.gameObject.GetComponent<LocalVariables>();
            runSE = this.gameObject.transform.GetComponentInChildren<CriAtomSource>();
            BGM = GameObject.Find("Ray01(CriAtomSource)").GetComponent<CriAtomSource>();
            rigidbody = this.gameObject.GetComponent<Rigidbody>();
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            usePosition = ImageSkillParent.transform.localPosition;
            standbyPosition = ImageItemParent.transform.localPosition;
            useScale.x = 1;
            useScale.y = 1;
            standbyScale.x = 0.6f;
            standbyScale.y = 0.6f;

            runSpeed = 1;
#if UNITY_ANDROID
            senci = 5;
#endif

            IP = this.gameObject.GetComponent<Chara>();
            gameState = GameState.idle;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            canvasUI.enabled = true;
            Hip.enabled = true;
            audioListner.enabled = true;
            //fpc = FPSController.GetComponent<FirstPersonController>();
            //fpc.m_RunSpeed = 10;
            force = 5000;
            fireRate = 0.3f;
            coolDownSkill = new float[] { 1.0f, 1.5f, 2.0f, 3.0f };
            coolDownItem = new float[] { 0, 0, 0, 0, 0 };
            skillIsActive = new bool[] { false, false, false, false };
            itemIsActive = new bool[] { false, false, false, false, false };
            for(int index = 0; index < item.Length; index++)
            {
                item[index].GetComponent<Image>().enabled = false;
                items[index].GetComponent<Image>().enabled = false;
            }
            equipmentRefferrence = new string[] { "Weapon1" };
            selectStateCount = 0;
            cameraState = CameraState.Hip;
            skillTime = new float[] { 0, 0, 0, 0 };
            itemTime = new float[] { 0, 0, 0, 0, 0 };
            skillUseCount = new int[4] { 0, 0, 0, 0 };
            skillUseCountToLevelUp = new int[4] { 5, 3, 2, 1 };
            skillLevel = new int[4] { 1, 1, 1, 1 };
            //firstBullet = false;
            isShooted = false;
            cameraCenter = new Vector3(Screen.width / 2, Screen.height / 2);

            //--------------------------ここからHpとMana初期化------------------------------//

            initHpMax = 100;
            initManaMax = 100;
            playerLocalVariables.MaxHp = initHpMax;
            playerLocalVariables.ManaMax = initManaMax;
            playerLocalVariables.Hp = (int)initHpMax;
            playerLocalVariables.Mana = (int)initManaMax;

            //-------------------------ここまでHpとMana初期化-------------------------------//

            championID = 0;
            havingItemID = new int[4] { 0, 0, 0, 0 };
            //havingItemCount = new int[4] { 0, 0, 0, 0 };
            manaToUseSkill = new float[4] { 20, 15, 30, 80 };
            //playerLocalVariables.Hp = hpMax;
            //mana = manaMax;
            manaRecoverCount = 0;
            //money = 100;
            startCount = 3;
            skillMode = true;

            for (int a = 0; a < 7; a++)
            {
                //装備のIDを格納
                equipmentId[a] = 0;
            }
            //Listに変換
            //equipmentList.AddRange(equipmentId);
            explainerBoard.SetActive(false);
            isPoson = false;
            isStun = false;
            isSlow = false;
            isHealig = false;
            isShooting = false;
            timePoison = 10f;
            isPoisonDamagedTime = 1f;
            timeSlow = 10f;
            timeStun = 10f;
            recoil = 10;
            networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
            BGM.Play();
            isShooted = false;
            shopManager = GameObject.Find("ShopManager").GetComponent<ShopManager>();
            shopManager.p_localVariables = this.gameObject.GetComponent<LocalVariables>();
            shopManager.chara = this;
            //StartCoroutine(Shot(fireRate));

            //--------------------------ここからキャラステ初期化--------------------------//

            playerLocalVariables.Level = 1;
            playerLocalVariables.PhysicalOffence = 5;
            playerLocalVariables.MagicalOffence = 1;
            playerLocalVariables.PhysicalDefence = 1;
            playerLocalVariables.MagicalDefence = 1;
            playerLocalVariables.AutomaticManaRecovery = 3;
            playerLocalVariables.AutomaticHpRecovery = 3;
            initMoveSpeed = 10;
            playerLocalVariables.MoveSpeed = initMoveSpeed;
            Status_PhysicalAttack.fillAmount = playerLocalVariables.PhysicalOffence * 0.1f;
            Status_SpecialAttack.fillAmount = playerLocalVariables.MagicalOffence * 0.1f;
            Status_PhysicalDefence.fillAmount = playerLocalVariables.PhysicalDefence * 0.1f;
            Status_SpecialDefence.fillAmount = playerLocalVariables.MagicalDefence * 0.1f;
            Status_Speed.fillAmount = playerLocalVariables.MoveSpeed * 0.01f;
            LevelText.text = "Lv." + playerLocalVariables.Level;
            manaAutoRecoveryinterval = 5;
            hpAutoRecoveryinterval = 5;

            //---------------------------キャラステ初期化ここまで----------------------------//

            photonView.RPC("LevelUp", PhotonTargets.MasterClient, playerLocalVariables.Level, this.gameObject.GetPhotonView().ownerId,
                playerLocalVariables.MaxHp, playerLocalVariables.PhysicalOffence, playerLocalVariables.PhysicalDefence,
                playerLocalVariables.MagicalOffence, playerLocalVariables.MagicalDefence, playerLocalVariables.MoveSpeed);
            networkManager.photonView.RPC("RecieveChangedHP", PhotonTargets.MasterClient, this.gameObject.GetPhotonView().ownerId, playerLocalVariables.Hp);
        }
    }

    [PunRPC]
    public void LevelUp(int level,int playerID, int MaxHP, float PhysicalOffence, float PhysicalDefence,
                        float MagicalOffence, float MagicalDefence, float MoveSpeed)
    {
        if (!PhotonNetwork.isMasterClient)
            return;

        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();

        //レベルが上がったら
        //        if (networkManager.subLocalVariables[playerID - 1].Level < level)
        //        {
        /*
         * 一気に2レベル以上上がった場合
         * （内容にしたいけどレベルデザイン前はありえそうなので一応）
         */
        //            for (int count = 0; count < level - networkManager.subLocalVariables[playerID - 1].Level; count++)
        //            {
        networkManager.subLocalVariables[playerID - 1].MaxHp = MaxHP;
                //hpMax = playerLocalVariables.MaxHp;
                //hpBar_OnHead.maxValue = hpMax;
                //hpBar.maxValue = hpMax;
                networkManager.subLocalVariables[playerID - 1].PhysicalOffence = PhysicalOffence;
                networkManager.subLocalVariables[playerID - 1].PhysicalDefence = PhysicalDefence;
                networkManager.subLocalVariables[playerID - 1].MagicalOffence = MagicalOffence;
                networkManager.subLocalVariables[playerID - 1].MagicalDefence = MagicalDefence;
                networkManager.subLocalVariables[playerID - 1].MoveSpeed = MoveSpeed;
                networkManager.subLocalVariables[playerID - 1].Level = level;
//            }
//        }
    }

    void Update()
    {
        if (photonView.isMine)
        {
//            hpPlayer = playerLocalVariables.Hp;
//            mana = playerLocalVariables.Mana;
            
 //           sliderHpPlayer = hpPlayer;

            if (gameState == GameState.idle && networkManager.isStart)
            {
                gameStartTimer += Time.deltaTime;
                if (gameStartTimer >= 1)
                {
                    gameStartTimer = 0;
                    startCount--;
                }
                startTimer.GetComponent<Image>().fillAmount = gameStartTimer;
                startTimer.transform.GetChild(0).GetComponent<Text>().text = "" + startCount;
                if (startCount < 0)
                {
                    gameState = GameState.Game;
                    startTimer.gameObject.SetActive(false);
                    rigidbody.constraints = RigidbodyConstraints.None;
                    rigidbody.constraints = RigidbodyConstraints.FreezePositionY
                        | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                }
            }

            if (gameState == GameState.Game)
            {
                if (playerLocalVariables.Mana < playerLocalVariables.ManaMax)
                {
                    manaDeclien_StartTime += Time.deltaTime;
                    if (manaDeclien_StartTime >= manaAutoRecoveryinterval)
                    {
                        playerLocalVariables.Mana += playerLocalVariables.AutomaticManaRecovery;
                        manaDeclien_StartTime = 0;
                    }
                }

                if (playerLocalVariables.Hp < playerLocalVariables.MaxHp)
                {
                    hpDeclien_StartTime += Time.deltaTime;
                    if (hpDeclien_StartTime >= hpAutoRecoveryinterval)
                    {
                        playerLocalVariables.Hp += playerLocalVariables.AutomaticHpRecovery;

                        networkManager.photonView.RPC("RecieveChangedHP", PhotonTargets.MasterClient, gameObject.GetPhotonView().ownerId, playerLocalVariables.Hp);

                        hpDeclien_StartTime = 0;
                    }
                }

                if (isStun)//スタンは一定時間攻撃・移動・スキル全部使用不可能
                {
                    timeStun -= Time.deltaTime;
                    if (timeStun <= 0)
                    {
                        isStun = false;timeStun = 10f;
                    }
                }
                else
                {
                    ControlBasely();
                }

                if (isFired)
                {
                    timeFire -= Time.deltaTime;
                    isFiredDamageTime -= Time.deltaTime;
                    if (isFiredDamageTime <= 0)
                    {
                        playerLocalVariables.Hp -= 2;
                        isFiredDamageTime = 1;
                    }
                    if (timeFire <= 0)
                    {
                        isFired = false;
                        timeFire = 10f;
                    }

                }

                //２秒ごとにマナが１回復する
                if (playerLocalVariables.Mana < playerLocalVariables.ManaMax) manaRecoverCount += Time.deltaTime;

                if (manaRecoverCount >= 2 && playerLocalVariables.Mana < playerLocalVariables.ManaMax)
                {
                    manaRecoverCount = 0;
                    playerLocalVariables.Mana++;

                }
            }
        }
    }

    public void LevelUpMethod()
    {
        /*
         * 一気に2レベル以上上がった場合
         * （内容にしたいけどレベルデザイン前はありえそうなので一応）
         * 
         */
        playerLocalVariables.Level++;

        playerLocalVariables.MaxHp += 20;
        //hpBar_OnHead.maxValue = playerLocalVariables.MaxHp;
        playerLocalVariables.PhysicalOffence *= 1.1f;
        playerLocalVariables.PhysicalDefence += 1f;
        playerLocalVariables.MagicalOffence += 1f;
        playerLocalVariables.MagicalDefence += 1f;
        playerLocalVariables.MoveSpeed += 1f;

        Status_PhysicalAttack.fillAmount = playerLocalVariables.PhysicalOffence * 0.1f;
        Status_SpecialAttack.fillAmount = playerLocalVariables.MagicalOffence * 0.1f;
        Status_PhysicalDefence.fillAmount = playerLocalVariables.PhysicalDefence * 0.1f;
        Status_SpecialDefence.fillAmount = playerLocalVariables.MagicalDefence * 0.1f;
        Status_Speed.fillAmount = playerLocalVariables.MoveSpeed * 0.01f;

        //nowLevel++;
        LevelText.text = "Lv." + playerLocalVariables.Level;
        
        photonView.RPC("LevelUp", PhotonTargets.MasterClient, playerLocalVariables.Level, this.gameObject.GetPhotonView().ownerId, 
                        playerLocalVariables.MaxHp, playerLocalVariables.PhysicalOffence, playerLocalVariables.PhysicalDefence,
                        playerLocalVariables.MagicalOffence, playerLocalVariables.MagicalDefence,playerLocalVariables.MoveSpeed);
    }

    //[PunRPC]
    //public void RecieveMoney(int recievedPlayerID, int recievedMoney)
    //{
    //    if (this.gameObject.GetPhotonView().isMine)
    //    {
    //        if (this.gameObject.GetPhotonView().ownerId == recievedPlayerID)
    //        {
    //            money += recievedMoney;
    //            MoneyText.text = "" + money;
    //        }
    //    }
    //}


    #region インターフェースのため、記述
    public void Damage(int d)
    {
        this.playerLocalVariables.Hp = this.playerLocalVariables.Hp - d;
        //throw new NotImplementedException();
    }

    public Transform GetPlayerTransform()
    {
        //throw new NotImplementedException();
        return muzzle.transform;
    }
    #endregion



    //移動させる関数
    private void Move()
    {
        if (isShopMode) return;
#if UNITY_ANDROID
        if (Input.GetButtonDown("Run")) runSpeed = 1.5f;

        if (Input.GetButtonUp("Run")) runSpeed = 1;

        float xAxis = Input.GetAxis("Horizontal") * 2;
        float zAxis = Input.GetAxis("Vertical") * 2;

        Vector3 position_ = gameObject.transform.rotation * new Vector3(xAxis * playerLocalVariables.MoveSpeed * runSpeed, 0, zAxis * playerLocalVariables.MoveSpeed * runSpeed);
        gameObject.transform.position += position_ * Time.deltaTime;

        

#endif

#if UNITY_EDITOR
        //左Shiftを押しながらだと走る
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.LeftShift))
        {
            //isMaxSpeed = true;
            runSpeed = 1.5f;
        }
        else
        {
            runSpeed = 1;
        }

        if(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.A)
            || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            if (runSE.status != CriAtomSource.Status.Playing)
            {
                runSE.Play();
            }
        }
/*        if(!Input.GetKey(KeyCode.W)&& !Input.GetKey(KeyCode.A)&& !Input.GetKey(KeyCode.S)&& !Input.GetKey(KeyCode.D))
        {
            source.Stop();
        }
*/        if (Input.GetKey(KeyCode.W))
        {
            Vector3 position = gameObject.transform.rotation * new Vector3(0, 0, playerLocalVariables.MoveSpeed * runSpeed);
            gameObject.transform.position += position * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S))
        {
            Vector3 position = gameObject.transform.rotation * new Vector3(0, 0, -playerLocalVariables.MoveSpeed * runSpeed);
            gameObject.transform.position += position * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.D))
        {
            Vector3 position = gameObject.transform.rotation * new Vector3(playerLocalVariables.MoveSpeed * runSpeed, 0, 0);
            gameObject.transform.position += position * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A))
        {
            Vector3 position = gameObject.transform.rotation * new Vector3(-playerLocalVariables.MoveSpeed * runSpeed, 0, 0);
            gameObject.transform.position += position * Time.deltaTime;
        }
#endif
    }

#region SkillMethod
    //どちらかの引数をNullにすることで、この関数で完結した
    //と思ったが、スキルの種類が増えたため、現在は各スキルで関数を作っている。
    //第1引数：射出型スキル　第2引数：設置型スキル
    protected void SkillInterface(ThrowSkillBase throwSkill , TokenSkillBase tokenSkill)
    {
        playerLocalVariables.Mana -= throwSkill.ManaCost;
        if (throwSkill == null) tokenSkill.UseSkill(IP,gameObject);
        else
            throwSkill.UseSkill(IP,gameObject);
    }

    //ここから範囲スキル
    protected void FlameRadiation(AreaSkillBase flameRadiation)
    {
        playerLocalVariables.Mana -= flameRadiation.ManaCost;
        flameRadiation.UseSkill(IP, gameObject);
    }

    protected void Request(AreaSkillBase request)
    {
        playerLocalVariables.Mana -= request.ManaCost;
        request.UseSkill(IP, gameObject);
    }

    //ここから光線スキル
    protected void RailGun(LaySkillBase railGun)
    {
        playerLocalVariables.Mana -= railGun.ManaCost;
        railGun.UseSkill(IP, gameObject);
    }

    protected void TazaGun(LaySkillBase tazaGun)
    {
        playerLocalVariables.Mana -= tazaGun.ManaCost;
        tazaGun.UseSkill(IP, gameObject);
    }

    //ここから射出スキル
    protected void Armor_PiercingAmmunitioin(ThrowSkillBase armor_piercingAmmunitioni)
    {
        playerLocalVariables.Mana -= armor_piercingAmmunitioni.ManaCost;
        armor_piercingAmmunitioni.UseSkill(IP, gameObject);
    }

    protected void Drone(ThrowSkillBase drone)
    {
        playerLocalVariables.Mana -= drone.ManaCost;
        drone.UseSkill(IP, gameObject);
    }

    protected void ExplosionBomb(ThrowSkillBase explosionBomb)
    {
        playerLocalVariables.Mana -= explosionBomb.ManaCost;
        explosionBomb.UseSkill(IP, gameObject);
    }

    protected void FlameBullet(ThrowSkillBase flameBullet)
    {
        playerLocalVariables.Mana -= flameBullet.ManaCost;
        flameBullet.UseSkill(IP, gameObject);
    }

    protected void Grenade(ThrowSkillBase grenade)
    {
        playerLocalVariables.Mana -= grenade.ManaCost;
        grenade.UseSkill(IP, gameObject);
    }

    protected void HeavyBullet(ThrowSkillBase heavyBullet)
    {
        playerLocalVariables.Mana -= heavyBullet.ManaCost;
        heavyBullet.UseSkill(IP, gameObject);
    }

    protected void Missile(ThrowSkillBase missile)
    {
        playerLocalVariables.Mana -= missile.ManaCost;
        missile.UseSkill(IP, gameObject);
    }

    protected void PierceShot(ThrowSkillBase pierceShot)
    {
        playerLocalVariables.Mana -= pierceShot.ManaCost;
        pierceShot.UseSkill(IP, gameObject);
    }

    protected void Shot(ThrowSkillBase shot)
    {
        playerLocalVariables.Mana -= shot.ManaCost;
        shot.UseSkill(IP, gameObject);
    }

    //ここから設置型スキル
    protected void Mine(TokenSkillBase mine)
    {
        playerLocalVariables.Mana -= mine.ManaCost;
        mine.UseSkill(IP, gameObject);
    }


#endregion


    //腰撃ちとADSの切り替え
    private void CameraControl()
    {
        cameraState = (cameraState == CameraState.Hip) ?CameraState.ADS:CameraState.Hip;

        switch (cameraState)
        {
            case CameraState.Hip:
                Hip.enabled = true;
                ADS.enabled = false;
                break;
            case CameraState.ADS:
                Hip.enabled = false;
                ADS.enabled = true;
                break;
        }
    }

    //カメラの振り向き
    private void CameraRotate()
    {
        if (isShopMode) return;
#if UNITY_ANDROID
        float x = Input.GetAxis("HorizontalRightStick");
        //Debug.Log(x);
        float y = Input.GetAxis("VerticalRightStick");
        //Debug.Log(y);
        verticalRotate.transform.Rotate(0, x * senci, 0);
        horizontaoRotate.transform.Rotate(-y * senci, 0, 0);
#endif
#if UNITY_EDITOR
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");

        verticalRotate.transform.Rotate(0, x, 0);
        horizontaoRotate.transform.Rotate(-y, 0, 0);
#endif
    }
    
    //基本的な操作をこの関数にまとめた
    protected void ControlBasely()
    {
        //スキルの制御
        SkillController(championID);
        //UIの制御
        UIController();
        //カメラの振り向き制御
        CameraRotate();
        //移動
        Move();

        if(Input.GetKeyDown(KeyCode.C) || Input.GetButtonDown("Square"))
        {
            isShopMode = (isShopMode) ? false : true;
        }

        //アイテムの制御について
        //アイテムごとにIDを決める
        //そのIDごとにクールダウン時間を決める
        //どのアイテムを所持しているかをhavingItemID[]で管理
        //今のところ、所持可能なアイテム数の上限は４つ
        //havingItemIDに格納されたID(初期値は全部０)を購入時に代入するだけで、制御できる
        for (int index = 0; index < 4; index++)
        {
            if (havingItemID[index] == 0) item[index].gameObject.SetActive(false);
            if (havingItemID[index] != 0) ItemControl(havingItemID[index]);
        }

#if UNITY_ANDROID
        if (isShopMode) return;
        if (!r2Downed && !r2Active && (Input.GetAxis("R2")) < -0.5f)
        {
            r2Downed = true;
            r2Active = true;
        }

        if(r2Downed && r2Active)
        {
            r2Active = false;
            isShooted = true;
            if (!isShooting) StartCoroutine(Shot(fireRate));
        }

        if((Input.GetAxis("R2")) > 0.5f)
        {
            isShooted = false;
            r2Downed = false;
        }

        if(Input.GetButtonDown("Triangle"))
        {
            //スキルモードとアイテムモードの切り替え
            skillMode = (skillMode) ? false : true;
            if (skillMode)
            {
                ImageSkillParent.transform.localPosition = usePosition;
                ImageSkillParent.transform.localScale = useScale;
                ImageItemParent.transform.localPosition = standbyPosition;
                ImageItemParent.transform.localScale = standbyScale;
            }
            else
            {
                ImageSkillParent.transform.localPosition = standbyPosition;
                ImageSkillParent.transform.localScale = standbyScale;
                ImageItemParent.transform.localPosition = usePosition;
                ImageItemParent.transform.localScale = useScale;
            }
        }

        //スキル１　十字キー上
        if(Input.GetAxis("PS4TenVertical") < -0.8f)
        {
            if (skillMode) UseSkill(1);
            else UseItem(1);
        }

        //スキル２　十字キー右
        if(Input.GetAxis("PS4TenHorizontal") < -0.8f)
        {
            if (skillMode) UseSkill(2);
            else UseItem(2);
        }

        //スキル３　十字キー下
        if (Input.GetAxis("PS4TenVertical") > 0.8f)
        {
            if (skillMode) UseSkill(3);
            else UseItem(3);
        }

        //スキル４　十字キー左
        if (Input.GetAxis("PS4TenHorizontal") > 0.8f)
        {
            if (skillMode) UseSkill(4);
            else UseItem(4);
        }
#endif

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.E))
        {
            //スキルモードとアイテムモードの切り替え
            skillMode = (skillMode) ? false : true;
            if (skillMode)
            {
                ImageSkillParent.transform.localPosition = usePosition;
                ImageSkillParent.transform.localScale = useScale;
                ImageItemParent.transform.localPosition = standbyPosition;
                ImageItemParent.transform.localScale = standbyScale;
            }
            else
            {
                ImageSkillParent.transform.localPosition = standbyPosition;
                ImageSkillParent.transform.localScale = standbyScale;
                ImageItemParent.transform.localPosition = usePosition;
                ImageItemParent.transform.localScale = useScale;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (skillMode) UseSkill(1);
            else UseItem(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (skillMode) UseSkill(2);
            else UseItem(2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (skillMode) UseSkill(3);
            else UseItem(3);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (skillMode) UseSkill(4);
            else UseItem(4);
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            isShooted = true;
            if(!isShooting)StartCoroutine(Shot(fireRate));
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            isShooted = false;
            //isShooted = true;
            //firstBullet = false;
            //time = 0;
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            //CameraControl();
        }

        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    selectStateCount++;
        //    switch (selectStateCount % 3)
        //    {
        //        case 0:
        //            EventSystem.current.SetSelectedGameObject(null);
        //            break;
        //        case 1:
        //            skill[0].Select();
        //            break;
        //        case 2:
        //            item[0].Select();
        //            break;
        //    }
        //}

        //Qキーを押したら、敵の情報を取得する予定だが、現状は使わない方向で行く予定
        if (Input.GetKeyDown(KeyCode.Q))
        {
            return;
            Ray ray = Hip.ScreenPointToRay(cameraCenter);
            RaycastHit hitObject;

            //Debug.DrawRay(ray.origin, ray.direction, Color.red, 1f, false);
            if (Physics.Raycast(ray, out hitObject, 100.0f, layerMask))
            {
                //当たった敵のオブジェクトから、敵の装備情報を抽出し、こちらのリストに代入
                //equipmentListEnemy =
                //    hitObject.collider.gameObject.GetComponent<CharaBody>().firstPersonCharacter.GetComponent<Chara>().equipmentList;

                for (int num = 0; num < equipmentListEnemy.Count; num++)
                {
                    equipmentExplainer[num].text = equipmentRefferrence[num];
                }
                explainerBoard.SetActive(true);
            }
            else explainerBoard.SetActive(false);
        }
#endif


    }

    private IEnumerator Shot(float fireRate)
    {
        while (true)
        {
            isShooting = true;

            photonView.RPC ( "BulletsSynchronization", PhotonTargets.All, muzzle.transform.position, muzzle.transform.rotation, (int)playerLocalVariables.team, PhotonNetwork.player.ID);

            yield return new WaitForSeconds(fireRate);

            isShooting = false;

            if (!isShooted) yield break;
        }
    }

    [PunRPC]
    public void BulletsSynchronization( Vector3 muzzlePos, Quaternion direction, int bulletsTeam, int playerID)
    {
        GameObject bulletPrefab = (GameObject)Resources.Load("SphereBullet");
        GameObject bullets = Instantiate( bulletPrefab, muzzlePos, direction);

        BulletsController bc = bullets.GetComponent<BulletsController>();
        bc.teamColor = (TeamColor)bulletsTeam;
        force = 5000;
        bullets.GetComponent<Rigidbody>().AddForce(bullets.transform.forward * force);
        bc.originPlayer = this.gameObject;
        //bc.originChara = originChara;
        bc.ownerID = photonView.ownerId;
        bc.playerID = playerID;
        bc.Initialize( PhotonNetwork.player.ID == playerID);
    }

    ////攻撃関数
    //public void Shoot()
    //{
    //    if (firstBullet)//初弾が撃たれていたら
    //    {
    //        time += Time.deltaTime;
    //        if (time >= fireRate)
    //        {
    //            /*銃弾を生成して飛ばす*/
    //            GameObject bullets = (GameObject)PhotonNetwork.Instantiate("SphereBullet", muzzle.transform.position, muzzle.transform.rotation , 0);
    //            BulletControl bc = bullets.GetComponent<BulletControl>();

    //            bc.teamColor = teamColorPlayer;
    //            force = 5000;
    //            bullets.GetComponent<Rigidbody>().AddForce(bullets.transform.forward * force);
    //            bc.player = gameObject;
    //            //bc.direct = bc.transform.forward;// * Vector3.forward;

    //            /*計測時間を０に戻す*/
    //            time = 0;
    //        }
    //    }
    //    else//撃たれていなければ
    //    {
    //        GameObject bullets = (GameObject)PhotonNetwork.Instantiate("SphereBullet", muzzle.transform.position, muzzle.transform.rotation ,0);
    //        BulletControl bc = bullets.GetComponent<BulletControl>();
    //        bc.teamColor = teamColorPlayer;
    //        bc.player = gameObject;
    //        //bullets.GetComponent<BulletControl>().teamColor = teamColorPlayer;
    //        force = 5000;
    //        bullets.GetComponent<Rigidbody>().AddForce(bullets.transform.forward * force);
    //        //bullets.GetComponent<BulletControl>().player = gameObject;
    //        //bc.direct = bc.transform.forward;// * Vector3.forward;
    //        firstBullet = true;
    //    }

    //}

    //スキル使用関数
    public void UseSkill(int skillNum)//ボタンに張り付けて、スキルを実際に使用するための関数
    {
        switch (skillNum)/*番号に応じたスキルを使う　配列の通し番号と１つずつずれてるので注意！*/
        {
            case 1:
                if (skillIsActive[championID * 4 + 0] && manaToUseSkill[championID * 4 + 0] <= playerLocalVariables.Mana)
                {
                    skillTime[championID * 4 + 0] = 0;
                    FlameBullet(flameBullet);
                }
                break;
            case 2:
                if (skillIsActive[championID * 4 + 1] && manaToUseSkill[championID * 4 + 1] <= playerLocalVariables.Mana)
                {
                    skillTime[championID * 4 + 1] = 0;
                    HeavyBullet(heavyBullet);
                }
                break;
            case 3:
                if (skillIsActive[championID * 4 + 2] && manaToUseSkill[championID * 4 + 2] <= playerLocalVariables.Mana)
                {
                    skillTime[championID * 4 + 2] = 0;
                    Shot(shot);
                }
                break;
            case 4:
                if (skillIsActive[championID * 4 + 3] && manaToUseSkill[championID * 4 + 3] <= playerLocalVariables.Mana)
                {
                    skillTime[championID * 4 + 3] = 0;
                    Grenade(grenade);
                }
                break;
            
            default:
                break;
        }
        skillUseCount[skillNum - 1]++;
        if (skillUseCount[skillNum - 1] == skillUseCountToLevelUp[skillNum - 1])
        {
            skillUseCount[skillNum - 1] = 0;
            skillLevel[skillNum - 1]++;
        }
    }

    /// <summary>
    /// ボタンに張り付けて、アイテムを使用する
    /// </summary>
    /// <param name="index">アイテム欄のインデックス</param>
    public void UseItem(int index)
    {
        if (itemIsActive[index])
        {
            //カテゴリーで分ける
            switch(havingItemID[index - 1])
            {
                case 1://30秒間毎秒１体力回復
                    StartCoroutine(Cure(playerLocalVariables.Hp, 1, 1, 30));
                    break;

                case 2://30秒間毎秒１マナ回復
                    StartCoroutine(Cure(playerLocalVariables.Mana, 1, 1, 30));
                    break;

                case 3://10秒間移動速度を1.5倍
                    StartCoroutine(ChangeParamater(playerLocalVariables.MoveSpeed, 1.5f, 10));
                    break;

                case 4://5秒間攻撃速度を1.5倍
                    StartCoroutine(ChangeParamater(playerLocalVariables.AttackSpeed, 1.5f, 5));
                    break;

                default:
                    break;
            }

            

            itemTime[index] = 0;
            //アイテムの個数を更新
            shopManager.shopItem[havingItemID[index]].itemBought--;
            //残りの個数が０になったら、Chara内のアイテム情報を初期化
            if(shopManager.shopItem[havingItemID[index]].itemBought == 0)
            {
                havingItemID[index] = 0;
                coolDownItem[index] = 0;
                itemIsActive[index] = false;
                item[index - 1].GetComponent<Image>().enabled = false;
            }
        }
    }


    /// <summary>
    /// 体力やマナを回復させる
    /// </summary>
    /// <param name="paramater">回復させるパソコンパラメータ</param>
    /// <param name="curePoint">回復させる度合</param>
    /// <param name="interval">回復する間隔</param>
    /// <param name="cureTime">回復する時間</param>
    private IEnumerator Cure(int paramater , int curePoint , float interval , float cureTime)
    {
        int count = (int)(cureTime / interval);
        while (true)
        {
            paramater += curePoint;
            yield return new WaitForSeconds(interval);
            count--;
            if (count == 0) yield break;
        }
    }

    /// <summary>
    /// 一定時間パラメータを変える
    /// </summary>
    /// <param name="paramater">変化させたいパラメータ</param>
    /// <param name="extension">変化させたい割合</param>
    /// <param name="span">変化させる時間</param>
    /// <returns></returns>
    private IEnumerator ChangeParamater(float paramater , float extension , float span)
    {
        paramater = (int)(paramater * extension);
        yield return new WaitForSeconds(span);
        paramater = (int)(paramater / extension);
    }

    //スキルのクールダウンを管理する関数
    void SkillController(int champID)
    {
        
        if(skillTime[4 * champID + 0] >= coolDownSkill[4 * champID + 0])//スキル１
        {
            skillIsActive[4 * champID + 0] = true;
            
        }
        else
        {
            skillIsActive[4 * champID + 0] = false;
            skillTime[4 * champID + 0] += Time.deltaTime;
        }

        if (skillTime[4 * champID + 1] >= coolDownSkill[4 * champID + 1])//スキル２
        {
            skillIsActive[4 * champID + 1] = true;
        }
        else
        {
            skillIsActive[4 * champID + 1] = false;
            skillTime[4 * champID + 1] += Time.deltaTime;
        }

        if (skillTime[4 * champID + 2] >= coolDownSkill[4 * champID + 2])//スキル３
        {
            skillIsActive[4 * champID + 2] = true;
        }
        else
        {
            skillIsActive[4 * champID + 2] = false;
            skillTime[4 * champID + 2] += Time.deltaTime;
        }

        if (skillTime[4 * champID + 3] >= coolDownSkill[4 * champID + 3])//スキル４
        {
            skillIsActive[4 * champID + 3] = true;
        }
        else
        {
            skillIsActive[4 * champID + 3] = false;
            skillTime[4 * champID + 3] += Time.deltaTime;
        }
    }

    //アイテムのクールダウンを管理する関数
    void ItemControl(int itemID)
    {
        //Debug.Log(itemID);
        if (itemTime[itemID] >= coolDownItem[itemID]) itemIsActive[itemID] = true;
        else
        {
            itemIsActive[itemID] = false;
            itemTime[itemID] += Time.deltaTime;
        }
    }
    
    //UIのパラメータ関連の管理関数
    void UIController()
    {
        //体力バーの表示管理
        hpBar.maxValue = playerLocalVariables.MaxHp;
        hpBar.value = playerLocalVariables.Hp;
        //頭上
        //hpBar_OnHead.maxValue = playerLocalVariables.MaxHp;
        //hpBar_OnHead.value = playerLocalVariables.Hp;


        //マナバーの表示管理
        manaBar.maxValue = playerLocalVariables.ManaMax;
        manaBar.value = playerLocalVariables.Mana;

        //スキルボタンの表示管理
        skill[0].GetComponent<Image>().fillAmount = skillTime[0] / coolDownSkill[0];
        skill[1].GetComponent<Image>().fillAmount = skillTime[1] / coolDownSkill[1];
        skill[2].GetComponent<Image>().fillAmount = skillTime[2] / coolDownSkill[2];
        skill[3].GetComponent<Image>().fillAmount = skillTime[3] / coolDownSkill[3];

        //スキルImageの表示管理
        for (int index = 0; index < skills.Length; index++)
        {
            skills[index].fillAmount = skillTime[index] / coolDownSkill[index];
        }


        //アイテムボタンの表示管理
        for(int index = 0; index < item.Length; index++)
        {
            if(item[index].GetComponent<Image>().enabled == true)
                item[index].GetComponent<Image>().fillAmount = itemTime[havingItemID[index]] / coolDownItem[havingItemID[index]];
        }

        //item[0].GetComponent<Image>().fillAmount = itemTime[havingItemID[0]] / coolDownItem[havingItemID[0]];
        //item[1].GetComponent<Image>().fillAmount = itemTime[havingItemID[1]] / coolDownItem[havingItemID[1]];
        //item[2].GetComponent<Image>().fillAmount = itemTime[havingItemID[2]] / coolDownItem[havingItemID[2]];
        //item[3].GetComponent<Image>().fillAmount = itemTime[havingItemID[3]] / coolDownItem[havingItemID[3]];

        //アイテムImageの表示管理
        //for (int index = 0; index < items.Length; index++)
        //{
        //    items[index].fillAmount = itemTime[havingItemID[index]] / coolDownItem[havingItemID[index]];
        //}

    }
/*
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            //データの送信
            stream.SendNext(playerLocalVariables.Hp);
        }
        else
        {
            if (photonView.isMine) return;
            //データの受信
            this.playerLocalVariables.Hp = (int)stream.ReceiveNext();
        }
    }
*/

    //#region AnnounceMethod
    ///// <summary>
    ///// アナウンスを開始する関数
    ///// </summary>
    ///// <param name="str">追加するテキスト</param>
    //public void StartAnnounce(string str)
    //{
    //    announceTask.Enqueue(str);
    //    if (!isRunning)
    //    {
    //        StartCoroutine(Announce());
    //    }
    //}

    ///// <summary>
    ///// アナウンスを表示する関数
    ///// </summary>
    ///// <returns>time変数の長さ毎に表示を更新</returns>
    //private IEnumerator Announce()
    //{
    //    isRunning = true;

    //    while (announceTask.Count > 0)
    //    {
    //        string str = announceTask.Dequeue();
    //        text1.text = str;
    //        //Debug.Log( "Announce : " + str );

    //        if (text2 != null)
    //        {
    //            text2.text = str;
    //        }

    //        yield return new WaitForSeconds(timeTextVisible);
    //        if (announceTask.Count <= 0)
    //        {
    //            HideText();
    //            isRunning = false;
    //            yield break;
    //        }
    //    }
    //}

    ///// <summary>
    ///// アナウンスを非表示にする関数
    ///// </summary>
    //private void HideText()
    //{
    //    text1.text = null;
    //    if (text2 != null)
    //    {
    //        text2.text = null;
    //    }
    //}

    /////<summary>
    /////ここからアナウンスの内容
    ///// </summary>

    //[PunRPC]//破壊されたタワーがどちらのチームに属するかでメッセージが変わる
    //public void CallIfTowerDestroyed(int teamColor)
    //{
    //    string message = ((TeamColor)teamColor != teamColorPlayer) ? "味方のタワーが破壊されました" : "敵のタワーを破壊しました";
    //    StartAnnounce(message);
    //}

    //[PunRPC]//キルされたプレイヤーがどちらのチームに属するかでメッセージが変わる
    //public void CallIfPlayerKilled(int teamColorOfPlayer)
    //{
    //    string message = ((TeamColor)teamColorOfPlayer == teamColorPlayer) ? "味方が倒されました" : "敵を倒しました";
    //    StartAnnounce(message);
    //}

    //public void CallIfPlayerLevelUpped()
    //{
    //    string message = "レベルが上がりました";
    //    StartAnnounce(message);
    //}
    //#endregion
}
