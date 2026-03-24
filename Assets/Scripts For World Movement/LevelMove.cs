using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMoveCave : MonoBehaviour
{
    [SerializeField] string sceneBuildScene;
    //should change this script later to use don'tdestroyonload to save player data

    void OnTriggerEnter2D(Collider2D other) 
    {
        Debug.Log(other.name);

        if(other.tag == ("Player"))
        {
            //player entered, so move level
            print("Switching Scene to " + sceneBuildScene);
            //we only want to load one scene at a time so we use LoadSceneMode.Single argument, should be more efficient 
            //then many loaded at the same time
            //SceneManager.LoadScene(sceneBuildScene, LoadSceneMode.Single);
            SceneChanger.Instance.LoadScene(sceneBuildScene);

        }
    }
}
