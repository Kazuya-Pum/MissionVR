using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RequireScenes : MonoBehaviour
{

    private void Awake()
    {
        if ( !SceneManager.GetSceneByName( "Common" ).isLoaded )
        {
            SceneManager.LoadSceneAsync( "Common", LoadSceneMode.Additive );
        }
    }
}
