using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public enum GameState { WAIT, COUNT_DOWN, GAME }

public enum AnounceType { LEVEL, PlAYER_DEATH, DESTROY }

public class GameManager : Photon.MonoBehaviour, IPunObservable
{
    public static GameManager instance;

    [SerializeField] private byte maxPlayers;
    public int selectedPlayer = 0;

    [SerializeField] private DataBaseFormat dataBase;

    public Transform whiteSpawnPoint;
    public Transform blackSpawnPoint;
    [SerializeField] private float setRespawnTime;
    public WaitForSeconds respawnTime;

    private GameState gameState;

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

    public GameState GameState
    {
        get
        {
            return gameState;
        }

        set
        {
            gameState = value;
            if ( value == GameState.GAME )
            {
                ToStart();
            }
        }
    }

    public Queue<BulletBase> bullets = new Queue<BulletBase>();
    public HashSet<EntityBase>[] minions;

    [SerializeField] private Text anounceText;
    [SerializeField] private float setAnounceSpeed;
    private WaitForSeconds anounceSpeed;
    private Queue<string> anounceTask = new Queue<string>();

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

        minions = new HashSet<EntityBase>[DataBase.entityInfos.Length];
        for ( int i = 0; i < minions.Length; i++ )
        {
            minions[i] = new HashSet<EntityBase>();
        }
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
        if ( ( PhotonNetwork.room.PlayerCount == maxPlayers || ( maxPlayers == 0 && PhotonNetwork.room.PlayerCount == 8 ) ) && GameState == GameState.WAIT )
        {
            photonView.RPC( "GameStart", PhotonTargets.AllViaServer );
        }
    }

    [PunRPC]
    protected void GameStart()
    {
        GameState = GameState.COUNT_DOWN;
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
            photonView.RPC( "FetchGameState", PhotonTargets.AllViaServer );
            onGameStart();
        }
    }

    [PunRPC]
    protected void FetchGameState()
    {
        GameState = GameState.GAME;
    }

    private void ToStart()
    {
        countDownText.transform.parent.gameObject.SetActive( false );
        if ( PlayerController.instance.player )
        {
            PlayerController.instance.PlayerState = PlayerState.PLAY;
        }
    }

    private void InstantiatePlayer( Team team )
    {
        Transform spawnPoint = GetSpawnPoint( team );

        Vector3 shiftedPosition = spawnPoint.position;
        shiftedPosition.x += Random.Range( -5, 10 );
        shiftedPosition.z += Random.Range( -5, 10 );

        PlayerController.instance.player = PhotonNetwork.Instantiate( dataBase.playerType[selectedPlayer], shiftedPosition, spawnPoint.rotation, 0 ).GetComponent<PlayerBase>();
        PlayerController.instance.player.localSensitivity = PlayerController.instance.sensitivity;
        PlayerController.instance.player.team = team;

        PlayerController.instance.playerCamera.parent = PlayerController.instance.player.head.Find( "CameraPos" );
        PlayerController.instance.playerCamera.localPosition = Vector3.zero;
        PlayerController.instance.playerCamera.localEulerAngles = Vector3.zero;

        if ( GameState == GameState.GAME )
        {
            ToStart();
        }

        onSetPlayer();
    }

    public Transform GetSpawnPoint( Team team )
    {
        Transform spawnPoint;
        if ( team == Team.WHITE )
        {
            spawnPoint = this.whiteSpawnPoint;
        }
        else
        {
            spawnPoint = this.blackSpawnPoint;
        }

        return spawnPoint;
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
            if ( minions[index].Any() )
            {
                entity = minions[index].First();
                minions[index].Remove( entity );
            }
            else
            {
                entity = PhotonNetwork.InstantiateSceneObject( DataBase.entityInfos[index].name, point.position, point.rotation, 0, null ).GetComponent<EntityBase>();
            }

            entity.photonView.RPC( "ToActiveSetting", PhotonTargets.All, team, point.position, point.rotation );
            entity.index = index;
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

    public void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
    {
        if ( stream.isWriting )
        {
            stream.SendNext( GameState );
        }
        else
        {
            GameState = (GameState)stream.ReceiveNext();
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

    public string[] playerType;
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