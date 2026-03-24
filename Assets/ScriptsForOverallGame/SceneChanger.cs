using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//https://discussions.unity.com/t/how-can-i-open-previous-scene/737474/5

public class SceneChanger: MonoBehaviour
{
    private List<string> sceneHistory = new List<string>();
    public static SceneChanger Instance { get; private set; }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        sceneHistory.Add(SceneManager.GetActiveScene().name);
    }

    void Awake()
    {
        if(Instance !=null && Instance !=this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    public void LoadScene(string newScene)
    {
        if(sceneHistory.Count == 0 || sceneHistory[sceneHistory.Count - 1] != newScene)
        {
            sceneHistory.Add(newScene);
        }
        
        SceneManager.LoadScene(newScene, LoadSceneMode.Single);

    }

    public bool PreviousScene()
    {
        bool returnValue = false;
        if(sceneHistory.Count >= 2)
        {
            returnValue = true;
            sceneHistory.RemoveAt(sceneHistory.Count -1);
            SceneManager.LoadScene(sceneHistory[sceneHistory.Count -1]);
        }

        return returnValue;
    }
}
