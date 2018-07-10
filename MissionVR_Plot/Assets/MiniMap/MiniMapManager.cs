using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapManager : MonoBehaviour
{
    public Camera mmCamera; //全体を映すカメラ

    public GameObject BigView;
    public GameObject MiniMapImage;
    public Transform BigViewContent;
    public Slider BigSlider;
    float value;

    bool isRunning = false;

    public bool isEMP;//EMPストライクによりTrueになり、Trueの間、ミニマップが表示されなくなる
    private float empTimer;
    private float empTime;//EMP有効時間

    public Transform Player;//ミニマップの中央をプレイヤー

    private void Start()
    {
        //Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        empTime = 10;
    }

    private void Update()
    {   //Mキー押したらマップ大表示
        if (isEMP)
        {
            MiniMapImage.SetActive(false);
            BigView.SetActive(false);
            empTimer += Time.deltaTime;
            if (empTimer > empTime) isEMP = false;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.M) && isRunning == false)
            {
                MiniMapImage.SetActive(false);
                BigView.SetActive(true);
                isRunning = true;
            }
            else if (Input.GetKeyDown(KeyCode.M) && isRunning == true)
            {
                MiniMapImage.SetActive(true);
                BigView.SetActive(false);
                isRunning = false;
            }

            if (isRunning == false && Player != null)
            {
                mmCamera.transform.position = new Vector3(Player.position.x, mmCamera.transform.position.y, Player.position.z);
            }
        }
        
    }

    public void Size()
    {
        value = BigSlider.value;
        BigViewContent.localScale = new Vector3(value, value, 1f);
    }

}
