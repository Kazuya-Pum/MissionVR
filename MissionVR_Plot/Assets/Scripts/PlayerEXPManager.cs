using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEXPManager : MonoBehaviour {

    private List<GameObject> EnemyObject = new List<GameObject>();
    TeamColor myTeam;
	// Use this for initialization
	void Start () {
        myTeam = this.gameObject.transform.parent.GetComponent<LocalVariables>().team;
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<LocalVariables>().team != myTeam)
        {
            EnemyObject.Add(other.gameObject);
        }
    }

    private void Update()
    {
    }

}
