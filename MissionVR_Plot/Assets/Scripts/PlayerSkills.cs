using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerSkills : MonoBehaviour,MOBAEngine.Skills.IPlayer
{
    [SerializeField]
    private MOBAEngine.Skills.ThrowSkillBase Skill;
    private MOBAEngine.Skills.IPlayer IP;

    private MOBAEngine.Skills.SkillBase SkillBase = new MOBAEngine.Skills.ThrowSkillBase();
    private Abnormal.AbnormalType AbnormalType=new Abnormal.AbnormalType();

    private float mana;

    /*
    private void Skill(int skill_ID,int num)
    {
        switch (skill_ID)
        {
            case 1:
                // バフ
                Sprint();
                break;

            case 2:
                // バフ
                Berserk();
                break;

            case 3:
                // バフ
                ArmorUp();
                break;

            case 4:
                // エリアバフ
                AdvancedSprint();
                break;

            case 5:
                // バフ
                ReloadMaster();
                break;

            case 6:
                // エリアバフ
                AdvancedBerserk();
                break;

            case 7:
                // エリアバフ
                AdvancedArmorUp();
                break;

            case 8:
                // エリア回復
                Heal();
                break;

            case 9:
                // 回復
                MediKit();
                break;

            case 10:
                // Throw
                Shot();
                break;

            case 11:
                // Throw
                FlameBullet();
                break;

            case 12:
                // Throw
                ExplosionBomb();
                break;

            case 13:
                // Throw
                HeavyBullet();
                break;

            case 14:
                Grenade();
                break;

            case 15:
                FlameRadiation();
                break;
            case 16:
                PierceS();
                break;

            case 17:
                Missile();
                break;

            case 18:
                TazaGun();
                break;

            case 19:
                RailGun();
                break;

            case 20:
                Armor_PiercingAmmunition();
                break;
            case 21:
                Request();
                break;
            case 22:
                Radar();
                break;

            case 23:
                Drone();
                break;

            case 24:
                EMP_Strike();
                break;

            case 25:
                HighGravity();
                break;

            default:
                Debug.Log("error:スキルＩＤがおかしい");
                break;
        }
    }

    */
    
    // バフ
    private void Sprint(MOBAEngine.Skills.SkillBase Skill)
    {
        mana -= Skill.ManaCost;
        Skill.UseSkill(IP,gameObject);
        AbnormalType.AbnormalOccurrence("MoveBuff", "Buff", 20f, 15f);
    }

    private void Berserk(MOBAEngine.Skills.SkillBase Skill)
    {
        mana -= Skill.ManaCost;
        Skill.UseSkill(IP,gameObject);
        AbnormalType.AbnormalOccurrence("AttackBuff", "Buff", 30f, 10f);
    }

    private void ArmorUp(MOBAEngine.Skills.SkillBase Skill)
    {
        mana -= Skill.ManaCost;
        Skill.UseSkill(IP,gameObject);
        AbnormalType.AbnormalOccurrence("DffenceBuff", "Buff", 30f, 10f);
    }

    private void AdvancedSprint(MOBAEngine.Skills.AreaSkillBase Skill)
    {
        mana -= Skill.ManaCost;
        Skill.UseSkill(IP,gameObject);
        //AbnormalType.AbnormalOccurrence(a, "Buff", 30f,)
    }

    private void ReloadMaster(MOBAEngine.Skills.SkillBase Skill)
    {
        mana -= Skill.ManaCost;
        Skill.UseSkill(IP,gameObject);
        //AbnormalType.AbnormalOccurrence("ReroadBuff", "Buff", 30f,)
    }

    private void AdvancedBerserk(MOBAEngine.Skills.AreaSkillBase Skill)
    {
        mana -= Skill.ManaCost;
        Skill.UseSkill(IP,gameObject);
        //AbnormalType.AbnormalOccurrence(a, "Buff", 30f,);
    }

    private void AdvancedArmorUp(MOBAEngine.Skills.AreaSkillBase Skill)
    {
        mana -= Skill.ManaCost;
        Skill.UseSkill(IP,gameObject);
        //AbnormalType.AbnormalOccurrence(a, "Buff", 30f,);
    }
    // 回復
    private void Heal(MOBAEngine.Skills.AreaSkillBase Skill)
    {
        mana -= Skill.ManaCost;
        Skill.UseSkill(IP,gameObject);
        AbnormalType.AbnormalOccurrence("Heal", "Heal", Skill.Damage, Skill.Timer);
    }

    private void MediKit(MOBAEngine.Skills.SkillBase Skill)
    {
        mana -= Skill.ManaCost;
        Skill.UseSkill(IP,gameObject);
        AbnormalType.AbnormalOccurrence("Heal","Heal", Skill.CoolTme, Skill.CoolTme);
    }
    

    public void Start()
    {
        IP = this.gameObject.GetComponent<PlayerSkills>();
    }

    public void Damage(int d)
    {
        throw new NotImplementedException();
    }

    public Transform GetPlayerTransform()
    {
        throw new NotImplementedException();
    }
    // 攻撃
    //private void Shot(MOBAEngine.Skills.ThrowSkillBase Skill)
    //{
    //    mana -= Skill.ManaCost;
    //    Skill.UseSkill(IP);
    //}
    
    //private void FlameBullet(MOBAEngine.Skills.ThrowSkillBase Skill)
    //{
    //    mana -= Skill.ManaCost;
    //    Skill.UseSkill(IP);
    //    //AbnormalType.AbnormalOccurrence("","Damage", Skill.Damage,,)
        
    //}

    //private void ExplosionBomb(MOBAEngine.Skills.ThrowSkillBase Skill)
    //{
    //    mana -= Skill.ManaCost;
    //    Skill.UseSkill(IP);
    //}

    //private void HeavyBullet(MOBAEngine.Skills.ThrowSkillBase Skill)
    //{
    //    mana -= Skill.ManaCost;
    //    Skill.UseSkill(IP);
    //}

    //private void Grenade(MOBAEngine.Skills.ThrowSkillBase Skill)
    //{
    //    mana -= Skill.ManaCost;
    //    Skill.UseSkill(IP);
    //}

    //private void FlameRadiation(MOBAEngine.Skills.LaySkillBase Skill)
    //{
    //    mana -= Skill.ManaCost;
    //    Skill.UseSkill(IP);
    //}

    //private void PierceShot(MOBAEngine.Skills.ThrowSkillBase Skill)
    //{
    //    mana -= Skill.ManaCost;
    //    Skill.UseSkill(IP);
    //}

    //private void Missile(MOBAEngine.Skills.ThrowSkillBase Skill)
    //{
    //    mana -= Skill.ManaCost;
    //    Skill.UseSkill(IP);
    //}

    //private void TazaGun(MOBAEngine.Skills.LaySkillBase Skill)
    //{
    //    mana -= Skill.ManaCost;
    //    Skill.UseSkill(IP);
    //}

    //private void RailGun(MOBAEngine.Skills.LaySkillBase Skill)
    //{
    //    mana -= Skill.ManaCost;
    //    Skill.UseSkill(IP);
    //}

    //private void Armor_PiercingAmmunition (MOBAEngine.Skills.ThrowSkillBase Skill)
    //{
    //    mana -= Skill.ManaCost;
    //    Skill.UseSkill(IP);
    //}
    
    //private void Request(MOBAEngine.Skills.AreaSkillBase Skill)
    //{
    //    mana -= Skill.ManaCost;
    //    Skill.UseSkill(IP);
    //}
    //// 特殊
    //private void Radar(MOBAEngine.Skills.AreaSkillBase Skill)
    //{
    //    mana -= Skill.ManaCost;
    //    Skill.UseSkill(IP);
    //}

    //private void Mine(MOBAEngine.Skills.TokenSkillBase Skill)
    //{
    //    mana -= Skill.ManaCost;
    //    Skill.UseSkill(IP);
    //}

    //private void Drone(MOBAEngine.Skills.TokenSkillBase Skill)
    //{
    //    mana -= Skill.ManaCost;
    //    Skill.UseSkill(IP);
    //}

    //private void EMP_Strike()
    //{

    //}

    //private void HighGravity(MOBAEngine.Skills.SkillBase Skill)
    //{
    //    mana -= Skill.ManaCost;
    //    Skill.UseSkill(IP);
    //}

}
