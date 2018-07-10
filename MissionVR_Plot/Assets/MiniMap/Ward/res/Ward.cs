using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WardManager : MonoBehaviour
{
    public TeamColor team;

    public GameObject Hit;//Wardの当たり判定と見た目用のオブジェクト

    private void OnTriggerStay(Collider other)
    {
        //tagがTargetでかつWardと違うチーム
        if (other.gameObject.tag == "Target" && team != other.gameObject.GetComponent<WardTargetManager>().team)
        {
            other.gameObject.GetComponent<WardTargetManager>().Appear();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Target" && team != other.gameObject.GetComponent<WardTargetManager>().team)
        {
            other.gameObject.GetComponent<WardTargetManager>().Hide();
        }

    }

    private void Update()
    {
        //当たり判定のオブジェクトが消滅したらWard本体も消える
        if(Hit == null)
        {
            Destroy(gameObject);
        }
    }

}
