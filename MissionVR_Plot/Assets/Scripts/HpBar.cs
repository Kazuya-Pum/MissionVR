using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HpBar : Photon.MonoBehaviour
{
    LocalVariables localVariables;
    SubLocalVariables ownSubLocalVariables;
    NetworkManager networkManager;
    Color colorHpBarOnHead;
    Color colorHpBarOnHead_Minion;
    float red, green, blue, alpha;
    int playerSliderValue = 100;
    int minionSliderValue = 100;
    int otherSliderValue = 100;
    int playerMaxValue = 100;
    int minionMaxValue = 100;
    int otherMaxValue = 100;
    Slider slider;
    bool setEnd;
    PhotonView playerPhotonView;
    
    // Use this for initialization
    private void Start()
    {
        setEnd = false;
        slider = this.gameObject.GetComponent<Slider>();
    }

    //初期設定用メソッド
    void StartSetting()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();

        if (this.gameObject.transform.parent.parent.tag == "Tower")
        {
            localVariables = this.gameObject.transform.parent.gameObject.transform.parent.gameObject.GetComponent<LocalVariables>();

            otherMaxValue = localVariables.MaxHp;
            slider.maxValue = otherMaxValue;
        }
        else if (this.gameObject.transform.parent.parent.tag == "Projector")
        {
            localVariables = this.gameObject.transform.parent.parent.gameObject.GetComponent<LocalVariables>();

            otherMaxValue = localVariables.MaxHp;
            slider.maxValue = otherMaxValue;
        }
        else if (this.gameObject.transform.root.tag == "Player")
        {
            if (PhotonNetwork.isMasterClient)
            {
                ownSubLocalVariables = networkManager.subLocalVariables[this.gameObject.transform.root.gameObject.GetPhotonView().ownerId - 1];
            }
            else
            {
                playerPhotonView = this.gameObject.transform.root.GetComponent<PhotonView>().photonView;

                if (this.gameObject.transform.root.gameObject.GetPhotonView().ownerId % 2 == 0)//Black
                {
                    colorHpBarOnHead = Color.red;
                }
                else//White
                {
                    colorHpBarOnHead = Color.blue;
                }
            }
            localVariables = this.gameObject.transform.root.gameObject.GetComponent<LocalVariables>();
        }
        else if ( this.gameObject.transform.root.tag == "Minion")
        {
            localVariables = this.gameObject.transform.root.gameObject.GetComponent<MeleeMinionLocalVariables>();

            minionMaxValue = localVariables.MaxHp;
            slider.maxValue = minionMaxValue;
        }
        else
        {
            localVariables = this.gameObject.transform.root.gameObject.GetComponent<LocalVariables>();

            colorHpBarOnHead = this.gameObject.transform.GetChild(1).GetChild(0).GetComponent<Image>().color;
            if ( !PhotonNetwork.isMasterClient) return;
            otherMaxValue = localVariables.MaxHp;
            slider.maxValue = otherMaxValue;
        }

        setEnd = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (setEnd == false)
        {
            StartSetting();
        }
      
        if(setEnd == true)
        {
            if (PhotonNetwork.isMasterClient)//マスタークライアントの場合
            {
                if (this.gameObject.transform.root.tag == "Player")
                {
                    playerMaxValue = ownSubLocalVariables.MaxHp;
                    slider.maxValue = playerMaxValue;

                    playerSliderValue = ownSubLocalVariables.Hp;
                    slider.value = playerSliderValue;

                    if (this.gameObject.transform.root.gameObject.GetPhotonView().ownerId % 2 == 0)//Black
                    {
                        colorHpBarOnHead = Color.red;
                    }
                    else//White
                    {
                        colorHpBarOnHead = Color.blue;
                    }

                    red = colorHpBarOnHead.r;
                    green = colorHpBarOnHead.g;
                    blue = colorHpBarOnHead.b;
                    alpha = colorHpBarOnHead.a;
                    this.gameObject.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = colorHpBarOnHead;
                }
                else//プレイヤー以外
                {
                    if ( this.gameObject.transform.root.tag == "Minion")//ミニオンの場合
                    {
                        minionSliderValue = localVariables.Hp;
                        slider.value = minionSliderValue;

                        switch (localVariables.team)
                        {
                            case TeamColor.Black:
                                colorHpBarOnHead_Minion = Color.red;
                                break;
                            case TeamColor.White:
                                colorHpBarOnHead_Minion = Color.blue;
                                break;
                        }

                        this.gameObject.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = colorHpBarOnHead_Minion;
                    }
                    else//プレイヤーとミニオン以外
                    {
                        otherSliderValue = localVariables.Hp;
                        slider.value = otherSliderValue;

                        switch (localVariables.team)
                        {
                            case TeamColor.Black:
                                colorHpBarOnHead = Color.red;
                                break;
                            case TeamColor.White:
                                colorHpBarOnHead = Color.blue;
                                break;
                        }

                        red = colorHpBarOnHead.r;
                        green = colorHpBarOnHead.g;
                        blue = colorHpBarOnHead.b;
                        alpha = colorHpBarOnHead.a;
                        this.gameObject.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = colorHpBarOnHead;
                    }
                }
            }
            else//マスタークライアントではない場合
            {
                if (this.gameObject.transform.root.tag == "Player")
                {
                    if (playerPhotonView.isMine)
                    {
                        playerMaxValue = localVariables.MaxHp;
                        slider.maxValue = playerMaxValue;

                        playerSliderValue = localVariables.Hp;
                        slider.value = playerSliderValue;
                    }
                    else
                    {
                        slider.maxValue = playerMaxValue;
                        slider.value = playerSliderValue;
                    }

                    this.gameObject.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = colorHpBarOnHead;
                }
                else
                {
                    if (this.gameObject.transform.root.tag == "Minion")
                    {
                        minionSliderValue = localVariables.Hp;
                        slider.value = minionSliderValue;

                        switch ( localVariables.team)
                        {
                            case TeamColor.Black:
                                colorHpBarOnHead_Minion = Color.red;
                                break;
                            case TeamColor.White:
                                colorHpBarOnHead_Minion = Color.blue;
                                break;
                        }
                        this.gameObject.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = colorHpBarOnHead_Minion;
                    }
                    else
                    {
                        colorHpBarOnHead.r = red;
                        colorHpBarOnHead.g = green;
                        colorHpBarOnHead.b = blue;
                        colorHpBarOnHead.a = alpha;

                        this.gameObject.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = colorHpBarOnHead;

                        slider.maxValue = otherMaxValue;
                        slider.value = otherSliderValue;
                    }
                }
            }
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            //データの送信
            stream.SendNext(playerMaxValue);
            stream.SendNext(playerSliderValue);
            stream.SendNext(otherMaxValue);
            stream.SendNext(otherSliderValue);

            stream.SendNext(red);
            stream.SendNext(green);
            stream.SendNext(blue);
            stream.SendNext(alpha);
        }
        else
        {
            if (PhotonNetwork.isMasterClient) return;

            //データの受信
            this.playerMaxValue = (int)stream.ReceiveNext();
            this.playerSliderValue = (int)stream.ReceiveNext();
            this.otherMaxValue = (int)stream.ReceiveNext();
            this.otherSliderValue = (int)stream.ReceiveNext();

            this.red = (float)stream.ReceiveNext();
            this.green = (float)stream.ReceiveNext();
            this.blue = (float)stream.ReceiveNext();
            this.alpha = (float)stream.ReceiveNext();
        }
    }
}