﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Refactoring
{
    public enum PlayerState { WAIT, PLAY, SHOP }

    public enum GameState { WAIT, COUNT_DOWN, GAME }

    public class GameManager : Photon.MonoBehaviour
    {
        public static GameManager instance;
        [SerializeField] private DataBaseFormat dataBase;
        public Transform[] spawnPoint;
        [SerializeField] private float setRespawnTime;
        public WaitForSeconds respawnTime;
        public GameState gameState;

        [SerializeField] private byte maxPlayers;

        [SerializeField] private Text countDownText;
        [SerializeField] private int countDownTime;

        public delegate void OnSetPlayer();
        public OnSetPlayer onSetPlayer;

        public DataBaseFormat DataBase
        {
            get
            {
                return dataBase;
            }
        }

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
                photonView.RPC( "FetchGameState", PhotonTargets.AllBufferedViaServer );
            }
        }

        [PunRPC]
        protected void FetchGameState()
        {
            this.gameState = GameState.GAME;
            PlayerController.instance.playerState = PlayerState.PLAY;
            countDownText.transform.parent.gameObject.SetActive( false );
        }

        private void InstantiatePlayer( Team team )
        {
            Vector3 shiftedPosition = spawnPoint[(int)team].position;
            shiftedPosition.x += Random.Range( -5, 10 );
            shiftedPosition.z += Random.Range( -5, 10 );

            PlayerController.instance.player = PhotonNetwork.Instantiate( "Player", shiftedPosition, spawnPoint[(int)team].rotation, 0 ).GetComponent<PlayerBase>();
            PlayerController.instance.player.photonView.RPC( "FetchTeam", PhotonTargets.AllBuffered, team );
            PlayerController.instance.player.photonView.RPC( "FetchSetting", PhotonTargets.AllBuffered, PlayerController.instance.sensitivity );

            PlayerController.instance.playerCamera = PlayerController.instance.player.head.Find( "Main Camera" ).transform;

            onSetPlayer();
        }


        [PunRPC]
        protected void Summon( int index, Team team )
        {
            MinionBase minion;
            minion = PhotonNetwork.InstantiateSceneObject( DataBase.entityInfos[index].name, Vector3.zero, Quaternion.identity, 0, null ).GetComponent<MinionBase>();
            minion.photonView.RPC( "FetchTeam", PhotonTargets.AllBuffered, team );
        }

        /// <summary>
        /// ミニオンの生成デバッグ用
        /// </summary>
        /// <param name="white">true: White, false: Black</param>
        public void TestSummon( bool white )
        {
            int index = 0;
            Team team = ( white ) ? Team.WHITE : Team.BLACK;
            photonView.RPC( "Summon", PhotonTargets.MasterClient, index, team );
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
}