using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;

public class ReadyButton : MonoBehaviour
{
    private Outline outline;
    public bool interactableNow = false;

    public float pressDepth = 0.2f; // 버튼이 눌리는 깊이
    public float pressDuration = 0.1f; // 눌리는 시간
    public float returnDuration = 0.1f; // 돌아오는 시간

    private Vector3 originalScale;
    private bool isPressed = false;
    private bool isReady = false;
    public ReadyDisplay readyDisplay;

    private void Start()
    {
        outline = GetComponent<Outline>();
        originalScale = transform.localScale;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && interactableNow)
            Interact();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PhotonView>(out PhotonView pv))
        {
            if (pv.IsMine)
            {
                outline.eraseRenderer = false;
                interactableNow = true;
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PhotonView>(out PhotonView pv))
        {
            if (pv.IsMine)
            {
                outline.eraseRenderer = true;
                interactableNow = false;
            }
        }
    }

    public void Interact()
    {
        StartCoroutine(PressButton());
    }
    IEnumerator PressButton()
    {
        isPressed = true;
        isReady = !isReady;
        readyDisplay.ReadyButtonInteract(isReady);
        Vector3 targetScale = new Vector3(originalScale.x, originalScale.y, 0.2f);

        float elapsedTime = 0f;
        while (elapsedTime < pressDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / pressDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;

        elapsedTime = 0f;
        while (elapsedTime < returnDuration)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsedTime / returnDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;

        isPressed = false;
    }
}
