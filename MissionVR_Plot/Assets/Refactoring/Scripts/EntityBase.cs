using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType { PHYSICAL, MAGIC, THROUGH }

public class EntityBase : Photon.MonoBehaviour
{
    #region variable
    public TeamColor team;
    public ObjectType objectType;
    [SerializeField] protected int maxHp;
    protected int hp;
    [SerializeField] protected int maxMana;
    protected int mana;
    [SerializeField] protected int physicalAttack;
    [SerializeField] protected int physicalDefense;
    [SerializeField] protected int magicAttack;
    [SerializeField] protected int magicDifense;
    [SerializeField] protected float attackSpeed;
    [SerializeField] protected int sendingExp;
    [SerializeField] protected int sendingMoney;

    [SerializeField] private float moveSpeed;
    #endregion

    protected virtual void Awake()
    {
        hp = maxHp;
        mana = maxMana;

        physicalAttack = ( physicalAttack <= 0 ) ? 1 : physicalAttack;
        physicalDefense = ( physicalDefense <= 0 ) ? 1 : physicalDefense;
        magicAttack = ( magicAttack <= 0 ) ? 1 : magicAttack;
        magicDifense = ( magicDifense <= 0 ) ? 1 : magicDifense;
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

        hp -= value;

        if ( CheckDeath() )
        {
            EntityBase killer = PhotonView.Find( id ).GetComponent<EntityBase>();
            if ( killer.objectType == ObjectType.Champion )
            {
                killer.photonView.RPC( "GetReward", PhotonTargets.MasterClient, sendingExp, sendingMoney );
            }
            photonView.RPC( "Death", PhotonTargets.All );
        }
    }

    private bool CheckDeath()
    {
        return ( hp <= 0 ) ? true : false;
    }

    [PunRPC]
    protected virtual void Death()
    {
        if ( objectType != ObjectType.Champion )
        {
            Destroy( gameObject );
        }
    }
}
