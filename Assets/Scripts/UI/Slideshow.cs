using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class Slideshow : MonoBehaviour
    {
        public Text activeSlideDisplay;
        public int firstActive = 0;
        private SlideshowSlide[] slides;
        private int currentSlide = -1;

        public void NextSlide()
        {
            if (currentSlide < slides.Length - 1)
                SetSlide(currentSlide + 1);
            else
                SetSlide(0);
        }

        public void PrevSlide()
        {
            if (currentSlide > 0)
                SetSlide(currentSlide - 1);
            else
                SetSlide(slides.Length - 1);
        }

        public void SetSlide(int slide)
        {
            if (slide >= 0 && slide < slides.Length)
            {
                //Hide previously active slide
                if (currentSlide >= 0 && currentSlide < slides.Length)
                {
                    slides[currentSlide].gameObject.SetActive(false);
                }

                slides[slide].gameObject.SetActive(true);

                currentSlide = slide;

                if (activeSlideDisplay)
                {
                    activeSlideDisplay.text = (currentSlide + 1) + "/" + slides.Length;
                }
            }
        }

        //Called by buttons when switching slide
        public void TrySetSelectedGameObject(GameObject o)
        {
            var eventSystem = FindObjectOfType<EventSystem>();

            if (eventSystem)
            {
                eventSystem.SetSelectedGameObject(o);
            }
        }

        private void Start()
        {
            slides = GetComponentsInChildren<SlideshowSlide>();

            //Reset and hide all slides
            foreach (var slide in slides)
            {
                slide.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                slide.gameObject.SetActive(false);
            }
            //Unhide first active slide
            SetSlide(firstActive);
        }

        private void Update()
        {
        }
    }
}
