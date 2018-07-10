using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerNumberManager : MonoBehaviour {

    [SerializeField]
    public GameObject[] whiteTower;
    [SerializeField]
    public GameObject[] BlackTower;
    private LocalVariables towerLocalVariables;
    private WhiteTowerManager whiteTowerManager;
    private BlackTowerManager blackTowerManager;
    [SerializeField]
    public GameObject WhiteProjector;
    [SerializeField]
    public GameObject BlackProjector;
    [SerializeField]
    public int towerHP = 500;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (whiteTower[0] != null)
        {
            towerLocalVariables = whiteTower[1].GetComponent<LocalVariables>();
            towerLocalVariables.Hp = towerLocalVariables.MaxHp;
        }

        if (whiteTower[2] != null)
        {
            towerLocalVariables = whiteTower[3].GetComponent<LocalVariables>();
            towerLocalVariables.Hp = towerLocalVariables.MaxHp;
        }

        if (whiteTower[4] != null)
        {
            towerLocalVariables = whiteTower[5].GetComponent<LocalVariables>();
            towerLocalVariables.Hp = towerLocalVariables.MaxHp;
        }

        if (whiteTower[1] != null && whiteTower[3] != null && whiteTower[5] != null)
        {
            towerLocalVariables = whiteTower[6].GetComponent<LocalVariables>();
            towerLocalVariables.Hp = towerLocalVariables.MaxHp;
            towerLocalVariables = whiteTower[7].GetComponent<LocalVariables>();
            towerLocalVariables.Hp = towerLocalVariables.MaxHp;
        }

        if (whiteTower[6] != null || whiteTower[7] != null)
        {
            towerLocalVariables = WhiteProjector.GetComponent<LocalVariables>();
            towerLocalVariables.Hp = towerLocalVariables.MaxHp;
        }

        if (BlackTower[0] != null)
        {
            towerLocalVariables = BlackTower[1].GetComponent<LocalVariables>();
            towerLocalVariables.Hp = towerLocalVariables.MaxHp;
        }

        if (BlackTower[2] != null)
        {
            towerLocalVariables = BlackTower[3].GetComponent<LocalVariables>();
            towerLocalVariables.Hp = towerLocalVariables.MaxHp;
        }

        if (BlackTower[4] != null)
        {
            towerLocalVariables = BlackTower[5].GetComponent<LocalVariables>();
            towerLocalVariables.Hp = towerLocalVariables.MaxHp;
        }

        if (BlackTower[1] != null && BlackTower[3] != null && BlackTower[5] != null)
        {
            towerLocalVariables = BlackTower[6].GetComponent<LocalVariables>();
            towerLocalVariables.Hp = towerLocalVariables.MaxHp;
            towerLocalVariables = BlackTower[7].GetComponent<LocalVariables>();
            towerLocalVariables.Hp = towerLocalVariables.MaxHp;
        }

        if (BlackTower[6] != null || BlackTower[7] != null)
        {
            towerLocalVariables = BlackProjector.GetComponent<LocalVariables>();
            towerLocalVariables.Hp = towerLocalVariables.MaxHp;
        }
    }
}
