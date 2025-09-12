using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    Text timerText;

    float currentTime = 0f;
    void Awake()
    {
        timerText = GetComponent<Text>();
    }
    void Update()
    {
        Timer();
    }
    void Timer()
    {
        currentTime += Time.deltaTime;
        int totalTime = Mathf.FloorToInt(currentTime);
        int min = totalTime / 60;
        int sec = totalTime % 60;
        timerText.text = string.Format("{0:D2}:{1:D2}", min,sec);
    }
}
