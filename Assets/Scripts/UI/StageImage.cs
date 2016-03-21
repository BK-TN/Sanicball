using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sanicball.UI
{
    [RequireComponent(typeof(LayoutElement))]
    [RequireComponent(typeof(Button))]
    public class StageImage : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        public StageSelection stageSelectionObject;

        public float baseWidth = 350f;
        public float baseHeight = 233f;
        public float selectedScale = 0.8f;
        public float deselectedScale = 0.5f;
        public float animationSpeed = 5f;

        private bool selected = false;

        private LayoutElement le;

        private Button b;

        //Testing out delegates...
        public delegate void StageImageEvent();

        public event StageImageEvent onSelect;

        public event StageImageEvent onActivate;

        public void OnSelect(BaseEventData eventData)
        {
            selected = true;
            if (onSelect != null) onSelect.Invoke();
            b.onClick.AddListener(Activate);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            selected = false;
            b.onClick.RemoveAllListeners();
        }

        // Use this for initialization
        private void Start()
        {
            le = GetComponent<LayoutElement>();
            b = GetComponent<Button>();

            le.preferredWidth = baseWidth * deselectedScale;
            le.preferredHeight = baseHeight * deselectedScale;
        }

        // Update is called once per frame
        private void Update()
        {
            float targetWidth = selected ? baseWidth * selectedScale : baseWidth * deselectedScale;
            float targetHeight = selected ? baseHeight * selectedScale : baseHeight * deselectedScale;

            le.preferredWidth = Mathf.Lerp(le.preferredWidth, targetWidth, Time.deltaTime * animationSpeed);
            le.preferredHeight = Mathf.Lerp(le.preferredHeight, targetHeight, Time.deltaTime * animationSpeed);
        }

        private void Activate()
        {
            if (onActivate != null) onActivate.Invoke();
        }
    }
}
