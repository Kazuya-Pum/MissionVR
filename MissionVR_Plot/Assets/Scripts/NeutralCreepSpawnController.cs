using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeutralCreepSpawnController : MonoBehaviour {

    [SerializeField]
    GameObject spawnCreep;
    GameObject spawnCreep_Copy;
    float deathTime;
    [SerializeField]
    float rePopTime = 180;//3分後に復活
    bool deathFlag = false;
    [SerializeField]
    float popTime = 10;//始めにpopする時間
    bool firstPopFlag = false;

	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update () {
        if (!firstPopFlag && Time.time >= popTime)
        {
            spawnCreep_Copy = PhotonNetwork.Instantiate("NeutralCreep", this.gameObject.transform.position, this.gameObject.transform.rotation,0);
            firstPopFlag = !firstPopFlag;
            spawnCreep_Copy.GetComponent<NeutralCreepSearchAndAttack>().neutralCreepSpawnPoint = this.gameObject.transform.position;
        }

        if (spawnCreep == null)
        {
            deathTime = Time.time;
            deathFlag = true;
        }

        if(deathFlag && Time.time >= deathTime + rePopTime)
        {
            deathFlag = false;
            spawnCreep = PhotonNetwork.Instantiate("NeutralCreep", this.gameObject.transform.position, this.gameObject.transform.rotation, 0);
        }
	}
}