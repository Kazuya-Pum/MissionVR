using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBAEngine.Skills
{
    public class MineObject : PlayerObject
    {
        [HideInInspector]
        public int Damage = 0;
        [HideInInspector]
        public string BombEffect = null;//着弾時発生オブジェクト
        [HideInInspector]
        public float DestoyTime = 0;//時間経過による自壊までのタイムリミット
        //[HideInInspector]
        //public GameObject player;
        float startime;
        // Use this for initialization
        void Start()
        {
            startime = Time.time;
        }

        // Update is called once per frame
        void Update()
        {
            if (!photonView.isMine) return;
            if ((Time.time - startime) > DestoyTime && PhotonNetwork.isMasterClient)
                PhotonNetwork.Destroy(gameObject);
        }

        virtual public void OnTriggerEnter(Collider c)
        {
            if (!photonView.isMine) return;
            if (c.tag != "Player") return;
            LocalVariables charaOther = c.GetComponent<LocalVariables>();
            LocalVariables charaPlayer = player.GetComponent<LocalVariables>();
            if (charaOther == null) return;
            if(charaOther.team != charaPlayer.team)
            {
                charaPlayer.gameObject.GetComponent<Chara>().networkManager.photonView.RPC("SendSkillDamage",PhotonTargets.MasterClient, player.GetPhotonView().ownerId, charaOther.gameObject.GetPhotonView().ownerId, Damage, charaOther.gameObject.transform.root.gameObject.GetPhotonView().viewID);
            }
            //IPlayer p;
            //if ((p = c.gameObject.GetComponent<IPlayer>()) != null)//プレイヤーに着弾時ダメージ
            //{
            //    p.Damage(Hit(Damage));
            //}
            if (BombEffect != null)
            {
                PhotonNetwork.Instantiate(BombEffect, transform.position, Quaternion.identity,0);
            }
            if(PhotonNetwork.isMasterClient)
                PhotonNetwork.Destroy(gameObject);//自壊
        }

        private int Hit(int damage)
        {
            return damage;
        }
    }
}
