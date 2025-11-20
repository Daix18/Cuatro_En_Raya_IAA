using UnityEngine;
using UnityEngine.SceneManagement;
using static GameManager;

public class LoadMainGame : MonoBehaviour
{

    public void LoadMainGameScene(int level)
    {
        SceneManager.LoadScene(level);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
