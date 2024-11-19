using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;
    
    public Image fadeImage;
    private float fadeDuration = 2f;
    private void Start()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public IEnumerator FadeIn(Action callback = null)
    {
        float t = fadeDuration;
        while (t > 0)
        {
            t -= Time.deltaTime;
            float alpha = Mathf.Clamp01(t / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(0);
        callback?.Invoke();
    }

    public IEnumerator FadeOut(float waitDuration,Action callback = null)
    {
        yield return new WaitForSeconds(waitDuration);
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(1);
        callback?.Invoke();
    }

    void SetAlpha(float alpha)
    {
        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }
}
