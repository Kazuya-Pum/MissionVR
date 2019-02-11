using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public enum DamageType : byte { PHYSICAL, MAGIC, THROUGH }

public enum Team : byte { WHITE, BLACK }

public enum EntityType : byte { CHANPION, MINION, TOWER, PROJECTOR, BULLET }

// TODO 状態異常をここに追加するか別でenum作るかは要検討
public enum EntityState : byte { ALIVE, DEATH }

public class EntityBase : Photon.MonoBehaviour, IPunObservable
{
    #region variable
    public Team team;
    public EntityType entityType;
    public EntityState entityState;

    // クラス作ってもいいかも
    #region Status
    [SerializeField] protected int maxHp;
    private int hp;
    [SerializeField] protected int maxMana;
    private int mana;
    [SerializeField] protected int physicalAttack;
    [SerializeField] protected int physicalDefense;
    [SerializeField] protected int magicAttack;
    [SerializeField] protected int magicDefense;
    [SerializeField] protected int sendingExp;
    [SerializeField] protected int sendingMoney;
    #endregion

    [HideInInspector] public Transform tfCache;

    protected Image hpBar;
    protected Transform tfBarCache;
    protected Transform playerCamera;

    protected Image miniMapPoint;

    public int gunIndex;
    protected GunInfo gunInfo;
    [HideInInspector] public bool trigger;

    /// <summary>
    /// 視点を取得するゲームオブジェクト
    /// </summary>
    [HideInInspector] public Transform head;
    /// <summary>
    /// 銃撃時の銃弾の生成位置
    /// </summary>
    [HideInInspector] public Transform muzzle;

    [HideInInspector] public int index;

    public virtual int Hp
    {
        get
        {
            return hp;
        }

        set
        {
            if ( value >= maxHp )
            {
                hp = maxHp;
            }
            else
            {
                hp = value;
            }

            hpBar.fillAmount = (float)hp / maxHp;
        }
    }

    public virtual int Mana
    {
        get
        {
            return mana;
        }

        set
        {
            if ( value >= maxMana )
            {
                mana = maxMana;
            }
            else
            {
                mana = value;
            }
        }
    }

    #endregion


    protected virtual void Awake()
    {
        tfCache = transform;

        tfBarCache = tfCache.Find( "Visual/Bar" );
        hpBar = tfBarCache.Find( "HP/HP_Bar" ).GetComponent<Image>();
        miniMapPoint = tfCache.Find( "Visual/MiniMap/MiniMapPoint" ).GetComponent<Image>();
        head = tfCache.Find( "Head" );
        muzzle = head.Find( "Muzzle" );

        #region 値チェック
        physicalAttack = ( physicalAttack <= 0 ) ? 1 : physicalAttack;
        physicalDefense = ( physicalDefense <= 0 ) ? 1 : physicalDefense;
        magicAttack = ( magicAttack <= 0 ) ? 1 : magicAttack;
        magicDefense = ( magicDefense <= 0 ) ? 1 : magicDefense;
        #endregion
    }

    protected virtual void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        Hp = maxHp;
        Mana = maxMana;

        entityState = EntityState.ALIVE;

        gunInfo = GameManager.instance.DataBase.gunInfos[gunIndex];

