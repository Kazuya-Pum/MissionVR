using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WardHitManager : LocalVariables {
    //HPなどはこのスクリプトで設定する
    public float time;
	void Start () {
        //Wardに設定したチームを取得
        team = gameObject.transform.parent.GetComponent<WardManager>().team;
	}

    private void Update()
    {
        //Trinketの範囲から出るとtimeが減り始め、0未満で隠れる
        time -= Time.deltaTime;
        if(time < 0)
        {
            gameObject.SetActive(false);
        }
    }

}
