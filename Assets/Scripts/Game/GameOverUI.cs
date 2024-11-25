using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public Text[] texts;
    public Image[] images;

    private float moveDuration = .5f;
    private float fillDuration = .5f; 
    public Vector2 targetPosition = new Vector2(0, 0); 

    private void OnEnable()
    {
        StartCoroutine(ShowGameOverUI());
    }

    private IEnumerator ShowGameOverUI()
    {
        float elapsedTime = 0f;
        Vector2[] startPositions = new Vector2[texts.Length];
        for (int i = 0; i < texts.Length; i++)
            startPositions[i] = texts[i].GetComponent<RectTransform>().anchoredPosition;

        while (elapsedTime < Mathf.Max(moveDuration, fillDuration))
        {
            elapsedTime += Time.deltaTime;
            float moveT = Mathf.Clamp01(elapsedTime / moveDuration);
            for (int i = 0; i < texts.Length; i++)
            {
                RectTransform rect = texts[i].GetComponent<RectTransform>();
                rect.anchoredPosition = Vector2.Lerp(startPositions[i], targetPosition, moveT);
            }

            float fillT = Mathf.Clamp01(elapsedTime / fillDuration);
            foreach (var image in images)
                image.fillAmount = Mathf.Lerp(0, 1, fillT);
            yield return null;
        }
    }
}
