using UnityEngine;
using UnityEngine.UI;

namespace VRUiKits.Utils
{
    public class Item : MonoBehaviour
    {
        public Button button;
        public delegate void OnItemSelectedHandler(Item item);
        public event OnItemSelectedHandler OnItemSelected;
        protected Color normalColor; // record the current normal color of the button
        protected Color highlightedColor;
        Image image;
        protected Sprite originSprite;
        protected Sprite highlightedSprite;
        bool listenerRegistered;
        bool visualStateCached;

        void Awake()
        {
            if (!EnsureButton())
                return;

            if (!listenerRegistered)
            {
                button.onClick.AddListener(() =>
                {
                    if (null != OnItemSelected)
                    {
                        OnItemSelected(this);
                    }
                });
                listenerRegistered = true;
            }
        }

        bool EnsureButton()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (image == null)
            {
                image = GetComponent<Image>();
            }

            if (button == null)
            {
                Debug.LogError("Item requires button to be assigned", this);
                return false;
            }

            if (!visualStateCached)
            {
                if (button.transition == Selectable.Transition.ColorTint)
                {
                    normalColor = button.colors.normalColor;
                    highlightedColor = button.colors.highlightedColor;
                }

                if (null != image && button.transition == Selectable.Transition.SpriteSwap)
                {
                    originSprite = image.sprite;
                    highlightedSprite = button.spriteState.highlightedSprite;
                }

                visualStateCached = true;
            }

            return true;
        }

        public virtual void Activate()
        {
            if (!EnsureButton())
                return;

            if (button.transition == Selectable.Transition.ColorTint)
            {
                //Changes the button's Normal color to the new color.
                ColorBlock cb = button.colors;
                cb.normalColor = button.colors.selectedColor;
                cb.highlightedColor = button.colors.selectedColor;
                button.colors = cb;
            }

            if (null != image && button.transition == Selectable.Transition.SpriteSwap)
            {
                image.sprite = button.spriteState.selectedSprite;
                SpriteState spriteState = button.spriteState;
                spriteState.highlightedSprite = button.spriteState.selectedSprite;
                button.spriteState = spriteState;
            }
        }

        public virtual void Deactivate()
        {
            if (!EnsureButton())
                return;

            if (button.transition == Selectable.Transition.ColorTint)
            {
                //Changes the button's Normal color to the original color.
                ColorBlock cb = button.colors;
                cb.normalColor = normalColor;
                Debug.Log("normalcolor has change!");
                cb.highlightedColor = highlightedColor;
                button.colors = cb;
            }

            if (null != image && button.transition == Selectable.Transition.SpriteSwap)
            {
                image.sprite = originSprite;
                SpriteState spriteState = button.spriteState;
                spriteState.highlightedSprite = highlightedSprite;
                button.spriteState = spriteState;
            }
        }
    }
}
