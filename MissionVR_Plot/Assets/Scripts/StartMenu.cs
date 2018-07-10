using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon;


public class StartMenu : Photon.MonoBehaviour {


    [SerializeField,Header("最大プレイ人数")] private int maxPlayer;
    [Space(5)]
    //InputField
    [SerializeField] private InputField roomNameField;//ルーム名入力用
    [SerializeField] private InputField passWordField;//パスワード入力用
    [SerializeField] private InputField maxPlayerField;//最大プレイ人数入力用
    [Space(5)]
    //Text
    [SerializeField] private Text pingText;//ping表示用テキスト　pingが高いと、赤く表示させる
    [SerializeField] private Text messageText;//何かしらのメッセージを表示させるText
    [SerializeField] private Text roomNameText;//ルーム名を表示させるText
    //Button
    [Space(5)]
    [SerializeField] private GameObject[] roomButtons;//ルーム選択用ボタン
    [SerializeField] private GameObject[] charaSelectButtons;//キャラ選択用ボタン

    [SerializeField] private Image[] charaImages;//全員が選択しているキャラクター確認用のImage
    [SerializeField] private Sprite[] charaSprites;//キャラクターのアイコン
    private List<string> roomNames;//ルーム名をここに格納して、入室時に使う　更新があるたびに全消去

    [Space(5)]
    //Toggle
    [SerializeField] private Toggle isOpenToggle;//ルームを公開可能にするかを変えるチェックボックス
    [SerializeField] private Toggle isVisibleToggle;//ルーム一覧に表示させるかを変えるチェックボックス
    //その他
    [SerializeField] private GameObject panelRoomCreate;//ルーム作成時に表示されるパネル
    [SerializeField] private GameObject panelTopMenu;//最初に表示されるパネル
    [SerializeField] private GameObject panelRoomMenu;//ルームに入っているときに表示するパネル
    [SerializeField] private GameObject panelSelectChara;//キャラクター選択画面のパネル
    [SerializeField] private GameObject roomContentField;//ルーム一覧を表示させるContent
    [SerializeField] private GameObject roomButton;//ルームに入るためのボタン
    [SerializeField] private GameObject gameButton;//ゲームシーンに映るためのボタン
    //[SerializeField] private GameObject buttonSpawnOrigin;//ボタン生成の初期地点のオブジェクト


    private Queue<string> messages;
    private string roomName;//ルーム名
    private string passWord;//パスワード
    private int ping;//pingを入れる
    private int id;//選択しているキャラのID
    private bool roomIsOpen;//入室可能かどうか
    private bool roomIsVisible;//ルーム一覧に見えるかどうか
    private bool isInRoom;
    private bool isMax;//ルームが最大人数になっているかどうか
    private bool isInCharaSelect;//キャラ選択画面にいるかどうか
    private int[] charaIDSelected;//ルーム内のプレイヤー全員が選択しているキャラのID


    private void Awake()
    {
        PhotonNetwork.autoJoinLobby = true;
        PhotonNetwork.ConnectUsingSettings(null);
    }

    // Use this for initialization
    void Start () {
        //Photon関連の設定
        //isInRoom = false;
        
        //Panelを非表示にする
        panelRoomCreate.SetActive(false);
        panelTopMenu.SetActive(true);
        panelRoomMenu.SetActive(false);
        panelSelectChara.SetActive(true);
        //メッセージテキストを表示して、文字入れる
        messageText.gameObject.SetActive(true);
        messageText.text = "Connecting Server";
        //キャラ選択画面のキャラ画像を一度全部非表示にする
        for (int index = 0; index < charaImages.Length; index++)
        {
            charaImages[index].sprite = null;
            charaImages[index].gameObject.SetActive(false);
        }
        //キャラ選択画面の選択ボタンを初期化する
        charaSelectButtons = GameObject.FindGameObjectsWithTag("SelectButton");
        //Debug.Log(charaSelectButtons.Length);
        for (int index = 0; index < charaSelectButtons.Length; index++)
        {
            //Debug.Log(charaSelectButtons[index].name);
            charaSelectButtons[index] = panelSelectChara.transform.GetChild(index).gameObject;
            charaSelectButtons[index].SetActive(false);
        }
        panelSelectChara.SetActive(false);
        //ルーム作成画面の初期設定
        roomNameField.text = "BattleField";
        passWordField = null;
        roomButtons = GameObject.FindGameObjectsWithTag("RoomButton");
        //if (roomButtons == null || roomButtons.Length == 0) return;
        roomNames = new List<string>();
        //ルーム入室ボタンを非表示にする
        for(int index = 0; index < roomButtons.Length; index++)
        {
            roomButtons[index].SetActive(false);
        }
        isInRoom = false;
        isMax = false;
        isInCharaSelect = false;
        
    }
	
