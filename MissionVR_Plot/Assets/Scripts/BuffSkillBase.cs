using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBAEngine.Skills
{
    [CreateAssetMenu]
    public class BuffSkillBase : SkillBase
    {
        [SerializeField]
        float raito = 0;//割合
        public float Raito { get { return raito; } }
        [SerializeField]
        float life = 1;
        public float Life { get { return life; } }//持続時間
        [SerializeField]
        string status = "";//変更ステータス
        public string Speed { get { return status; } }
        
        public override int UseSkill(IPlayer p,GameObject player)
        {
            base.UseSkill(p,player);
            return 0;
        }
    }
}