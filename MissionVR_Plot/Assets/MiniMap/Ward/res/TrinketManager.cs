using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrinketManager : MonoBehaviour {

    public TeamColor team;
    public float LimitTime;//消滅までの時間

    private void Start()
    {
        //10秒後に消滅
        Destroy(gameObject, LimitTime);
    }

    private void OnTriggerStay(Collider other)
    {
        //tagがWardでかつTrinketと違うチーム
        if (other.gameObject.tag == "Ward" && team != other.gameObject.GetComponent<WardManager>().team)
        {
            //Wardの可視化
            other.gameObject.GetComponent<WardManager>().Hit.SetActive(true);
            //Trinketの範囲からWardが出る、もしくはTriketが消滅したことで範囲からでたときにtimeが減っていき０未満でWardが隠れる
            other.gameObject.GetComponent<WardManager>().Hit.GetComponent<WardHitManager>().time = 1;
        }
    }

}
