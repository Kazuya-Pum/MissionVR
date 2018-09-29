using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : EntityBase
{
    [SerializeField] private int myExp;
    [SerializeField] private int myMoney;
    private int level;

    [SerializeField] private GrowthValues growthValues;

    private void Update()
    {
        if ( photonView.isMine )
        {
            GetKey();
        }
    }

    private void GetKey()
    {
        float x = Input.GetAxis( "Horizontal" ) * Time.deltaTime * moveSpeed;
        float z = Input.GetAxis( "Vertical" ) * Time.deltaTime * moveSpeed;

        Move( x, z );
    }

    private void Move( float x, float z )
    {
        tfCache.Translate( x, 0, z );
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
        //maxMana += growthValues.mana;
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
