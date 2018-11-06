using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteProjectorManager : MonoBehaviour {

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

    private LocalVariables whiteLocalVariables;

    #endregion

    // Use this for initialization
    void Awake () {

        ProjectorManager projectorManager = GameObject.Find("ProjectorManager").GetComponent<ProjectorManager>();

        whiteLocalVariables = this.gameObject.GetComponent<LocalVariables>();

        whiteLocalVariables.Hp = projectorManager.ProjectorHp;
        whiteLocalVariables.MaxHp = projectorManager.ProjectorHp;
        whiteLocalVariables.Mana = ProjectorMana;
        whiteLocalVariables.PhysicalDefence = ProjectorPhysicalDefence;
        whiteLocalVariables.PhysicalOffence = ProjectorPhysicalOffence;
        whiteLocalVariables.MagicalDefence = ProjectorMagicalDefence;
        whiteLocalVariables.MagicalOffence = ProjectorMagicalOffence;
        whiteLocalVariables.AutomaticHpRecovery = ProjectorAutomaticHpRecovery;
        whiteLocalVariables.AutomaticManaRecovery = ProjectorAutomaticManaRecovery;
        whiteLocalVariables.MoveSpeed = ProjectorMoveSpeed;
        whiteLocalVariables.AttackSpeed = ProjectorAttackSpeed;

        whiteLocalVariables.team = TeamColor.White;
	}
	
}
