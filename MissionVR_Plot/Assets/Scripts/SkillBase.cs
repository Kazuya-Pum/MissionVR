using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBAEngine.Skills
{
    public class SkillBase : ScriptableObject//全スキルのベースになる継承元
    {
        [SerializeField]
        string skillname;//スキル名
        public string Name { get { return skillname; } }

        [SerializeField]
        int manaCost;//スキルコスト
        public int ManaCost { get { return manaCost; } private set { manaCost = value; } }

        [SerializeField]
        float coolTime;//クールタイム
        public float CoolTme { get { return coolTime; } private set { coolTime = value; } }

        public virtual int UseSkill(IPlayer p , GameObject player) {//スキル使用関数
            return 0;
        }
    }
}