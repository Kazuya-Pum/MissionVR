﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : EntityBase
{
    [SerializeField] private int myExp;
    [SerializeField] private int myMoney;
    private int level;

    [SerializeField] private GrowthValues growthValues;

    // TODO 設定ファイル等に移設
    [SerializeField] private float sensitivity;

    private Transform head;

    protected override void Awake()
    {
        base.Awake();
        sensitivity = ( sensitivity <= 0 ) ? 1 : sensitivity;

        head = tfCache.Find( "Visor" );

        if ( photonView.isMine )
        {
            head.Find( "Main Camera" ).gameObject.SetActive( true );
        }
    }

    protected void Update()
    {
        if ( photonView.isMine )
        {
            GetKey();
        }
    }

    private void GetKey()
    {
        float x = Input.GetAxis( "Horizontal" ) * Time.deltaTime;
        float z = Input.GetAxis( "Vertical" ) * Time.deltaTime;

        if ( x != 0 || z != 0 )
        {
            photonView.RPC( "Move", PhotonTargets.All, x, z );
        }

        float mouse_x = Input.GetAxis( "Mouse X" ) * Time.deltaTime * sensitivity;
        float mouse_y = Input.GetAxis( "Mouse Y" ) * Time.deltaTime * sensitivity;

        if ( mouse_x != 0 || mouse_y != 0 )
        {
            photonView.RPC( "Rotate", PhotonTargets.All, mouse_x, mouse_y );
        }

        if ( Input.GetKeyDown( KeyCode.LeftShift ) )
        {
            photonView.RPC( "SetDashFlag", PhotonTargets.All, true );
        }
        else if ( Input.GetKeyUp( KeyCode.LeftShift ) )
        {
            photonView.RPC( "SetDashFlag", PhotonTargets.All, false );
        }
    }

    [PunRPC]
    protected void Rotate( float x = 0, float y = 0 )
    {
        tfCache.localEulerAngles = new Vector3( 0, tfCache.localEulerAngles.y + x, 0 );
        head.localEulerAngles = new Vector3( head.localEulerAngles.x - y, 0, 0 );   // TODO 角度の上限作成
    }

    [PunRPC]
    protected void SetDashFlag(bool flag)
    {
        dashFlag = flag;
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
