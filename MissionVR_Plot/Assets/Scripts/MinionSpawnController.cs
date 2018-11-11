using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MinionSpawnController : Photon.MonoBehaviour
{
    [SerializeField] private Transform spawnPoints;
    [SerializeField] private float spawnIntervalTime;
    [SerializeField] private float waveIntervalTime;
    [SerializeField] private int waveSpawnLimit;
    private WaitForSeconds spawnInterval;
    private WaitForSeconds waveInterval;

    [SerializeField] private Transform[] rootTopPoints;
    [SerializeField] private Transform[] rootBotPoints;

    private void Awake()
    {
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
            int teamNum = 0;
            foreach ( Transform teamPoint in spawnPoints )
            {
                int pos = 0;
                foreach ( Transform point in teamPoint )
                {
                    MinionAI minionAI = GameManager.instance.Summon( 0, point, (Team)teamNum ).GetComponent<MinionAI>();
                    minionAI.minionLane = (MinionLane)pos;
                    if ( pos == (int)MinionLane.TOP )
                    {
                        foreach ( Transform item in rootTopPoints )
                        {
                            if ( teamNum == (int)Team.WHITE )
                            {
                                minionAI.lanePoints.Add( item );
                            }
                            else
                            {
                                minionAI.lanePoints.Insert( 0, item );
                            }
                        }
                    }
                    else if ( pos == (int)MinionLane.BOT )
                    {
                        foreach ( Transform item in rootBotPoints )
                        {
                            if ( teamNum == (int)Team.WHITE )
                            {
                                minionAI.lanePoints.Add( item );
                            }
                            else
                            {
                                minionAI.lanePoints.Insert( 0, item );
                            }
                        }
                    }
                    minionAI.lanePoints.Add( GameManager.instance.projectorPos[teamNum] );
                    pos++;
                }
                teamNum++;
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
