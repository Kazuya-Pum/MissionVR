﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DamageType { PHYSICAL, MAGIC, THROUGH }

public enum Team { WHITE, BLACK }

public enum EntityType { CHANPION, MINION, TOWER, PROJECTOR, BULLET }

public class EntityBase : Photon.MonoBehaviour, IPunObservable
{
    #region variable
    public Team team;
    public EntityType entityType;

    // クラス作ってもいいかも
    #region Status
    [SerializeField] protected int maxHp;
    [SerializeField] private int hp;
    [SerializeField] protected int maxMana;
    [SerializeField] private int mana;
    [SerializeField] protected int physicalAttack;
    [SerializeField] protected int physicalDefense;
    [SerializeField] protected int magicAttack;
    [SerializeField] protected int magicDifense;
    [SerializeField] protected int sendingExp;
    [SerializeField] protected int sendingMoney;
    #endregion

    protected Transform tfCache;

    [SerializeField] protected Slider hpSlider;
    protected Transform tfSliderCache;
    protected Transform playerCamera;

    public int gunIndex;
    protected GunInfo gunInfo;
    protected bool trigger;

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

        gunInfo = TestNetwork.instance.DataBase.gunInfos[gunIndex];

        tfCache = transform;
        tfSliderCache = hpSlider.transform;
    }

    protected virtual void Start()
    {
        playerCamera = PlayerController.instance.player.head.Find( "Main Camera" ).transform;

        SetBarColor( PlayerController.instance.player.team );
    }

    protected virtual void Update()
    {
        tfSliderCache.LookAt( playerCamera );
        hpSlider.maxValue = maxHp;
        hpSlider.value = Hp;

        Shooting( trigger );
    }

    [PunRPC]
    protected void FetchTeam( Team remoteTeam )
    {
        team = remoteTeam;
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

    float interval;
    protected void Shooting( bool trigger )
    {
        if ( interval >= gunInfo.fireRate )
        {
            if ( trigger )
            {
                Shoot();
                interval = 0;
            }
        }
        else
        {
            interval += Time.deltaTime;
        }
    }

    protected void Shoot()
    {
        GameObject bullet = Instantiate( gunInfo.bullet, muzzle.position, head.rotation );

        BulletBase bulletBase = bullet.GetComponent<BulletBase>();

        if ( PhotonNetwork.isMasterClient )
        {
            bulletBase.ownerId = photonView.viewID;
            bulletBase.damageValue = physicalAttack;
            bulletBase.damageType = DamageType.PHYSICAL;
        }
        bulletBase.range = gunInfo.range;
        bulletBase.team = team;
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

        if ( Hp <= 0 )
        {
            EntityBase killer = PhotonView.Find( id ).GetComponent<EntityBase>();
            if ( killer.entityType == EntityType.CHANPION )
            {
                killer.photonView.RPC( "GetReward", PhotonTargets.MasterClient, sendingExp, sendingMoney );
            }
            photonView.RPC( "Death", PhotonTargets.MasterClient );
        }
    }

    [PunRPC]
    protected virtual void Death()
    {
        PhotonNetwork.Destroy( gameObject );
    }

    private void SetBarColor( Team playerTeam )
    {
        if ( playerTeam == team )
        {
            tfSliderCache.Find( "Fill Area/Fill" ).GetComponent<Image>().color = TestNetwork.instance.DataBase.allyColor;
        }
        else
        {
            tfSliderCache.Find( "Fill Area/Fill" ).GetComponent<Image>().color = TestNetwork.instance.DataBase.enemyColor;
        }
    }

    // TODO デリゲートとジェネリック作る
    public virtual void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
    {
        if ( stream.isWriting && PhotonNetwork.isMasterClient )
        {
            stream.SendNext( maxHp );
            stream.SendNext( Hp );
            stream.SendNext( head.rotation );
        }
        if ( stream.isReading && !PhotonNetwork.isMasterClient )
        {
            maxHp = (int)stream.ReceiveNext();
            Hp = (int)stream.ReceiveNext();
            head.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}