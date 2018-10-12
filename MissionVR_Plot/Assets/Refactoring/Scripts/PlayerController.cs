using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Refactoring
{
    // TODO GameStateとの統合を検討
    public enum PlayerState { WAIT, PLAY, BIGMAP, SHOP }

    public class PlayerController : Photon.MonoBehaviour
    {
        public static PlayerController instance;

        public PlayerBase player;

        public Transform playerCamera;

        public PlayerState playerState;

        public Camera miniMapCamera;
        private Transform tfMiniMapCamera;
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
        }

        private void Start()
        {
            GameManager.instance.onSetPlayer += OnSetPlayer;
        }

        private void OnSetPlayer()
        {
            OnStatusChanged();
        }

        void Update()
        {
            if ( ( playerState == PlayerState.PLAY || playerState == PlayerState.BIGMAP ) && player && player.entityState == EntityState.ALIVE )
            {
                GetKey();
                OnChange_HP_MANA();

                if ( playerState != PlayerState.BIGMAP )
                {
                    tfMiniMapCamera.position = new Vector3( player.tfCache.position.x, tfMiniMapCamera.position.y, player.tfCache.position.z );
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
                player.photonView.RPC( "Move", PhotonTargets.AllViaServer, x, z );
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
                if ( playerState == PlayerState.PLAY )
                {
                    playerState = PlayerState.BIGMAP;

                    miniMapCamera.orthographicSize = 300;
                    miniMapImage.rectTransform.anchoredPosition = new Vector3( -400, -250, 0 );
                    miniMapImage.rectTransform.sizeDelta = new Vector2( 450, 450 );
                }
                else if ( playerState == PlayerState.BIGMAP )
                {
                    playerState = PlayerState.PLAY;

                    miniMapCamera.orthographicSize = 50;
                    miniMapImage.rectTransform.anchoredPosition = new Vector3( -60, -60, 0 );
                    miniMapImage.rectTransform.sizeDelta = new Vector2( 100, 100 );
                }
            }
        }

        public void OnStatusChanged()
        {
            OnChange_HP_MANA();
            OnGetReward();

            levelText.text = ( "Lv." + player.Level );
            statusPhysicalAttack.fillAmount = player.PhysicalAttack * 0.1f;
            statusPhysicalDiffence.fillAmount = player.PhysicalDefense * 0.1f;
            statusMagicDiffence.fillAmount = player.MagicDefense * 0.1f;
            statusSpeed.fillAmount = player.MoveSpeed * 0.1f;
        }

        // TODO 動きを滑らかにしたい
        public void OnChange_HP_MANA()
        {
            hpBar.fillAmount = (float)player.Hp / player.MaxHp;
            manaBar.fillAmount = (float)player.Mana / player.MaxMana;
        }

        public void OnGetReward()
        {
            moneyText.text = player.MyMoney.ToString();
            expValue.fillAmount = (float)player.MyExp / player.Level / 100;
        }
    }
}
