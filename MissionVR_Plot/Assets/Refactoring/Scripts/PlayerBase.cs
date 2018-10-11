using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{

    public class PlayerBase : MobBase
    {
        [SerializeField] private int myExp;
        [SerializeField] private int myMoney;
        [SerializeField] private int level;
        [SerializeField] private float autoRecoverSpam;

        [SerializeField] private GrowthValues growthValues;

        protected Collider playerCollider;

        private float localSensitivity;

        protected override void Awake()
        {
            base.Awake();

            autoRecoverSpam = ( autoRecoverSpam <= 0 ) ? 1f : autoRecoverSpam;
            level = 1;

            entityType = EntityType.CHANPION;

            if ( photonView.isMine )
            {
                head.Find( "Main Camera" ).gameObject.SetActive( true );
            }

            playerCollider = GetComponent<Collider>();
        }

        protected override void Update()
        {
            base.Update();

            if ( PhotonNetwork.isMasterClient )
            {
                AutoRecover();
                photonView.RPC( "FetchLocalParams", PhotonTargets.Others, maxHp, Hp, maxMana, Mana, myExp, myMoney, level, moveSpeed );
            }
        }

        [PunRPC]
        public void FetchSetting( float sensitivity )
        {
            localSensitivity = sensitivity;
        }

        [PunRPC]
        protected override void Rotate( float x = 0, float y = 0 )
        {
            base.Rotate( x * localSensitivity, y * localSensitivity );
            modelRotate.localRotation = head.localRotation;
        }

        protected override void UpdateRotation()
        {
            base.UpdateRotation();
            modelRotate.localRotation = head.localRotation;
        }

        /// <summary>
        /// 報酬を受け取る関数。
        /// エンティティーをキルした際に呼ばれる
        /// <para>prev:Master->Master->next:Master</para>
        /// </summary>
        /// <param name="exp">獲得経験値</param>
        /// <param name="money">獲得金額</param>
        public void GetReward( int exp = 0, int money = 0 )
        {
            myExp += exp;
            myMoney += money;

            if ( photonView.isMine )
            {
                PlayerController.instance.OnGetReward();
            }

            if ( myExp >= level * 100 )
            {
                LevelUp();
            }
        }

        /// <summary>
        /// レベルアップを処理する関数。各種ステータス成長
        /// <para>prev:Master->Master->next:Master</para>
        /// </summary>
        protected void LevelUp()
        {
            myExp -= level * 100;
            level++;

            maxHp += growthValues.hp;
            maxMana += growthValues.mana;
            physicalAttack += growthValues.physicalAttack;
            physicalDefense += growthValues.physicalDefense;
            magicAttack += growthValues.magicAttack;
            magicDefense += growthValues.magicDefense;
            moveSpeed += growthValues.moveSpeed;

            hpSlider.maxValue = maxHp;

            if ( photonView.isMine )
            {
                PlayerController.instance.OnStatusChanged();
            }

            if ( myExp >= level * 100 )
            {
                LevelUp();
            }
        }

        protected override void Death()
        {
            photonView.RPC( "ToDeathState", PhotonTargets.All );

            StartCoroutine( "Respawn" );
        }

        /// <summary>
        /// 全クライアント上でこのプレイヤーを死亡状態に遷移する関数
        /// <para>prev:Master->All</para>
        /// </summary>
        [PunRPC]
        protected void ToDeathState()
        {
            entityState = EntityState.DEATH;
            playerCollider.enabled = false;
        }

        /// <summary>
        /// 一定時間でリスポーン処理をマスターから全クライアントに対して行う関数
        /// <para>prev:Master->Master->next:All</para>
        /// </summary>
        /// <returns></returns>
        private IEnumerator Respawn()
        {
            yield return GameManager.instance.respawnTime;

            photonView.RPC( "ToAliveState", PhotonTargets.AllViaServer );
        }

        /// <summary>
        /// 全クライアント上でこのプレイヤーを生存状態に遷移する関数
        /// <para>prev:Master->All</para>
        /// </summary>
        [PunRPC]
        protected void ToAliveState()
        {
            Hp = maxHp;
            tfCache.position = GameManager.instance.spawnPoint[(int)team].position;
            head.localEulerAngles = Vector3.zero;
            modelRotate.localRotation = head.localRotation;

            entityState = EntityState.ALIVE;
            playerCollider.enabled = true;
        }

        /// <summary>
        /// HP、Manaの回復
        /// <para>prev:Master->Master</para>
        /// </summary>
        /// <param name="cHp">HPの回復値</param>
        /// <param name="cMana">Manaの回復値（省略可）</param>
        protected void Recover( int cHp, int cMana = 0 )
        {
            Hp += cHp;
            Mana += cMana;
        }

        float autoRecoverTime;
        /// <summary>
        /// 一定時間毎に自動回復を行う関数
        /// <para>prev:Master->Master->next:Master</para>
        /// </summary>
        protected void AutoRecover()
        {
            autoRecoverTime += Time.deltaTime;
            if ( autoRecoverTime >= autoRecoverSpam )
            {
                Recover( 1, 1 );
                autoRecoverTime -= autoRecoverSpam;
            }
        }

        /// <summary>
        /// マスタークライアントで処理した各プレイヤーの変数をそれぞれローカルに反映させる。
        /// <para>prev:Master->Master以外</para>
        /// <para>更新処理部分で必要な変数のみ更新するようにするべき？</para>
        /// </summary>
        /// <param name="maxHp"></param>
        /// <param name="hp"></param>
        /// <param name="maxMana"></param>
        /// <param name="mana"></param>
        /// <param name="myExp"></param>
        /// <param name="myMoney"></param>
        /// <param name="level"></param>
        [PunRPC]
        protected void FetchLocalParams( int maxHp, int hp, int maxMana, int mana, int myExp, int myMoney, int level, float moveSpeed )
        {
            this.maxHp = maxHp;
            Hp = hp;
            this.maxMana = maxMana;
            Mana = mana;
            this.myExp = myExp;
            this.myMoney = myMoney;
            this.level = level;
            this.moveSpeed = moveSpeed;
        }

        public int MaxHp
        {
            get
            {
                return maxHp;
            }
        }

        public int MaxMana
        {
            get
            {
                return maxMana;
            }
        }

        public int PhysicalAttack
        {
            get
            {
                return physicalAttack;
            }
        }

        public int PhysicalDefense
        {
            get
            {
                return physicalDefense;
            }
        }

        public int MagicAttack
        {
            get
            {
                return magicAttack;
            }
        }

        public int MagicDefense
        {
            get
            {
                return magicDefense;
            }
        }

        public float MoveSpeed
        {
            get
            {
                return moveSpeed;
            }
        }

        public int Level
        {
            get
            {
                return level;
            }
        }

        public int MyMoney
        {
            get
            {
                return myMoney;
            }
        }

        public int MyExp
        {
            get
            {
                return myExp;
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
        public int physicalAttack;
        public int physicalDefense;
        public int magicAttack;
        public int magicDefense;
        public float moveSpeed;
    }
}