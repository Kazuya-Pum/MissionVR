using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : MobBase
{
    [SerializeField] private int myExp;
    [SerializeField] private int myMoney;
    private int level;

    [SerializeField] private GrowthValues growthValues;

    // TODO 設定ファイル等に移設
    [SerializeField] private float sensitivity;

    // TODO 銃の設定から取得
    [SerializeField] private float gunFireRate;
    private WaitForSeconds fireRate;


    protected override void Awake()
    {
        base.Awake();
        sensitivity = ( sensitivity <= 0 ) ? 1f : sensitivity;
        gunFireRate = ( gunFireRate <= 0 ) ? 0.1f : gunFireRate;

        fireRate = new WaitForSeconds( gunFireRate );
    }

    protected void Update()
    {
        if ( photonView.isMine )
        {
            GetKey();
        }
    }

    // TODO InputManagerからキーを設定し、そちらを使用
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

        if ( Input.GetKeyDown( KeyCode.Mouse0 ) )
        {
            photonView.RPC( "PullTheTrigger", PhotonTargets.AllViaServer, true );
        }
        else if ( Input.GetKeyUp( KeyCode.Mouse0 ) )
        {
            photonView.RPC( "PullTheTrigger", PhotonTargets.AllViaServer, false );
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

    protected IEnumerator Shooting()
    {
        while ( true )
        {
            Debug.Log( "shoot" );
            yield return fireRate;
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
