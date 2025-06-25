using UnityEngine;
using TMPro;

public class LapTimer : MonoBehaviour
{
    public TextMeshProUGUI currentLapText;
    public TextMeshProUGUI lastLapText;
    public TextMeshProUGUI bestLapText;
    public TextMeshProUGUI lapImprovementText;
    
    private CarAgent agent;

    private float lapTime = 0f;
    private float bestLapTime = Mathf.Infinity;
    private float lapImprovement = 0f;
    private float lastLapTime = 0f;
    
    private bool isTiming = false;

    private void Start()
    {
        agent = FindAnyObjectByType<CarAgent>();
    }

    void Update()
    {
        if (isTiming)
        {
            lapTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    private void UpdateTimerUI()
    {
        currentLapText.text = $"Current lap: {lapTime:F3}s";
        
        if (bestLapTime < Mathf.Infinity)
        {
            bestLapText.text = $"Best lap: {bestLapTime:F3}s";
            lastLapText.text = $"Last lap: {lastLapTime:F3}s";
        }
        else
        {
            bestLapText.text = "Best lap: -=-=-=-";
            lastLapText.text = $"Last lap: -=-=-=-";
        }
    }

    public void StartLap()
    {
        lapTime = 0f;
        lastLapText.text = "Last lap: -=-=-=-";
        isTiming = true;
    }
    public void EndLap()
    {
        isTiming = false;

        lastLapTime = lapTime;

        if(bestLapTime == Mathf.Infinity)
        {
            bestLapTime = lapTime;
        }
        else
        {
            if (lapTime < bestLapTime)
            {
                lapImprovement = bestLapTime - lapTime;
                bestLapTime = lapTime;
                lapImprovementText.text = $"<color=green>-{lapImprovement:F3}s";
            }
            
            else
            {
                lapImprovement = lapTime - bestLapTime;
                lapImprovementText.text = $"<color=red>+{lapImprovement:F3}s";

            }
        }
    StartLap();
    }

    public void ResetLaps()
    {
        lapTime = 0f;
        bestLapTime = Mathf.Infinity;
        isTiming = false;
        UpdateTimerUI();
    }
    public void NullifyLap()
    {
        lapTime = 0f;
        UpdateTimerUI();
    }

}
          