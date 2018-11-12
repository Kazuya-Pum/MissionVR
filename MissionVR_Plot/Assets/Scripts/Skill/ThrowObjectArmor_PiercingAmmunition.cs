﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBAEngine.Skills
{
    public class ThrowObjectArmor_PiercingAmmunition : ThrowObject
    {
        //[HideInInspector]
        //public int Damage = 0;
        //[HideInInspector]
        //public float Speed = 0;
        //[HideInInspector]
        //public bool TrackingFlag = false;//ONにして追尾弾化
        //[HideInInspector]
        //public GameObject TrackTarget = null;//追尾ターゲット
        ////[HideInInspector]
        ////public GameObject player;
        //[HideInInspector]
        //public string BombEffect = null;//着弾時発生オブジェクト
        //[HideInInspector]
        //public float DestoyTime = 0;//時間経過による自壊までのタイムリミット
        [HideInInspector]
        public TeamColor teamColorSkillObject;

        float startime;
        // Use this for initialization
        void Start()
        {
            GetComponent<Rigidbody>().velocity = transform.forward * Speed;
            startime = Time.time;
        }

        // Update is called once per frame
        void Update()
        {
            if (!photonView.isMine) return;
            if (TrackingFlag)//追尾処理
                GetComponent<Rigidbody>().velocity = (TrackTarget.transform.position - transform.position).normalized * Speed;
            if ((Time.time - startime) > DestoyTime && PhotonNetwork.isMasterClient)
                PhotonNetwork.Destroy(gameObject);
        }

        virtual public void OnTriggerEnter(Collider c)
        {
            if (!photonView.isMine) return;
            if (c.tag != "Player") return;
            LocalVariables charaPlayer = player.GetComponent<LocalVariables>();
            LocalVariables charaOther = c.GetComponent<LocalVariables>();
            if(charaOther != null && charaOther.team != teamColorSkillObject)
            {
                charaPlayer.gameObject.GetComponent<Chara>().networkManager.photonView.RPC("SendSkillDamage", PhotonTargets.MasterClient, player.GetPhotonView().ownerId, c.gameObject.GetPhotonView().ownerId,Damage,c.gameObject.transform.root.gameObject.GetPhotonView().viewID);
            }
            //if ((p = c.gameObject.GetComponent<IPlayer>()) != null)//プレイヤーに着弾時ダメージ
            //{
            //    p.Damage(Hit(Damage));
            //}
            if (BombEffect != null)
            {
                PhotonNetwork.Instantiate(BombEffect, transform.position, Quaternion.identity,0);
            }
            if(PhotonNetwork.isMasterClient)
                PhotonNetwork.Destroy(gameObject);//着弾後自壊
        }

        private int Hit(int damage)
        {
            return damage;
        }
    }
}