using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class UiScript : MonoBehaviour
{
    public static UiScript instance; //Singular static instance

    public Text scoreText;
    public Text livesText;
    public Text stageText;

    private void Awake()
    {
        instance = this;
    }

    public void UpdateScoreText(int amount)
    {
        scoreText.text = amount.ToString("D9"); //Autofills with zeroes up to 9
    }

    public void UpdateLivesText(int amount)
    {
        livesText.text = "x" + amount.ToString("D2"); //Autofills with zeroes up to 2
    }

    public void ShowStageText(int amount)
    {
        scoreText.gameObject.SetActive(true);
        stageText.text = "Stage " + amount; //Autofills with zeroes

        Invoke("DeactivateStageText", 3f); //Show for x seconds
    }

    void DeactivateStageText() //hide scoretext when not a new round
    {
        stageText.gameObject.SetActive(false);
        CancelInvoke("DeactivateStageText");
    }
}
