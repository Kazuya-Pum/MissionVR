using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubLocalVariables {


    public TeamColor team;

    public Lane lane;

    public ObjectType objType;

    //体力
    public int Hp;

    public int MaxHp;

    //マナ
    public int Mana;

    public int ManaMax;

    //獲得金額
    public int money;

    //キル数
    public int killCount;

    //デス数
    public int deathCount;

    //体力自動回復
    public int AutomaticHpResilience;

    //マナ自動回復
    public int AutomaticManaResilience;

    //移動速度
    public float MoveSpeed;

    //攻撃速度
    public float AttackSpeed;

    //物理防御力
    public float PhysicalDefence;

    //物理攻撃力
    public float PhysicalOffence;

    //魔法防御力
    public float MagicalDefence;

    //魔法攻撃力
    public float MagicalOffence;

    public int Exp;

    public int Level;

    Chara playerChara;

    PhotonView photonView;

    public SubLocalVariables()
    {

    }

}
