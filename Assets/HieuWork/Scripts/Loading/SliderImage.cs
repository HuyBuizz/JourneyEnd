using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SliderImage : MonoBehaviour
{
    public RawImage[] images;
    private int currentIndex = 0;
    public float autoSlideDelay = 2f;

    private Coroutine autoSlideCoroutine;

    void Start()
    {
        ShowImage(currentIndex);
        StartAutoSlide();
    }

    public void NextImage()
    {
        currentIndex = (currentIndex + 1) % images.Length;
        ShowImage(currentIndex);
        RestartAutoSlide();
    }

    public void PrevImage()
    {
        currentIndex = (currentIndex - 1 + images.Length) % images.Length;
        ShowImage(currentIndex);
        RestartAutoSlide();
    }

    private void ShowImage(int index)
    {
        for (int i = 0; i < images.Length; i++)
        {
            images[i].gameObject.SetActive(i == index);
        }
    }

    private void StartAutoSlide()
    {
        autoSlideCoroutine = StartCoroutine(AutoSlide());
    }

    private void RestartAutoSlide()
    {
        if (autoSlideCoroutine != null)
        {
            StopCoroutine(autoSlideCoroutine);
        }
        StartAutoSlide();
    }

    private IEnumerator AutoSlide()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSlideDelay);
            NextImage();
        }
    }
}
