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
    }

    [PunRPC]
    protected virtual void Attack( int damageValue, EntityBase target, DamageType damageType = DamageType.PHYSICAL, int id = 0 )
    {
        if(id == 0)
        {
            id = photonView.viewID;
        }

        if ( target.team != team )
        {
            target.photonView.RPC( "Damaged", PhotonTargets.MasterClient, damageValue, damageType, id);
        }
    }

    [PunRPC]
    public void Damaged( int value, DamageType damageType, int id)
    {
        switch ( damageType )
        {
            case DamageType.PHYSICAL:

                break;
            case DamageType.MAGIC:
                break;
            case DamageType.THROUGH:
                break;
        }

        hp -= value;

        if ( CheckDeath() )
        {
            if(PhotonView.Find(id).gameObject.GetComponent<EntityBase>().objectType == ObjectType.Champion )
            {

            }
            photonView.RPC( "Death", PhotonTargets.All );
        }
    }

    [PunRPC]
    protected void Death()
    {
        PhotonNetwork.Destroy( gameObject );
    }

    private bool CheckDeath()
    {
        return ( hp <= 0 ) ? true : false;
    }
}
