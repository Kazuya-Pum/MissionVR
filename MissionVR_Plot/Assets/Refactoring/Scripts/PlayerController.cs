using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Photon.MonoBehaviour
{
    public static PlayerController instance;

    public PlayerBase player;

    // TODO 設定ファイル等に移設
    public float sensitivity;

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
    }

    void Update()
    {
        // TODO ゲームの状態も今後考慮する
        if ( player )
        {
            GetKey();
        }
    }

    // TODO InputManagerからキーを設定し、そちらを使用
    private void GetKey()
    {
        float x = Input.GetAxis( "Horizontal" ) * Time.deltaTime;
        float z = Input.GetAxis( "Vertical" ) * Time.deltaTime;

        if ( x != 0 || z != 0 )
        {
            player.photonView.RPC( "Move", PhotonTargets.All, x, z );
        }

        float mouse_x = Input.GetAxis( "Mouse X" ) * Time.deltaTime;
        float mouse_y = Input.GetAxis( "Mouse Y" ) * Time.deltaTime;

        if ( mouse_x != 0 || mouse_y != 0 )
        {
            player.photonView.RPC( "Rotate", PhotonTargets.All, mouse_x, mouse_y );
        }

        if ( Input.GetKeyDown( KeyCode.LeftShift ) )
        {
            player.photonView.RPC( "SetDashFlag", PhotonTargets.All, true );
        }
        else if ( Input.GetKeyUp( KeyCode.LeftShift ) )
        {
            player.photonView.RPC( "SetDashFlag", PhotonTargets.All, false );
        }

        if ( Input.GetKeyDown( KeyCode.Mouse0 ) )
        {
            player.photonView.RPC( "PullTheTrigger", PhotonTargets.AllViaServer, true );
        }
        else if ( Input.GetKeyUp( KeyCode.Mouse0 ) )
        {
            player.photonView.RPC( "PullTheTrigger", PhotonTargets.AllViaServer, false );
        }
    }
}
