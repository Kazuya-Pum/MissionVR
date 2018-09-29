using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : EntityBase
{
    [SerializeField] private int myExp;
    [SerializeField] private int myMoney;
    private int level;

    #region growthValue
    [SerializeField] private int growthHp;
    //[SerializeField] private int growthMana;
    [SerializeField] private int growthPhysicalAtk;
    [SerializeField] private int growthPhysicalDfe;
    [SerializeField] private int growthMagicAtk;
    [SerializeField] private int growthMagicDfe;
    #endregion

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

        maxHp += growthHp;
        //maxMana += growthMana;
        physicalAttack += growthPhysicalAtk;
        physicalDefense += growthPhysicalDfe;
        magicAttack += growthMagicAtk;
        magicDifense += growthMagicDfe;


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
}
