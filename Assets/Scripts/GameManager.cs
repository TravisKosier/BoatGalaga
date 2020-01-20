using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    static int level = 1; //Current level
    static int score;
    static int lives = 3;

    int enemyAmount;

    int scoreToBonusLife = 10000;

    static int bonusScore;
    static bool hasLost;

    //Score parameters - edit?
    /*
     * Flies - 80 moving, 50 formation
     * Wasps - 160 moving, 80 formation
     * Bosses - 400 moving, 100 formation
     */

    void Awake()
    {
        instance = this; //ensure there is only one, static GameManager

        if (hasLost) //Reset on loss
        {
            level = 1;
            score = 0;
            lives = 3;
            bonusScore = 0;
            hasLost = false;
        }
    }

    void Start()
    {
        UiScript.instance.UpdateScoreText(score); //Set initial vals
        UiScript.instance.UpdateLivesText(lives);
        UiScript.instance.ShowStageText(level);
    }

    public void AddScore(int amount)
    {
        score += amount;

        UiScript.instance.UpdateScoreText(score); //Update UI Value

        bonusScore += amount;
        if (bonusScore>=scoreToBonusLife)
        {
            lives++;
            bonusScore %= scoreToBonusLife;
        }
    }

    public void DecreaseLives()
    {
        lives--;

        UiScript.instance.UpdateLivesText(lives); //Update UI Value

        if (lives < 0)
        {
            //Game over
            ScoreHolder.level = level;
            ScoreHolder.score = score;
            hasLost = true;
            SceneManager.LoadScene("GameOver");
        }
    }

    public void WinCondition()
    {
        level++;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
