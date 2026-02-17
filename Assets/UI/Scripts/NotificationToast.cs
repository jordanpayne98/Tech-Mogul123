using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace TechMogul.UI
{
    public class NotificationToast : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        
        private Color _infoColor = new Color(0.2f, 0.6f, 1f, 0.9f);
        private Color _successColor = new Color(0.18f, 0.80f, 0.44f, 0.9f);
        private Color _warningColor = new Color(0.95f, 0.61f, 0.07f, 0.9f);
        private Color _errorColor = new Color(0.91f, 0.30f, 0.24f, 0.9f);
        
        public void Show(string message, NotificationType type, float duration)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }
            
            if (backgroundImage != null)
            {
                backgroundImage.color = type switch
                {
                    NotificationType.Success => _successColor,
                    NotificationType.Warning => _warningColor,
                    NotificationType.Error => _errorColor,
                    _ => _infoColor
                };
            }
            
            StartCoroutine(AnimateNotification(duration));
        }
        
        IEnumerator AnimateNotification(float displayDuration)
        {
            // Fade in
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                float timer = 0;
                
                while (timer < fadeInDuration)
                {
                    timer += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeInDuration);
                    yield return null;
                }
                
                canvasGroup.alpha = 1;
            }
            
            // Display
            yield return new WaitForSeconds(displayDuration);
            
            // Fade out
            if (canvasGroup != null)
            {
                float timer = 0;
                
                while (timer < fadeOutDuration)
                {
                    timer += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeOutDuration);
                    yield return null;
                }
            }
            
            Destroy(gameObject);
        }
    }
}