	// Update is called once per frame
	void Update () {
        //pingを常に取得し、値に応じて表記を変更
        ping = PhotonNetwork.GetPing();
        pingText.color = (ping >= 100) ? Color.red : Color.black;
        pingText.text = "Ping : "+ ping;
        if (isInRoom)
        {
            roomNameText.text = PhotonNetwork.room.Name + "\n" + PhotonNetwork.room.PlayerCount + " / " + PhotonNetwork.room.MaxPlayers;
            if (PhotonNetwork.room.PlayerCount == PhotonNetwork.room.MaxPlayers)
            {
                isMax = true;
            }
        }

        //ルーム内の人数が最大人数になったら自動的にキャラクター選択画面を表示
        

        if (isMax && !isInCharaSelect)
        {
            isMax = false;
            isInCharaSelect = true;
            //キャラ選択画面を表示
            GoToSelectCharaMenu();
        }

    }

    #region ルーム関連
    //ルームにランダムに入室するボタンに割り当てる
    public void JoinRandomRoom()
    {
        RoomInfo[] rooms = PhotonNetwork.GetRoomList();
        //bool isAbleToJoin = false;
        //ルームが存在しなければ、作ってそこに入室
        if (rooms.Length == 0)
        {
            StartCoroutine(ShowMessage("ルームがありません。\n作成しました。"));
            RoomOptions room = new RoomOptions();
            room.MaxPlayers = 1;
            room.IsOpen = true;
            room.IsVisible = true;
            //room.
            PhotonNetwork.CreateRoom("Room" + photonView.ownerId, room, null);
        }//存在すれば、人数が満たされていないところにランダム入室
        else
        {
            roomNames.Clear();
            //roomNames = new List<string>();
            for (int index = 0; index < rooms.Length; index++)
            {
                if (rooms[index].PlayerCount < rooms[index].MaxPlayers)
                {
                    roomNames.Add(rooms[index].Name);
                }
            }
            PhotonNetwork.JoinRoom(roomNames[Random.Range(0, roomNames.Count)]);
            //PhotonNetwork.JoinRandomRoom();
        }
    }

    //ルーム入室ボタンに割り当てる
    public void JoinRoom(int roomNumber)
    {
        PhotonNetwork.JoinRoom(roomNames[roomNumber]);
    }

    //ルームリストが更新されたときに呼ばれる
    void OnReceivedRoomListUpdate()
    {
        ShowRooms();
    }

    //ロビーに入った時に呼ばれる
    void OnJoinedLobby()
    {
        messageText.text = "";
        //Debug.Log("ロビーに入りました");
        //messageText.text = "ロビーに入りました";
        //StartCoroutine(ShowMessage("ロビーに入りました。"));
        ShowRooms();
        id = photonView.ownerId - 1;

    }

    void ShowRooms()
    {
        //roomNameText.text = ""
        roomNames.Clear();
        //int count = roomContentField.transform.GetChildCount();
        //if (count != 0)
        //{
        //    for (int index = 0; index < count; index++)
        //    {
        //        //Destroy(roomContentField.transform.GetChild(index));

        //    }
        //}
        //存在しているルームの情報を取得
        RoomInfo[] rooms = PhotonNetwork.GetRoomList();
        if (rooms == null || rooms.Length == 0) return;
        int buttonIndex = 0;
        //Vector2 spawnPos = buttonSpawnOrigin.transform.localPosition;
        //ルーム一覧を表示
        for (int index = 0; index < rooms.Length; index++)
        {
            //一覧表示をオンにしている場合のみ表示
            if (rooms[index].IsVisible == false) continue;
            if (rooms[index].PlayerCount < rooms[index].MaxPlayers)
            {
                roomNames.Add(rooms[index].Name);
                roomButtons[buttonIndex].SetActive(true);
                roomButtons[buttonIndex].transform.GetChild(0).GetComponent<Text>().text = roomNames[roomNames.Count - 1];
                buttonIndex++;
            }
            //RoomInfo roomInfo = rooms[index];

        }
        //Debug.Log(roomNames.Count);
    }

