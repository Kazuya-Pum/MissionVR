using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : MobBase, IPunObservable
{
    [SerializeField] private int myExp;
    [SerializeField] private int myMoney;
    [SerializeField] private int level;
    [SerializeField] private float autoRecoverSpam;

    [SerializeField] private GrowthValues growthValues;

    protected Collider playerCollider;

    public float localSensitivity;

    protected override void Awake()
    {
        base.Awake();

        autoRecoverSpam = ( autoRecoverSpam <= 0 ) ? 1f : autoRecoverSpam;
        level = 1;

        entityType = EntityType.CHANPION;

        if ( photonView.isMine )
        {
            // TODO プレイヤーのプレハブに非アクティブで配置しておくより、シーンに一つあらかじめ置いておいてPlayerControllerから子オブジェクトに移動させた方がよさげ
            head.Find( "Main Camera" ).gameObject.SetActive( true );
        }

        playerCollider = GetComponent<Collider>();
    }

    protected override void Update()
    {
        base.Update();

        if ( PhotonNetwork.isMasterClient )
        {
            AutoRecover();
        }
    }

    [PunRPC]
    public void FetchSetting( float sensitivity )
    {
        localSensitivity = sensitivity;
    }

    [PunRPC]
    protected override void Rotate( float x = 0, float y = 0 )
    {
        base.Rotate( x * localSensitivity, y * localSensitivity );
    }

    /// <summary>
    /// 報酬を受け取る関数。
    /// エンティティーをキルした際に呼ばれる
    /// <para>prev:All->All->next:All</para>
    /// </summary>
    /// <param name="exp">獲得経験値</param>
    /// <param name="money">獲得金額</param>
    public void GetReward( int exp = 0, int money = 0 )
    {
        myExp += exp;
        myMoney += money;

        if ( photonView.isMine )
        {
            PlayerController.instance.OnGetReward();
        }

        if ( myExp >= level * 100 )
        {
            LevelUp();

            if ( photonView.isMine )
            {
                GameManager.instance.SetAnounce( AnounceType.LEVEL );
            }
        }
    }

    /// <summary>
    /// レベルアップを処理する関数。各種ステータス成長
    /// <para>prev:All->All->next:All</para>
    /// </summary>
    protected void LevelUp()
    {
        myExp -= level * 100;
        level++;

        maxHp += growthValues.hp;
        maxMana += growthValues.mana;
        physicalAttack += growthValues.physicalAttack;
        physicalDefense += growthValues.physicalDefense;
        magicAttack += growthValues.magicAttack;
        magicDefense += growthValues.magicDefense;
        moveSpeed += growthValues.moveSpeed;

        hpBar.fillAmount = (float)Hp / maxHp;

        if ( myExp >= level * 100 )
        {
            LevelUp();
        }

        if ( photonView.isMine )
        {
            PlayerController.instance.OnStatusChanged();
        }
    }

    protected override void Death()
    {
        photonView.RPC( "ToDeathState", PhotonTargets.All );

        StartCoroutine( "Respawn" );
    }

    /// <summary>
    /// 全クライアント上でこのプレイヤーを死亡状態に遷移する関数
    /// <para>prev:Master->All</para>
    /// </summary>
    [PunRPC]
    protected override void ToDeathState()
    {
        GameManager.instance.SetAnounce( AnounceType.PlAYER_DEATH, team );
        entityState = EntityState.DEATH;
        playerCollider.enabled = false;
        trigger = false;

        if ( photonView.isMine )
        {
            PlayerController.instance.OnCheck();
        }
    }

    /// <summary>
    /// 一定時間でリスポーン処理をマスターから全クライアントに対して行う関数
    /// <para>prev:Master->Master->next:All</para>
    /// </summary>
    /// <returns></returns>
    private IEnumerator Respawn()
    {
        yield return GameManager.instance.respawnTime;

        photonView.RPC( "ToAliveState", PhotonTargets.AllViaServer );
    }

    /// <summary>
    /// 全クライアント上でこのプレイヤーを生存状態に遷移する関数
    /// <para>prev:Master->All</para>
    /// </summary>
    [PunRPC]
    protected void ToAliveState()
    {
        Hp = maxHp;

        Transform spawnPoint = GameManager.instance.GetSpawnPoint( team );

        tfCache.position = spawnPoint.position;
        tfCache.localRotation = spawnPoint.localRotation;
        head.localEulerAngles = Vector3.zero;
        modelRotate.localRotation = head.localRotation;

        entityState = EntityState.ALIVE;
        playerCollider.enabled = true;

        if ( photonView.isMine )
        {
            PlayerController.instance.OnCheck();
        }
    }

    /// <summary>
    /// HP、Manaの回復
    /// <para>prev:Master->All</para>
    /// </summary>
    /// <param name="cHp">HPの回復値</param>
    /// <param name="cMana">Manaの回復値（省略可）</param>
    [PunRPC]
    protected void Recover( int cHp, int cMana = 0 )
    {
        Hp += cHp;
        Mana += cMana;
    }

    float autoRecoverTime;
    /// <summary>
    /// 一定時間毎に自動回復を行う関数
    /// <para>prev:Master->Master->next:Master</para>
    /// </summary>
    protected void AutoRecover()
    {
        autoRecoverTime += Time.deltaTime;
        if ( autoRecoverTime >= autoRecoverSpam && entityState != EntityState.DEATH )
        {
            photonView.RPC( "Recover", PhotonTargets.All, 1, 1 );
            autoRecoverTime -= autoRecoverSpam;
        }
    }

    /// <summary>
    /// マスタークライアントで処理した各プレイヤーの変数をそれぞれローカルに反映させる。
    /// <para>prev:Master->Master以外</para>
    /// <para>更新処理部分で必要な変数のみ更新するようにするべき？</para>
    /// </summary>
    /// <param name="maxHp"></param>
    /// <param name="hp"></param>
    /// <param name="maxMana"></param>
    /// <param name="mana"></param>
    /// <param name="myExp"></param>
    /// <param name="myMoney"></param>
    /// <param name="level"></param>
    [PunRPC]
    protected void FetchLocalParams( int maxHp, int hp, int maxMana, int mana, int myExp, int myMoney, int level, float moveSpeed )
    {
        this.maxHp = maxHp;
        Hp = hp;
        this.maxMana = maxMana;
        Mana = mana;
        this.myExp = myExp;
        this.myMoney = myMoney;
        this.level = level;
        this.moveSpeed = moveSpeed;

        if ( photonView.isMine )
        {
            PlayerController.instance.OnStatusChanged();
        }
    }

    public int MaxHp
    {
        get
        {
            return maxHp;
        }
    }

    public override int Hp
    {
        get
        {
            return base.Hp;
        }

        set
        {
            base.Hp = value;
        }
    }

    public int MaxMana
    {
        get
        {
            return maxMana;
        }
    }

    public override int Mana
    {
        get
        {
            return base.Mana;
        }

        set
        {
            base.Mana = value;
        }
    }

    public int PhysicalAttack
    {
        get
        {
            return physicalAttack;
        }
    }

    public int PhysicalDefense
    {
        get
        {
            return physicalDefense;
        }
    }

    public int MagicAttack
    {
        get
        {
            return magicAttack;
        }
    }

    public int MagicDefense
    {
        get
        {
            return magicDefense;
        }
    }

    public float MoveSpeed
    {
        get
        {
            return moveSpeed;
        }
    }

    public int Level
    {
        get
        {
            return level;
        }
    }

    public int MyMoney
    {
        get
        {
            return myMoney;
        }
    }

    public int MyExp
    {
        get
        {
            return myExp;
        }
    }

    public override void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
    {
        base.OnPhotonSerializeView( stream, info );

        if ( stream.isWriting )
        {
            stream.SendNext( localSensitivity );
        }
        else
        {
            localSensitivity = (float)stream.ReceiveNext();
        }
    }
}

// TODO スクリプタブルオブジェクトにする
/// <summary>
/// レベルアップ時のステータス上昇値
/// </summary>
[System.Serializable]
public class GrowthValues
{
    public int hp;
    public int mana;
    public int physicalAttack;
    public int physicalDefense;
    public int magicAttack;
    public int magicDefense;
    public float moveSpeed;
}