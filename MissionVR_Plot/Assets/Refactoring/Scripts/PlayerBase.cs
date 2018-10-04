using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : MobBase
{
    [SerializeField] private int myExp;
    [SerializeField] private int myMoney;
    [SerializeField] private int level;
    [SerializeField] private float autoRecoverSpam;

    [SerializeField] private GrowthValues growthValues;

    private float localSensitivity;

    protected override void Awake()
    {
        base.Awake();

        autoRecoverSpam = ( autoRecoverSpam <= 0 ) ? 1f : autoRecoverSpam;

        entityType = EntityType.CHANPION;

        if ( photonView.isMine )
        {
            head.Find( "Main Camera" ).gameObject.SetActive( true );
        }
    }

    protected override void Update()
    {
        base.Update();

        if ( PhotonNetwork.isMasterClient )
        {
            AutoRecover();
            photonView.RPC( "FetchLocalParams", PhotonTargets.Others, maxHp, Hp, maxMana, Mana, myExp, myMoney, level );
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


    [PunRPC]
    protected void GetReward( int exp = 0, int money = 0 )
    {
        myExp += exp;
        myMoney += money;

        if ( myExp >= level * 100 + 50 )
        {
            photonView.RPC( "LevelUp", PhotonTargets.MasterClient );
        }
    }

    [PunRPC]
    protected void LevelUp()
    {
        myExp -= level * 100;
        level++;

        maxHp += growthValues.hp;
        maxMana += growthValues.mana;
        physicalAttack += growthValues.phycalAttack;
        physicalDefense += growthValues.physicalDefense;
        magicAttack += growthValues.magicAttack;
        magicDifense += growthValues.magicDefense;


        if ( myExp >= level * 100 )
        {
            LevelUp();
        }
    }


    [PunRPC]
    protected override void Death()
    {
        ReSpawn();
    }

    private void ReSpawn()
    {
        Debug.Log( "respawn" );
    }


    // TODO 連打で高速で撃ててしまうため要修正
    [PunRPC]
    protected void PullTheTrigger( bool trigger )
    {
        if ( trigger )
        {
            StartCoroutine( "Shooting" );
        }
        else
        {
            StopCoroutine( "Shooting" );
        }
    }

    /// <summary>
    /// HP、Manaの回復
    /// </summary>
    /// <param name="cHp">HPの回復値</param>
    /// <param name="cMana">Manaの回復値（省略可）</param>
    protected void Recover( int cHp, int cMana = 0 )
    {
        Hp += cHp;
        Mana += cMana;
    }

    float autoRecoverTime;
    protected void AutoRecover()
    {
        autoRecoverTime += Time.deltaTime;
        if ( autoRecoverTime >= autoRecoverSpam )
        {
            Recover( 1, 1 );
            autoRecoverTime -= autoRecoverSpam;
        }
    }

    /// <summary>
    /// マスタークライアントで処理した各プレイヤーの変数をそれぞれローカルに反映させる。
    /// 更新処理部分で必要な変数のみ更新するようにするべき？
    /// </summary>
    /// <param name="maxHp"></param>
    /// <param name="hp"></param>
    /// <param name="maxMana"></param>
    /// <param name="mana"></param>
    /// <param name="myExp"></param>
    /// <param name="myMoney"></param>
    /// <param name="level"></param>
    [PunRPC]
    protected void FetchLocalParams( int maxHp, int hp, int maxMana, int mana, int myExp, int myMoney, int level )
    {
        this.maxHp = maxHp;
        Hp = hp;
        this.maxMana = maxMana;
        Mana = mana;
        this.myExp = myExp;
        this.myMoney = myMoney;
        this.level = level;
    }
}

/// <summary>
/// レベルアップ時のステータス上昇値
/// </summary>
[System.Serializable]
public class GrowthValues
{
    public int hp;
    public int mana;
    public int phycalAttack;
    public int physicalDefense;
    public int magicAttack;
    public int magicDefense;
}
