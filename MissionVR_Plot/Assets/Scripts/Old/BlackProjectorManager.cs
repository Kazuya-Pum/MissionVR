using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//BlackチームのProjectorを管理するスクリプト
public class BlackProjectorManager : MonoBehaviour {

    #region UniqueVariables
    //[SerializeField]
    protected int ProjectorHp = 500;
    [SerializeField]
    protected int ProjectorMana = 0;
    [SerializeField]
    protected int ProjectorPhysicalOffence = 0;
    [SerializeField]
    protected int ProjectorPhysicalDefence = 0;
    [SerializeField]
    protected int ProjectorMagicalOffence = 0;
    [SerializeField]
    protected int ProjectorMagicalDefence = 0;
    [SerializeField]
    protected int ProjectorAutomaticHpRecovery = 1;
    [SerializeField]
    protected int ProjectorAutomaticManaRecovery = 0;
    [SerializeField]
    protected int ProjectorAttackSpeed = 0;
    [SerializeField]
    protected int ProjectorMoveSpeed = 0;

    private LocalVariables blackLocalVariables;
    #endregion

    // Use this for initialization
    void Awake () {

        ProjectorManager projectorManager = GameObject.Find("ProjectorManager").GetComponent<ProjectorManager>();

        blackLocalVariables = this.gameObject.GetComponent<LocalVariables>();

        blackLocalVariables.Hp = projectorManager.ProjectorHp;
        blackLocalVariables.MaxHp = projectorManager.ProjectorHp;
        blackLocalVariables.Mana = ProjectorMana;
        blackLocalVariables.PhysicalDefence = ProjectorPhysicalDefence;
        blackLocalVariables.PhysicalOffence = projectorManager.ProjectorPhysicalOffence;
        blackLocalVariables.MagicalDefence = ProjectorMagicalDefence;
        blackLocalVariables.MagicalOffence = projectorManager.ProjectorMagicalOffence;
        blackLocalVariables.AutomaticHpRecovery = ProjectorAutomaticHpRecovery;
        blackLocalVariables.AutomaticManaRecovery = ProjectorAutomaticManaRecovery;
        blackLocalVariables.MoveSpeed = ProjectorMoveSpeed;
        blackLocalVariables.AttackSpeed = ProjectorAttackSpeed;

        //チームを設定
        blackLocalVariables.team = TeamColor.Black;
	}
}
