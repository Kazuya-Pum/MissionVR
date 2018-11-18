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

    public Camera miniMapCamera;
    private Transform tfMiniMapCamera;
    private float miniMapPosY;
    [SerializeField] private RawImage miniMapImage;

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
                tfMiniMapCamera.position = new Vector3( player.tfCache.position.x, miniMapPosY, player.tfCache.position.z );
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
            player.photonView.RPC( "Move", PhotonTargets.AllViaServer, Vector3.Normalize( player.tfCache.forward * z + player.tfCache.right * x ) );
        }

        float mouse_x = Input.GetAxis( "Mouse X" ) * Time.deltaTime;
        float mouse_y = Input.GetAxis( "Mouse Y" ) * Time.deltaTime;

        if ( mouse_x != 0 || mouse_y != 0 )
        {
            player.photonView.RPC( "Rotate", PhotonTargets.AllViaServer, mouse_x, mouse_y );
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

                miniMapCamera.orthographicSize = 180;
                miniMapImage.rectTransform.anchoredPosition = new Vector3( -400, -250, 0 );
                miniMapImage.rectTransform.sizeDelta = new Vector2( 450, 450 );

                tfMiniMapCamera.position = new Vector3( 0, miniMapPosY, 0 );
            }
            else if ( PlayerState == PlayerState.BIGMAP )
            {
                PlayerState = PlayerState.PLAY;

                miniMapCamera.orthographicSize = 50;
                miniMapImage.rectTransform.anchoredPosition = new Vector3( -60, -60, 0 );
                miniMapImage.rectTransform.sizeDelta = new Vector2( 100, 100 );
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
