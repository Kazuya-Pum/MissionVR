using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultMenu : Photon.MonoBehaviour {

    [SerializeField] private Image[] selectedCharacters;
    [SerializeField] private Sprite[] charaSprites;
    [SerializeField] private Text messageText;
    [SerializeField] private Text[] informationText;//情報表示用テキスト

    private int[] charaIDSelected;//選択されたキャラのID
    private int[] killCount;//キル数
    private int[] gainMoney;//獲得金額
    private int[] deathCount;//デス数
    private bool isActive;
    private bool isWon;
    private bool isReverse;


	void Start () {
        //勝敗をここで取得する（現状まだ）
        isWon = (PlayerPrefs.GetString("WinOrLose") == "Win") ? true : false;
        isActive = true;
        //isWon = true;
        isReverse = false;
        int playerCount = PhotonNetwork.room.MaxPlayers;
        charaIDSelected = new int[playerCount];
        killCount = new int[playerCount];
        gainMoney = new int[playerCount];
        deathCount = new int[playerCount];

        //情報を表示
        for (int index = 0; index < playerCount; index++)
        {
            //情報取得
            charaIDSelected[index] = PlayerPrefs.GetInt("SelectChara" + index);
            killCount[index] = PlayerPrefs.GetInt("KillCount" + index);
            gainMoney[index] = PlayerPrefs.GetInt("Money" + index);
            deathCount[index] = PlayerPrefs.GetInt("DeathCount" + index);

            //画像表示
            selectedCharacters[index].gameObject.SetActive(true);
            selectedCharacters[index].sprite = charaSprites[charaIDSelected[index]];

            //情報表示
            informationText[index].gameObject.SetActive(true);
            informationText[index].text = killCount[index] + " / " + gainMoney[index] + " / " + deathCount[index];
        }
        StartCoroutine(ResultMessage());
	}


    void Update () {
		
	}

    IEnumerator ResultMessage()
    {
        while (true)
        {
            if (isActive)
            {
                isActive = false;
                if (isReverse)
                {
                    isReverse = false;
                    messageText.text = (isWon) ? "＜(^o^)＞勝利＜(^o^)＞" : "（T-T）敗北（T-T）";
                }
                else
                {
                    isReverse = true;
                    messageText.text = (isWon) ? "＼(^o^)／勝利＼(^o^)／" : "（・-・）敗北（・-・）";
                }
                yield return new WaitForSeconds(0.5f);
                isActive = true;
            }
        }
    }
}
