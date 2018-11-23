using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerState : byte { WAIT, PLAY, BIGMAP, SHOP }

public class PlayerController : Photon.MonoBehaviour
{
    public static PlayerController instance;

    public PlayerBase player;

    public Transform playerCamera;

    private PlayerState playerState;

    private bool enable = false;

    private Vector3 vector;

    public Camera miniMapCamera;
    private Transform tfMiniMapCamera;
    private float miniMapPosY;
    [SerializeField] private RawImage miniMapImage;

    private Vector3 mapCameraPos;
    private Vector2 mapPos;
    private Vector2 mapSize;

    // TODO 設定ファイル等に移設
    public float sensitivity;

    [SerializeField] private Image hpBar;
    [SerializeField] private Image manaBar;
    [SerializeField] private Text moneyText;
    [SerializeField] private Text levelText;
    [SerializeField] private Image expValue;
    [SerializeField] private Image statusPhysicalAttack;
    [SerializeField] private Image statusPhysicalDiffence;
    [SerializeField] private Image statusMagicDiffence;
    [SerializeField] private Image statusSpeed;

    public PlayerState PlayerState
    {
        get
        {
            return playerState;
        }

        set
        {
            playerState = value;
            OnCheck();
        }
    }

    private void Awake()
    {
        if ( instance == null )
        {
            instance = this;
        }
        else
        {
            Destroy( gameObject );
        }

        sensitivity = ( sensitivity <= 0 ) ? 100f : sensitivity;

        tfMiniMapCamera = miniMapCamera.transform;
        miniMapPosY = tfMiniMapCamera.position.y;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Start()
    {
        GameManager.instance.onSetPlayer += OnSetPlayer;
    }

    private void OnSetPlayer()
    {
        OnStatusChanged();
        OnCheck();
    }

    public void OnCheck()
    {
        if ( ( PlayerState == PlayerState.PLAY || PlayerState == PlayerState.BIGMAP ) && player && player.entityState == EntityState.ALIVE )
        {
            enable = true;
        }
        else
        {
            enable = false;
        }
    }

    void Update()
    {
        if ( enable )
        {
            OnChange_HP_MANA();
            GetKey();

            if ( PlayerState != PlayerState.BIGMAP )
            {
                mapCameraPos.Set( player.tfCache.position.x, miniMapPosY, player.tfCache.position.z );
                tfMiniMapCamera.position = mapCameraPos;
            }
        }

    }

    // TODO InputManagerからキーを設定し、そちらを使用
    //
    // 操作性重視　PhotonTargets.All : プレイヤーは入力した直後に動き出すが他のクライアントでは少しラグがあるため、位置を同期した際少しワープする
    //      ↕
    // ロールバック対策　PhotonTargets.AllViaServer : 全員が同じタイミングで動き出すが、プレイヤーにとっては入力してから少し動き出しにラグが生まれる
    private void GetKey()
    {
        float x = Input.GetAxis( "Horizontal" ) * Time.deltaTime;
        float z = Input.GetAxis( "Vertical" ) * Time.deltaTime;

        if ( x != 0 || z != 0 )
        {
            vector = Vector3.Normalize( player.tfCache.forward * z + player.tfCache.right * x );
            player.photonView.RPC( "Move", PhotonTargets.All, vector );
        }
        else if ( vector.magnitude > 0 )
        {
            player.photonView.RPC( "Move", PhotonTargets.All, Vector3.zero );
        }

        float mouse_x = Input.GetAxis( "Mouse X" ) * Time.deltaTime;
        float mouse_y = Input.GetAxis( "Mouse Y" ) * Time.deltaTime;

        if ( mouse_x != 0 || mouse_y != 0 )
        {
            player.photonView.RPC( "Rotate", PhotonTargets.All, mouse_x, mouse_y );
        }

        if ( Input.GetKeyDown( KeyCode.LeftShift ) )
        {
            player.photonView.RPC( "SetDashFlag", PhotonTargets.AllViaServer, true );
        }
        else if ( Input.GetKeyUp( KeyCode.LeftShift ) )
        {
            player.photonView.RPC( "SetDashFlag", PhotonTargets.AllViaServer, false );
        }

        if ( Input.GetKeyDown( KeyCode.Mouse0 ) )
        {
            player.trigger = true;
        }
        else if ( Input.GetKeyUp( KeyCode.Mouse0 ) )
        {
            player.trigger = false;
        }

        if ( Input.GetKeyDown( KeyCode.M ) )
        {
            if ( PlayerState == PlayerState.PLAY )
            {
                PlayerState = PlayerState.BIGMAP;

                mapCameraPos.Set( 0, miniMapPosY, 0 );
                mapPos.Set( -400, -250 );
                mapSize.Set( 450, 450 );

                miniMapCamera.orthographicSize = 180;
                miniMapImage.rectTransform.anchoredPosition = mapPos;
                miniMapImage.rectTransform.sizeDelta = mapSize;

                tfMiniMapCamera.position = mapCameraPos;
            }
            else if ( PlayerState == PlayerState.BIGMAP )
            {
                PlayerState = PlayerState.PLAY;

                mapPos.Set( -60, -60 );
                mapSize.Set( 100, 100 );

                miniMapCamera.orthographicSize = 50;
                miniMapImage.rectTransform.anchoredPosition = mapPos;
                miniMapImage.rectTransform.sizeDelta = mapSize;
            }
        }
    }

    public void OnStatusChanged()
    {
        OnGetReward();

        levelText.text = ( "Lv." + player.Level );
        statusPhysicalAttack.fillAmount = player.PhysicalAttack * 0.1f;
        statusPhysicalDiffence.fillAmount = player.PhysicalDefense * 0.1f;
        statusMagicDiffence.fillAmount = player.MagicDefense * 0.1f;
        statusSpeed.fillAmount = player.MoveSpeed * 0.1f;
    }

    public void OnChange_HP_MANA()
    {
        float hp = (float)player.Hp / player.MaxHp;
        float mana = (float)player.Mana / player.MaxMana;

        if ( !Mathf.Approximately( hpBar.fillAmount, hp ) )
        {
            hpBar.fillAmount = Mathf.Lerp( hpBar.fillAmount, hp, 0.25f );
        }
        if ( !Mathf.Approximately( manaBar.fillAmount, mana ) )
        {
            manaBar.fillAmount = Mathf.Lerp( manaBar.fillAmount, mana, 0.25f );
        }
    }

    public void OnGetReward()
    {
        moneyText.text = player.MyMoney.ToString();
        expValue.fillAmount = (float)player.MyExp / player.Level / 100;
    }
}
