using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Refactoring
{
    public enum DamageType { PHYSICAL, MAGIC, THROUGH }

    public enum Team { WHITE, BLACK }

    public enum EntityType { CHANPION, MINION, TOWER, PROJECTOR, BULLET }

    public enum EntityState { ALIVE, DEATH }

    public class EntityBase : Photon.MonoBehaviour, IPunObservable
    {
        #region variable
        public Team team;
        public EntityType entityType;
        public EntityState entityState;

        // クラス作ってもいいかも
        #region Status
        [SerializeField] protected int maxHp;
        [SerializeField] private int hp;
        [SerializeField] protected int maxMana;
        [SerializeField] private int mana;
        [SerializeField] protected int physicalAttack;
        [SerializeField] protected int physicalDefense;
        [SerializeField] protected int magicAttack;
        [SerializeField] protected int magicDefense;
        [SerializeField] protected int sendingExp;
        [SerializeField] protected int sendingMoney;
        #endregion

        protected Transform tfCache;

        [SerializeField] protected Slider hpSlider;
        protected Transform tfSliderCache;
        protected Transform playerCamera;

        [SerializeField] private int gunIndex;
        protected GunInfo gunInfo;
        [HideInInspector] public bool trigger;

        /// <summary>
        /// 視点を取得するゲームオブジェクト
        /// </summary>
        [SerializeField] public Transform head;
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

                hpSlider.value = hp;
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
            tfCache = transform;
            tfSliderCache = hpSlider.transform.parent;

            hpSlider.maxValue = maxHp;

            Hp = maxHp;
            Mana = maxMana;

            #region 値チェック
            physicalAttack = ( physicalAttack <= 0 ) ? 1 : physicalAttack;
            physicalDefense = ( physicalDefense <= 0 ) ? 1 : physicalDefense;
            magicAttack = ( magicAttack <= 0 ) ? 1 : magicAttack;
            magicDefense = ( magicDefense <= 0 ) ? 1 : magicDefense;
            #endregion
        }

        protected virtual void Start()
        {
            gunInfo = GameManager.instance.DataBase.gunInfos[gunIndex];

            if ( PlayerController.instance.player )
            {
                OnSetPlayer();
            }
        }

        void OnJoinedRoom()
        {
            GameManager.instance.onSetPlayer += OnSetPlayer;
        }

        private void OnSetPlayer()
        {
            playerCamera = PlayerController.instance.playerCamera;
            SetBarColor( PlayerController.instance.player.team );
        }

        protected virtual void Update()
        {
            tfSliderCache.LookAt( playerCamera );

            Shooting( trigger );

            if ( !photonView.isMine )
            {
                UpdateRotation();
            }
        }

        [PunRPC]
        protected void FetchTeam( Team remoteTeam )
        {
            team = remoteTeam;
        }

        /// <summary>
        /// 攻撃を処理する関数
        /// <para>prev:Master->Master->next:Master</para>
        /// </summary>
        /// <param name="damageValue">ダメージ値</param>
        /// <param name="target">攻撃対象</param>
        /// <param name="damageType">攻撃の種類</param>
        public virtual void Attack( int damageValue, EntityBase target, DamageType damageType = DamageType.PHYSICAL )
        {
            if ( target.team != team )
            {
                target.Damaged( damageValue, damageType, this );
            }
        }

        float interval;
        /// <summary>
        /// <para>prev:ローカル->ローカル->next:All</para>
        /// </summary>
        /// <param name="trigger"></param>
        protected void Shooting( bool trigger )
        {
            if ( interval >= gunInfo.fireRate )
            {
                if ( trigger )
                {
                    photonView.RPC( "Shoot", PhotonTargets.AllViaServer, head.rotation );
                    interval = 0;
                }
            }
            else
            {
                interval += Time.deltaTime;
            }
        }

        // TODO オブジェクトプール
        [PunRPC]
        protected void Shoot( Quaternion direction )
        {
            BulletBase bulletBase;

            if ( GameManager.instance.bullets.Count > 0 )
            {
                bulletBase = GameManager.instance.bullets.Dequeue();
                bulletBase.transform.position = muzzle.position;
                bulletBase.transform.localRotation = direction;
                bulletBase.gameObject.SetActive( true );
            }
            else
            {
                bulletBase = Instantiate( gunInfo.bullet, muzzle.position, direction ).GetComponent<BulletBase>();
            }

            if ( PhotonNetwork.isMasterClient )
            {
                bulletBase.owner = this;
                bulletBase.damageValue = physicalAttack;
                bulletBase.damageType = DamageType.PHYSICAL;
            }
            bulletBase.range = gunInfo.range;
            bulletBase.team = team;
        }

        /// <summary>
        /// ダメージを受ける関数
        /// <para>prev:Master->Master->Master</para>
        /// </summary>
        /// <param name="value">ダメージ値</param>
        /// <param name="damageType">攻撃の種類</param>
        /// <param name="killer">攻撃をしてきたエンティティー</param>
        public void Damaged( int value, DamageType damageType, EntityBase killer )
        {
            switch ( damageType )
            {
                case DamageType.PHYSICAL:
                    value *= value / physicalDefense;
                    break;
                case DamageType.MAGIC:
                    value *= value / magicDefense;
                    break;
                case DamageType.THROUGH:
                    break;
            }

            Hp -= value;

            if ( Hp <= 0 )
            {
                if ( killer.entityType == EntityType.CHANPION )
                {
                    killer.GetComponent<PlayerBase>().GetReward( sendingExp, sendingMoney );
                }
                Death();
            }
        }

        protected virtual void Death()
        {
            PhotonNetwork.Destroy( gameObject );
        }

        private void SetBarColor( Team playerTeam )
        {
            if ( playerTeam == team )
            {
                tfSliderCache.Find( "HP_Slider/Fill Area/Fill" ).GetComponent<Image>().color = GameManager.instance.DataBase.allyColor;
            }
            else
            {
                tfSliderCache.Find( "HP_Slider/Fill Area/Fill" ).GetComponent<Image>().color = GameManager.instance.DataBase.enemyColor;
            }
        }

        Quaternion networkHeadRotation;
        protected virtual void UpdateRotation()
        {
            head.localRotation = Quaternion.RotateTowards( head.localRotation, networkHeadRotation, 180 * Time.deltaTime );
        }

        public virtual void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
        {
            if ( stream.isWriting )
            {
                if ( PhotonNetwork.isMasterClient )
                {
                    stream.SendNext( maxHp );
                    stream.SendNext( Hp );
                }
                stream.SendNext( head.localRotation );
                networkHeadRotation = head.localRotation;
            }
            else
            {
                if ( !PhotonNetwork.isMasterClient )
                {
                    maxHp = (int)stream.ReceiveNext();
                    Hp = (int)stream.ReceiveNext();
                }
                networkHeadRotation = (Quaternion)stream.ReceiveNext();
            }
        }
    }
}