    //ルームに入室すると呼ばれる
    void OnJoinedRoom()
    {
        //Debug.Log(photonView.ownerId);
        Debug.Log("ルームに入室しました。");
        //StartCoroutine("ルームに入室しました。");
        //パネルの表示・非表示の切り替え
        panelRoomMenu.SetActive(true);
        panelTopMenu.SetActive(false);
        panelRoomCreate.SetActive(false);
        panelSelectChara.SetActive(false);
        //ルームの情報表示
        roomNameText.text = PhotonNetwork.room.Name + "\n" + PhotonNetwork.room.PlayerCount + " / " + PhotonNetwork.room.MaxPlayers;
        //キャラの選択状態の初期化
        charaIDSelected = new int[PhotonNetwork.room.MaxPlayers];
        for(int index = 0;index < charaIDSelected.Length; index++)
        {
            charaIDSelected[index] = 0;
        }

        for(int index = 0;index < charaImages.Length; index++)
        {
            charaImages[index].sprite = charaSprites[0];
        }
        isInRoom = true;
        
    }

    //ルーム名入力欄に割り当てる
    public void SetRoomName()
    {
        roomName = (roomNameField.text == "") ? roomNameField.placeholder.GetComponent<Text>().text : roomNameField.text;
        //Debug.Log(roomName);
        //roomName = roomNameField.text;
    }

    //パスワード入力欄に割り当てる
    public void SetPassWord()
    {
        passWord = (passWordField.text == "") ? passWordField.placeholder.GetComponent<Text>().text : passWordField.text;
        passWord = passWordField.text;
    }

    //最大プレイ人数入力欄に割り当てる
    public void SetMaxPlayer()
    {
        //Debug.Log(maxPlayerField.text);
        string max = (maxPlayerField.text == "") ? maxPlayerField.placeholder.GetComponent<Text>().text : maxPlayerField.text;
        //Debug.Log(max);
        maxPlayer = int.Parse(max);
    }

    //IsOpenチェックボックスに割り当てる
    public void SetRoomIsOpen()
    {
        roomIsOpen = isOpenToggle.isOn;
    }

    //IsVisibleチェックボックスに割り当てる
    public void SetRoomIsVisible()
    {
        roomIsVisible = isVisibleToggle.isOn;
    }


