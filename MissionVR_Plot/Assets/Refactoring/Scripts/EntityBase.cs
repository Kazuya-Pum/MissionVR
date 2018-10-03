using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType { PHYSICAL, MAGIC, THROUGH }

public enum Team { WHITE, BLACK }

public enum EntityType { CHANPION, MINION, TOWER, PROJECTOR, BULLET }

public class EntityBase : Photon.MonoBehaviour
{
    #region variable
    public Team team;
    public EntityType entityType;
    [SerializeField] protected int maxHp;
    private int hp;
    [SerializeField] protected int maxMana;
    private int mana;
    [SerializeField] protected int physicalAttack;
    [SerializeField] protected int physicalDefense;
    [SerializeField] protected int magicAttack;
    [SerializeField] protected int magicDifense;
    [SerializeField] protected int sendingExp;
    [SerializeField] protected int sendingMoney;

    protected Transform tfCache;

    /// <summary>
    /// 視点を取得するゲームオブジェクト
    /// </summary>
    [SerializeField] protected Transform head;
    /// <summary>
    /// 銃撃時の銃弾の生成位置
    /// </summary>
    [SerializeField] protected Transform muzzle;

    protected int Hp
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
        }
    }

    protected int Mana
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
        Hp = maxHp;
        Mana = maxMana;

        #region 値チェック
        physicalAttack = ( physicalAttack <= 0 ) ? 1 : physicalAttack;
        physicalDefense = ( physicalDefense <= 0 ) ? 1 : physicalDefense;
        magicAttack = ( magicAttack <= 0 ) ? 1 : magicAttack;
        magicDifense = ( magicDifense <= 0 ) ? 1 : magicDifense;
        #endregion

        tfCache = transform;
    }

    protected virtual void Start()
    {

    }

    [PunRPC]
    protected virtual void Attack( int damageValue, EntityBase target, DamageType damageType = DamageType.PHYSICAL, int id = 0 )
    {
        if ( id == 0 )
        {
            id = photonView.viewID;
        }

        if ( target.team != team )
        {
            target.photonView.RPC( "Damaged", PhotonTargets.MasterClient, damageValue, damageType, id );
        }
    }

    [PunRPC]
    public void Damaged( int value, DamageType damageType, int id )
    {
        switch ( damageType )
        {
            case DamageType.PHYSICAL:
                value *= value / physicalDefense;
                break;
            case DamageType.MAGIC:
                value *= value / magicDifense;
                break;
            case DamageType.THROUGH:
                break;
        }

        Hp -= value;

        if ( CheckDeath() )
        {
            EntityBase killer = PhotonView.Find( id ).GetComponent<EntityBase>();
            if ( killer.entityType == EntityType.CHANPION )
            {
                killer.photonView.RPC( "GetReward", PhotonTargets.MasterClient, sendingExp, sendingMoney );
            }
            photonView.RPC( "Death", PhotonTargets.MasterClient );
        }
    }

    private bool CheckDeath()
    {
        return ( Hp <= 0 ) ? true : false;
    }

    [PunRPC]
    protected virtual void Death()
    {
        PhotonNetwork.Destroy( gameObject );
    }
}

[System.Serializable]
public class GunInfo
{
    public string name;
    public GameObject bullet;
    public float fireRate;
    public float range;
    //public GameObject model;
}