using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeutralCreepLocalVariables : LocalVariables {

    #region UniqueVariables

    [SerializeField]
    private int neutralCreepHp = 10;

    [SerializeField]
    private int neutralCreepMana = 0;

    [SerializeField]
    private int neutralCreepPhysicalOffence = 2;

    [SerializeField]
    private int neutralCreepPhysicalDiffence = 1;

    [SerializeField]
    private int neutralCreepMagicalOffence = 1;

    [SerializeField]
    private int neutralCreepMagicalDiffence = 1;

    [SerializeField]
    private int neutralCreepHpResilience = 0;

    [SerializeField]
    private int neutralCreepManaResilience = 0;

    [SerializeField]
    private float neutralCreepAttackSpeed = 1;

    [SerializeField]
    private float neutralCreepMoveSpeed = 1;

    #endregion

    protected void Start()
    {
        //neutralCreepの時のみ
        Hp = neutralCreepHp;
        Mana = neutralCreepMana;
        PhysicalDefence = neutralCreepPhysicalDiffence;
        PhysicalOffence = neutralCreepPhysicalOffence;
        MagicalDefence = neutralCreepMagicalDiffence;
        MagicalOffence = neutralCreepMagicalOffence;
        AutomaticHpRecovery = neutralCreepHpResilience;
        AutomaticManaRecovery = neutralCreepManaResilience;
        AttackSpeed = neutralCreepAttackSpeed;
        MoveSpeed = neutralCreepMoveSpeed;
    }

    private void Update()
    {
        if (Hp <= 0)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
    }
}
