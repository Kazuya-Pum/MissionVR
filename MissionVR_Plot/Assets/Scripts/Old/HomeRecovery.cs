using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeRecovery : MonoBehaviour {

    GameObject[] players;
    GameObject[] AllyPlayers;
    TeamColor team;
    [SerializeField]
    private float healingRange = 70f;

	// Use this for initialization
	void Start () {
        team = this.gameObject.transform.parent.GetComponent<LocalVariables>().team;
        players = GameObject.FindGameObjectsWithTag("Player");
	}
	
	// Update is called once per frame
	void Update () {
	}

    private float HealingAreaRange(GameObject target, GameObject thisObject)
    {
        float xRange;
        float zRange;
        float range;
        xRange = (target.transform.position.x - thisObject.transform.position.x);
        zRange = (target.transform.position.z - thisObject.transform.position.z);
        range = xRange * xRange + zRange * zRange;
        return range;
    }
}