        if ( PlayerController.instance.player )
        {
            OnSetPlayer();
        }
        else
        {
            GameManager.instance.onSetPlayer += OnSetPlayer;
        }
    }

    [PunRPC]
    protected void ToActiveSetting( Team team, Vector3 position, Quaternion rotation )
    {
        gameObject.SetActive( true );
        this.team = team;
        Initialize();
        tfCache.position = position;
        tfCache.rotation = rotation;
    }

    private void OnSetPlayer()
    {
        playerCamera = Camera.main.transform;
        SetBarColor( PlayerController.instance.player.team );
    }

    protected virtual void Update()
    {
        tfBarCache.LookAt( playerCamera );

        if ( photonView.isMine )
        {
            Shooting( trigger );
        }
        else
        {
            UpdateRotation();
        }
    }

    float interval;
    /// <summary>
    /// <para>prev:ローカル->ローカル->next:All</para>
    /// </summary>
    /// <param name="trigger"></param>
    protected void Shooting( bool trigger )
    {
        if ( interval >= gunInfo.fireRate )
        {
            if ( trigger )
            {
                photonView.RPC( "Shoot", PhotonTargets.AllViaServer, head.rotation );
                interval = 0;
            }
        }
        else
        {
            interval += Time.deltaTime;
        }
    }

    [PunRPC]
    protected void Shoot( Quaternion direction )
    {
        BulletBase bulletBase;

        if ( GameManager.instance.bullets.Any() )
        {
            bulletBase = GameManager.instance.bullets.Dequeue();
            bulletBase.transform.position = muzzle.position;
            bulletBase.transform.localRotation = direction;
            bulletBase.gameObject.SetActive( true );
        }
        else
        {
            bulletBase = Instantiate( gunInfo.bullet, muzzle.position, direction ).GetComponent<BulletBase>();
        }

        if ( PhotonNetwork.isMasterClient )
        {
            bulletBase.owner = this;
            bulletBase.damageValue = physicalAttack;
            bulletBase.damageType = DamageType.PHYSICAL;
        }
        bulletBase.range = gunInfo.range;
        bulletBase.team = team;
    }

    /// <summary>
    /// 攻撃を処理する関数
    /// <para>prev:Master->Master->next:Master</para>
    /// </summary>
    /// <param name="damageValue">ダメージ値</param>
    /// <param name="target">攻撃対象</param>
    /// <param name="damageType">攻撃の種類</param>
    public virtual void Attack( int damageValue, EntityBase target, DamageType damageType = DamageType.PHYSICAL )
    {
        if ( target.team != team )
        {
            target.photonView.RPC( "Damaged", PhotonTargets.AllViaServer, (float)damageValue, damageType, photonView.viewID );
        }
    }

    /// <summary>
    /// ダメージを受ける関数
    /// <para>prev:Master->All->All(DeathのみMaster)</para>
    /// </summary>
    /// <param name="value">ダメージ値</param>
    /// <param name="damageType">攻撃の種類</param>
    /// <param name="killer">攻撃をしてきたエンティティー</param>
    [PunRPC]
    public void Damaged( float value, DamageType damageType, int killerId )
    {
        switch ( damageType )
        {
            case DamageType.PHYSICAL:
                value *= value / physicalDefense;
                break;
            case DamageType.MAGIC:
                value *= value / magicDefense;
                break;
            case DamageType.THROUGH:
                break;
        }

        Hp -= Mathf.CeilToInt( value );

        if ( Hp <= 0 )
        {
            EntityBase killer = PhotonView.Find( killerId ).GetComponent<EntityBase>();
            if ( killer.entityType == EntityType.CHANPION )
            {
                killer.GetComponent<PlayerBase>().GetReward( sendingExp, sendingExp );
            }

            if ( PhotonNetwork.isMasterClient )
            {
                Death();
            }
        }
    }

    protected virtual void Death()
    {
        photonView.RPC( "ToDeathState", PhotonTargets.All );
    }

    [PunRPC]
    protected virtual void ToDeathState()
    {
        if ( PhotonNetwork.isMasterClient )
        {
            GameManager.instance.minions[index].Add( this );
        }
        trigger = false;
        gameObject.SetActive( false );
        entityState = EntityState.DEATH;
    }

    private void SetBarColor( Team playerTeam )
    {
        if ( playerTeam == team )
        {
            hpBar.color = GameManager.instance.DataBase.allyColor;
            miniMapPoint.color = GameManager.instance.DataBase.allyColor;
        }
        else
        {
            hpBar.color = GameManager.instance.DataBase.enemyColor;
            miniMapPoint.color = GameManager.instance.DataBase.enemyColor;
        }
    }

    Vector3 rotateX;
    Vector3 rotateY;

    [PunRPC]
    protected virtual void Rotate( float x = 0, float y = 0 )
    {
        rotateX.x = head.localEulerAngles.x - y;
        rotateY.y = tfCache.localEulerAngles.y + x;

        tfCache.localEulerAngles = rotateY;
        head.localEulerAngles = rotateX;  // TODO 角度の上限作成
    }

    [PunRPC]
    public virtual void RotateToTarget( Vector3 to )
    {
        Vector3 diff = Quaternion.LookRotation( to - head.position ).eulerAngles;

        rotateX.x = diff.x;
        rotateY.y = diff.y;

        tfCache.localEulerAngles = rotateY;
        head.localEulerAngles = rotateX;
    }

    Quaternion networkHeadRotation;
    protected virtual void UpdateRotation()
    {
        head.localRotation = Quaternion.RotateTowards( head.localRotation, networkHeadRotation, 180 * Time.deltaTime );
    }

    public virtual void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
    {
        if ( stream.isWriting )
        {
            if ( PhotonNetwork.isMasterClient )
            {
                stream.SendNext( maxHp );
                stream.SendNext( Hp );
            }
            stream.SendNext( team );
            stream.SendNext( head.localRotation );
            networkHeadRotation = head.localRotation;
        }
        else
        {
            if ( !PhotonNetwork.isMasterClient )
            {
                maxHp = (int)stream.ReceiveNext();
                Hp = (int)stream.ReceiveNext();
            }
            team = (Team)stream.ReceiveNext();
            networkHeadRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}