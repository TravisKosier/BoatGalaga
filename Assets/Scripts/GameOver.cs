using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public Text highscoreText, highestlevelText;
    
    void Start()
    {
        highscoreText.text = "Highscore: " + ScoreHolder.score;
        highestlevelText.text = "Highest Level: " + ScoreHolder.level;
    }

     void Update()
    {
        if (Input.GetMouseButton(0))
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
