using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Refactoring
{
    public enum PlayerState { WAIT, PLAY, SHOP }

    public enum GameState { GAME, WAIT }

    public class GameManager : Photon.MonoBehaviour
    {
        public static GameManager instance;
        [SerializeField] private DataBaseFormat dataBase;
        public Transform[] spawnPoint;
        [SerializeField] private float setRespawnTime;
        public WaitForSeconds respawnTime;
        public GameState gameState;

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

        void OnJoinedLobby()
        {
            Debug.Log( "joined lobby" );
            PhotonNetwork.JoinOrCreateRoom( "Test", null, null );
        }

        void OnJoinedRoom()
        {
            Debug.Log( "joined room : " + PhotonNetwork.room.Name + ", isMasterClient : " + PhotonNetwork.isMasterClient );


            InstantiatePlayer( ( PhotonNetwork.room.PlayerCount % 2 == 0 ) ? Team.WHITE : Team.BLACK );
        }

        private void InstantiatePlayer( Team team )
        {
            PlayerController.player = PhotonNetwork.Instantiate( "Player", spawnPoint[(int)team].position, spawnPoint[(int)team].rotation, 0 ).GetComponent<PlayerBase>();
            PlayerController.player.photonView.RPC( "FetchTeam", PhotonTargets.AllBuffered, team );
            PlayerController.player.photonView.RPC( "FetchSetting", PhotonTargets.AllBuffered, PlayerController.instance.sensitivity );

            PlayerController.playerCamera = PlayerController.player.head.Find( "Main Camera" ).transform;

            if ( gameState == GameState.GAME )
            {
                PlayerController.instance.playerState = PlayerState.PLAY;
            }

            EntityBase[] entities = FindObjectsOfType<EntityBase>();
            foreach ( EntityBase entity in entities )
            {
                entity.OnSetPlayer();
            }


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