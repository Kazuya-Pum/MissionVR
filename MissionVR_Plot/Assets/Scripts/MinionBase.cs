using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionBase : Photon.MonoBehaviour
{

    #region value
    [SerializeField] protected int hp;
    [SerializeField] protected int mana;
    [SerializeField] protected int physicalAttack;
    [SerializeField] protected int physicalDefense;
    [SerializeField] protected int magicAttack;
    [SerializeField] protected int magicDifense;
    [SerializeField] protected float attackSpeed;
    [SerializeField] protected int exp;
    [SerializeField] protected int money;

    [SerializeField] private float moveSpeed;
    #endregion

    [PunRPC]
    protected virtual void Attack(int damageValue, MinionBase target)
    {
        target.photonView.RPC( "Damaged", PhotonTargets.MasterClient, damageValue );
    }

    [PunRPC]
    public void Damaged(int value)
    {
        hp -= value;

        if ( CheckDeath() )
        {
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
