using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : MobBase, IPunObservable
{
    [SerializeField] private int myExp;
    [SerializeField] private int myMoney;
    private int level;
    [SerializeField] private float autoRecoverSpam;

    [SerializeField] private GrowthValues growthValues;

    protected override void Awake()
    {
        base.Awake();

        autoRecoverSpam = ( autoRecoverSpam <= 0 ) ? 1f : autoRecoverSpam;

        entityType = EntityType.CHANPION;

        if ( photonView.isMine )
        {
            head.Find( "Main Camera" ).gameObject.SetActive( true );
            GameObject.FindGameObjectWithTag( "Controller" ).GetComponent<PlayerController>().player = this;
        }
    }

    protected void Update()
    {
        if ( PhotonNetwork.isMasterClient )
        {
            AutoRecover();
        }
    }

    [PunRPC]
    protected void GetReward( int exp = 0, int money = 0 )
    {
        myExp += exp;
        myMoney += money;

        if ( myExp >= level * 100 )
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


    // 連打で高速で撃ててしまう
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

    public override void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
    {
        base.OnPhotonSerializeView( stream, info );

        if ( stream.isWriting && PhotonNetwork.isMasterClient )
        {
            stream.SendNext( maxMana );
            stream.SendNext( Mana );
            stream.SendNext( myExp );
            stream.SendNext( myMoney );
        }
        else if ( stream.isReading && !PhotonNetwork.isMasterClient )
        {
            maxMana = (int)stream.ReceiveNext();
            Mana = (int)stream.ReceiveNext();
            myExp = (int)stream.ReceiveNext();
            myMoney = (int)stream.ReceiveNext();
        }
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
