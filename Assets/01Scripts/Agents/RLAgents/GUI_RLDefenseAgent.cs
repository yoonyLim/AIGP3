using UnityEngine;

public class GUI_RLDefenseAgent : MonoBehaviour
{
    [SerializeField] private RLDefensiveAagent _RLDefenseAgent;
    
    private GUIStyle _defaultStyle = new GUIStyle();
    private GUIStyle _positiveStyle = new GUIStyle();
    private GUIStyle _negativeStyle = new GUIStyle();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _defaultStyle.fontSize = 20;
        _defaultStyle.normal.textColor = Color.yellow;
        
        _positiveStyle.fontSize = 20;
        _positiveStyle.normal.textColor = Color.green;
        
        _negativeStyle.fontSize = 20;
        _negativeStyle.normal.textColor = Color.red;
    }

    private void OnGUI()
    {
        string debugEpisode = "Episode: " + _RLDefenseAgent.CurrentEpisode + " - Step: " + _RLDefenseAgent.StepCount;
        string debugReward = "Reward: " + _RLDefenseAgent.CumulativeReward;
        
        GUIStyle rewardStyle = _RLDefenseAgent.CumulativeReward >= 0 ? _positiveStyle : _negativeStyle;
        
        GUI.Label(new Rect(1000, 20, 500, 30), debugEpisode, _defaultStyle);
        GUI.Label(new Rect(1000, 60, 500, 30), debugReward, rewardStyle);
    }
}
