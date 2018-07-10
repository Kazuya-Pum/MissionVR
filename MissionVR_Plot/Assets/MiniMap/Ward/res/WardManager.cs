using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WardTargetManager : MonoBehaviour
{
    public TeamColor team;
    public GameObject Enemy;

    public void Appear()    //Wardの範囲に入ると呼び出される
    {
        Enemy.SetActive(true);
    }

    public void Hide()  //Wardの範囲外に出ると呼び出される
    {
        Enemy.SetActive(false);
    }

}