    //ルーム作成ボタンに割り当てる
    public void CreateRoom()
    {
        //ルームの情報をInput
        if (roomName == null) SetRoomName();
        if (maxPlayer == 0) SetMaxPlayer();
        SetRoomIsOpen();
        SetRoomIsVisible();
        //if (passWord == null) SetPassWord();
        //ルームの設定を行って作成
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)maxPlayer;
        roomOptions.IsOpen = roomIsOpen;
        roomOptions.IsVisible = roomIsVisible;
        //maxPlayer = byte.Parse(maxPlayerField.text);
        PhotonNetwork.CreateRoom(roomName, roomOptions, null);
        //Debug.Log(PhotonNetwork.GetRoomList());
        //PhotonNetwork.JoinRoom(roomName);
        //Debug.Log(PhotonNetwork.inRoom);
    }

    //ルーム作成ボタンを押したときに呼ぶ
    public void GoToCreateMenu()
    {
        panelRoomCreate.SetActive(true);
        panelTopMenu.SetActive(false);
        panelRoomMenu.SetActive(false);
        panelSelectChara.SetActive(false);
    }

    //キャラ選択画面に移行する
    public void GoToSelectCharaMenu()
    {
        panelSelectChara.SetActive(true);
        panelRoomCreate.SetActive(false);
        panelRoomMenu.SetActive(false);
        panelTopMenu.SetActive(false);
        //キャラ選択状態の初期化
        //Debug.Log(PhotonNetwork.room.MaxPlayers);
        for(int index = 0;index < PhotonNetwork.room.PlayerCount; index++)
        {
            charaImages[index].gameObject.SetActive(true);
            charaImages[index].sprite = charaSprites[0];
        }

        for(int index = 0;index < charaSprites.Length; index++)
        {
            charaSelectButtons[index].SetActive(true);
            charaSelectButtons[index].GetComponent<Image>().sprite = charaSprites[index];
        }

        //マスタークライアントのみ、ゲーム開始ボタンを表示する
        //必要によっては、時間経過で自動的にゲームに映るようにするつもり
        gameButton.SetActive((PhotonNetwork.isMasterClient) ? true : false);
    }

    //Photonサーバーから切断されたときに呼ばれる
    void OnDisconnectedFromPhoton()
    {

        Debug.Log("Photonサーバーから切断されました。");
        //StartCoroutine(ShowMessage("Photonサーバーから切断されました。"));
        //messageText.text = "";
        //Debug.Log("Photonサーバーから切断されました。");
    }

    //マスタークライアントが変更されたときに呼ばれる
    void OnMasterClientSwitched()
    {
        //Debug.Log("マスタークライアントが変わりました。");
        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("マスタークライアントが変わりました。\nあなたがマスタークライアントです。");
            //StartCoroutine(ShowMessage("マスタークライアントが変わりました。\nあなたがマスタークライアントです。"));
        }
        else
        {
            Debug.Log("マスタークライアントが変わりました。");
            //StartCoroutine(ShowMessage("マスタークライアントが変わりました。"));
        }

        //if (PhotonNetwork.isMasterClient)
        //    Debug.Log("あなたがマスタークライアントです。");
    }

    public void GotoGame()
    {
        for(int index = 0;index < PhotonNetwork.room.PlayerCount;index++)
        {
            PlayerPrefs.SetInt("SelectChara" + index, charaIDSelected[index]);
        }
        //PlayerPrefs.SetInt("SelectChara" + photonView.ownerId, charaIDSelected[photonView.ownerId]);


        photonView.RPC("LoadScene", PhotonTargets.All, "test_lol");
    }

    //シーン遷移関数
    [PunRPC]
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    #endregion



#region キャラクター選択画面関連

    //引数にキャラのIDを入れる
    public void SelectChara(int charaNumber)
    {
        //Debug.Log(charaIDSelected.Length);
        //引数に指定されたキャラIDをPrefsで保存
        PlayerPrefs.SetInt("SelectChara" + photonView.ownerId, charaNumber);
        //IDに応じてキャラの画像を切り替える
        //Debug.Log(photonView.ownerId);
        charaIDSelected[photonView.ownerId] = charaNumber;
        charaImages[photonView.ownerId].sprite = charaSprites[charaNumber];
    }

#endregion



    //



    //引数に書かれた文字列を表示させるだけ
    IEnumerator ShowMessage(string info)
    {
        messageText.text = info;
        yield return new WaitForSeconds(0.5f);
        messageText.text = "";
        yield break;
    }


#region 実験用関数
    public void InputFieldTest(Text text)
    {
        //Debug.Log(text.text);
    }

    //実験用関数
    void CreateRoomTest()
    {
        for(int index = 0;index < 3; index++)
        {
            PhotonNetwork.CreateRoom("room");
            //PhotonNetwork.LeaveRoom();
        }
    }

    void Test()
    {
        RoomInfo[] rooms = PhotonNetwork.GetRoomList();
        Debug.Log("ルーム数　:" + rooms.Length);

        for(int index = 0; index < rooms.Length; index++)
        {
            Debug.Log(rooms[index].Name);
        }
    }
#endregion


    //同期関数
    void OnPhotonSerializeView(PhotonStream stream , PhotonMessageInfo info)
    {
        if (!isInRoom) return;

        if (stream.isWriting)
        {

            //選択した本人のownerID - 1と選択したキャラのIDを送信
            id = photonView.ownerId - 1;
            //stream.SendNext(id);
            stream.SendNext(charaIDSelected[id]);
        }
        else
        {
            //送られてきた数値で、自動的にアイコンを変える
            id = (int)stream.ReceiveNext();
            charaIDSelected[id] = (int)stream.ReceiveNext();
            charaImages[id].sprite = charaSprites[charaIDSelected[id]];
        }
    }

}
