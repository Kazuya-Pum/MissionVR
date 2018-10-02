﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * タワーの攻撃対象やダメージの管理をするスクリプト
 * 実際はそれぞれのチームの色ごとに分かれたコライダー用スクリプトから判定を取得する
 */
public class ProjectorBehaviour : MonoBehaviour
{
    #region Variables
    public List<GameObject> atkTask = new List<GameObject>();//攻撃対象を格納する
    bool isRunning = false;//攻撃間隔管理用のFlag
    TeamColor team;//自身のチーム
    TeamColor otherTeam;//相手のチーム
    LocalVariables projectorLocalVariables;//Projectorのlocalな変数管理スクリプト

    private int dmg;//与ダメ
    [SerializeField]
    private float searchRange = 50f;

    ProjectorManager projectorManager;
    #endregion

    private void Start()
    {
        projectorLocalVariables = this.gameObject.transform.parent.GetComponent<LocalVariables>();
        team = projectorLocalVariables.team;
        projectorManager = this.gameObject.transform.parent.GetComponent<ProjectorManager>();
    }

    private void Update()
    {
        if (!PhotonNetwork.isMasterClient) return;

        //searchTaskのListの第一要素がnullの場合removeする
        if (atkTask.Count != 0 && atkTask[0] == null)
        {
            atkTask.RemoveAt(0);
        }
    }

    /*TriggerEnterに呼ばれる、引数は(対象のオブジェクト,このタワーのチームの色)*/
    public void Enter(Collider other, TeamColor l)
    {

        otherTeam = other.gameObject.GetComponent<LocalVariables>().team;//対象のチーム取得

        if (otherTeam != l)//対象のチームとこのタワーのチームが違ったら攻撃対象に追加
        {
            //Debug.Log("タワー範囲内に侵入");
            if (other.gameObject.tag == "NeutralCreep")
                return;

            if (other.gameObject.tag == "Tower")
                return;

            atkTask.Add(other.gameObject);
        }
    }

    //TriggerExitに呼ばれる
    public void Exit(Collider other, TeamColor l)
    {
        otherTeam = other.gameObject.GetComponent<LocalVariables>().team;

        if (otherTeam != l)
        {
            //            Debug.Log("タワー範囲外へ退出");

            //範囲外へ出たら攻撃対象から除外する
            for (int i = 0; i < atkTask.Count; i++)
            {
                if (atkTask[i] == other.gameObject)
                {
                    atkTask.Remove(atkTask[i]);
                }
            }
        }
    }

    //TriggerStayから呼ばれる
    public void Stay(Collider other, TeamColor l)
    {
        if (atkTask.Count != 0)
        {
            if (l != team)
            {
                atkTask.Remove(atkTask[0]);
                return;
            }
            if (other.gameObject == atkTask[0]
                && !isRunning)
            {
                if (AttackRange(atkTask[0], this.gameObject) <= searchRange * searchRange)
                {
                    StartCoroutine(Attacking(other.gameObject));
                    isRunning = true;
                }
                else
                {
                    atkTask.RemoveAt(0);
                }
            }
        }
        otherTeam = other.gameObject.GetComponent<LocalVariables>().team;
    }

    private float AttackRange(GameObject target, GameObject thisObject)
    {
        float xRange;
        float zRange;
        float range;
        xRange = (target.transform.position.x - thisObject.transform.position.x);
        zRange = (target.transform.position.z - thisObject.transform.position.z);
        range = xRange * xRange + zRange * zRange;
        return range;
    }

    //攻撃コルーチン
    IEnumerator Attacking(GameObject target)
    {
        if (target.tag == "Player")
        {
            if (!PhotonNetwork.isMasterClient)
                yield break;

            //対象のlocal変数管理スクリプト取得を取得してダメージ処理
            var localVariables = target.GetComponent<LocalVariables>();
            var chara = target.GetComponent<LocalVariables>();

            if (chara.team == team) yield break;
            /*
              * ダメージ計算式(とりあえず簡単にしておきます)
              * 与えるダメージは、{(自身の物理攻撃力 - 相手の物理防御力) + (自身の魔法攻撃力 - 相手の魔法防御力)}
              */
            dmg = (int)((projectorLocalVariables.PhysicalOffence - localVariables.PhysicalDefence)
                 + (projectorLocalVariables.MagicalOffence - localVariables.MagicalDefence));

            //回復しないように
            if (dmg < 0)
            {
                dmg = 0;
            }

            Debug.Log("Projectorが攻撃、ダメージ量 : " + dmg);
            //引数はダメージ量(int値)
            localVariables.Damage(dmg);
            yield return new WaitForSeconds(projectorManager.ProjectorAttackSpeed);
            isRunning = false;
            yield break;
        }
    }
}
