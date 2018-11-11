﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameState { WAIT, COUNT_DOWN, GAME }

public enum AnounceType { LEVEL, PlAYER_DEATH, DESTROY }

public class GameManager : Photon.MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private byte maxPlayers;

    [SerializeField] private DataBaseFormat dataBase;

    [HideInInspector] public Transform[] spawnPoint;
    [SerializeField] private float setRespawnTime;
    public WaitForSeconds respawnTime;

    public GameState gameState;

    [SerializeField] private Text countDownText;
    [SerializeField] private int countDownTime;

    public delegate void OnSetPlayer();
    public OnSetPlayer onSetPlayer;

    public delegate void OnGameStart();
    public OnGameStart onGameStart;

    public DataBaseFormat DataBase
    {
        get
        {
            return dataBase;
        }
    }

    public Queue<BulletBase> bullets = new Queue<BulletBase>();

    [SerializeField] private Text anounceText;
    [SerializeField] private float setAnounceSpeed;
    private WaitForSeconds anounceSpeed;
    private Queue<string> anounceTask = new Queue<string>();

    public Transform[] projectorPos;

    void Awake()
    {
        if ( instance == null )
        {
            instance = this;
        }
        else
        {
            Destroy( gameObject );
        }

        //PhotonNetwork.offlineMode = true;

        PhotonNetwork.ConnectUsingSettings( "v1.0" );

        respawnTime = new WaitForSeconds( setRespawnTime );
        anounceSpeed = new WaitForSeconds( setAnounceSpeed );
    }

    private void Start()
    {
        spawnPoint[0] = GameObject.Find( "WhitePlayerSpawnPoint" ).transform;
        spawnPoint[1] = GameObject.Find( "BlackPlayerSpawnPoint" ).transform;
    }

    private void OnDestroy()
    {
        PhotonNetwork.Disconnect();
    }

    void OnJoinedLobby()
    {
        Debug.Log( "joined lobby" );

        PhotonNetwork.JoinRandomRoom();
    }

    void OnPhotonRandomJoinFailed()
    {
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = maxPlayers
        };

        PhotonNetwork.CreateRoom( "TestRoom", roomOptions, null );
    }

    void OnJoinedRoom()
    {
        Debug.Log( "joined room : " + PhotonNetwork.room.Name + ", isMasterClient : " + PhotonNetwork.isMasterClient );


        InstantiatePlayer( ( PhotonNetwork.room.PlayerCount % 2 == 0 ) ? Team.WHITE : Team.BLACK );

        photonView.RPC( "CheckStart", PhotonTargets.MasterClient );

    }

    [PunRPC]
    protected void CheckStart()
    {
        if ( ( PhotonNetwork.room.PlayerCount == maxPlayers || ( maxPlayers == 0 && PhotonNetwork.room.PlayerCount == 8 ) ) && gameState == GameState.WAIT )
        {
            photonView.RPC( "GameStart", PhotonTargets.AllViaServer );
        }
    }

    [PunRPC]
    protected void GameStart()
    {
        gameState = GameState.COUNT_DOWN;
        StartCoroutine( "CountDown" );
    }

    private IEnumerator CountDown()
    {
        WaitForSeconds one = new WaitForSeconds( 1f );

        for ( int i = countDownTime; i > 0; i-- )
        {
            countDownText.text = i.ToString();
            yield return one;
        }

        if ( PhotonNetwork.isMasterClient )
        {
            // TODO Masterがゲームから抜けるとバッファが破棄されるため要対策
            photonView.RPC( "FetchGameState", PhotonTargets.AllBufferedViaServer );
            onGameStart();
        }
    }

    [PunRPC]
    protected void FetchGameState()
    {
        gameState = GameState.GAME;
        countDownText.transform.parent.gameObject.SetActive( false );
        if ( PlayerController.instance.player )
        {
            PlayerController.instance.PlayerState = PlayerState.PLAY;
        }
    }

    private void InstantiatePlayer( Team team )
    {
        Vector3 shiftedPosition = spawnPoint[(int)team].position;
        shiftedPosition.x += Random.Range( -5, 10 );
        shiftedPosition.z += Random.Range( -5, 10 );

        PlayerController.instance.player = PhotonNetwork.Instantiate( "CapsulePlayer", shiftedPosition, spawnPoint[(int)team].rotation, 0 ).GetComponent<PlayerBase>();
        PlayerController.instance.player.photonView.RPC( "FetchTeam", PhotonTargets.AllBuffered, team );
        PlayerController.instance.player.photonView.RPC( "FetchSetting", PhotonTargets.AllBuffered, PlayerController.instance.sensitivity );

        PlayerController.instance.playerCamera = PlayerController.instance.player.head.Find( "Main Camera" ).transform;

        if ( gameState == GameState.GAME )
        {
            PlayerController.instance.PlayerState = PlayerState.PLAY;
        }

        onSetPlayer();
    }

    /// <summary>
    /// エンティティーを生成するメソッド
    /// </summary>
    /// <param name="index">生成するエンティティーのインデックス</param>
    /// <param name="point">生成地点のtransform</param>
    /// <param name="team">所属するチーム</param>
    [PunRPC]
    public EntityBase Summon( int index, Transform point, Team team )
    {
        // TODO プロジェクト完成時にこのチェックは外してもいいかも
        if ( PhotonNetwork.isMasterClient )
        {
            EntityBase entity;
            entity = PhotonNetwork.InstantiateSceneObject( DataBase.entityInfos[index].name, point.position, point.rotation, 0, null ).GetComponent<EntityBase>();
            entity.photonView.RPC( "FetchTeam", PhotonTargets.AllBuffered, team );
            return entity;
        }
        else
        {
            Debug.LogWarning( "SummonメソッドをMasterClient以外で実行しました。" );
            return null;
        }
    }

    [PunRPC]
    public void SetAnounce( AnounceType type, Team team = Team.WHITE )
    {
        string message;
        switch ( type )
        {
            case AnounceType.LEVEL:
                message = "レベルが上がりました";
                break;
            case AnounceType.PlAYER_DEATH:
                message = ( team == PlayerController.instance.player.team ) ? "味方が倒されました" : "敵を倒しました";
                break;
            case AnounceType.DESTROY:
                message = ( team == PlayerController.instance.player.team ) ? "味方のタワーが破壊されました" : "敵のタワーを破壊しました";
                break;
            default:
                message = "error";
                break;
        }

        anounceTask.Enqueue( message );

        if ( anounceText.text == "" )
        {
            StartCoroutine( "OnAnounce" );
        }
    }

    private IEnumerator OnAnounce()
    {
        while ( anounceTask.Count > 0 )
        {
            anounceText.text = anounceTask.Dequeue();
            yield return anounceSpeed;
        }
        anounceText.text = null;
    }

    [PunRPC]
    protected void ToResult( Team loser )
    {
        if ( PlayerController.instance.player.team == loser )
        {

        }
        else
        {
            SceneManager.LoadSceneAsync( "Result" );
        }
    }
}

[System.Serializable]
public class DataBaseFormat
{
    public Color allyColor;
    public Color enemyColor;

    public GunInfo[] gunInfos;

    public GameObject[] entityInfos;
}

[System.Serializable]
public class GunInfo
{
    public string name;
    public GameObject bullet;
    public float fireRate;
    public float range;
    //public GameObject model;
}