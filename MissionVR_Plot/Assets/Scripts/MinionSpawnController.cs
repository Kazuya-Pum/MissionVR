using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MinionSpawnController : Photon.MonoBehaviour
{
    public static MinionSpawnController instance;

    [SerializeField] private Transform spawnPoints;
    [SerializeField] private float spawnIntervalTime;
    [SerializeField] private float waveIntervalTime;
    [SerializeField] private int waveSpawnLimit;
    private WaitForSeconds spawnInterval;
    private WaitForSeconds waveInterval;

    public Transform[] rootTopPoints;
    public Transform[] rootBotPoints;
    public Transform whileProjector;
    public Transform blackProjector;

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

        spawnInterval = new WaitForSeconds( spawnIntervalTime );
        waveInterval = new WaitForSeconds( waveIntervalTime );
    }

    private void Start()
    {
        GameManager.instance.onGameStart += OnGameStart;
    }

    public void OnGameStart()
    {
        if ( PhotonNetwork.isMasterClient )
        {
            StartCoroutine( "MinionSpawning" );
        }
    }

    private IEnumerator MinionSpawning()
    {
        int spawnCount = 0;
        while ( true )
        {
            Team team = 0;
            foreach ( Transform teamPoint in spawnPoints )
            {
                switch ( teamPoint.name )
                {
                    case "WHITE":
                        team = Team.WHITE;
                        break;
                    case "BLACK":
                        team = Team.BLACK;
                        break;
                    default:
                        Debug.LogWarning( "spawnPointsに指定したオブジェクトの名前が不正です。\n" );
                        break;
                }

                int pos = 0;
                foreach ( Transform point in teamPoint )
                {
                    MinionAI minionAI = GameManager.instance.Summon( 0, point, team ).GetComponent<MinionAI>();
                    minionAI.photonView.RPC( "SetValue", PhotonTargets.All, (MinionLane)pos, point.position, team );

                    pos++;
                }
            }

            spawnCount++;
            if ( spawnCount < waveSpawnLimit )
            {
                yield return spawnInterval;
            }
            else
            {
                spawnCount = 0;
                yield return waveInterval;
            }
        }
    }
}

[System.Serializable]
public class RootPoint
{
    public Transform[] point;
}
