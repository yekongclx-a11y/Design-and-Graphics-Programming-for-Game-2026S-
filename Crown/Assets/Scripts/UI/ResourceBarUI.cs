using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Crown.UI
{
    /// <summary>
    /// 单个资源条（金币/民心/教会/军队）。
    /// 用 Image.Filled 实现图标内填充效果，类似 Reigns。
    /// </summary>
    public class ResourceBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image iconFill;      // 必须是 Filled 类型
        [SerializeField] private TMP_Text valueText;

        [Header("Animation")]
        [SerializeField] private float tweenDuration = 0.4f;
        [SerializeField] private AnimationCurve tweenCurve =
            AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Flash on change")]
        [SerializeField] private Color flashUpColor   = new Color(1f, 0.95f, 0.6f);
        [SerializeField] private Color flashDownColor = new Color(1f, 0.4f, 0.4f);
        [SerializeField] private float flashDuration  = 0.25f;

        private const int MaxValue = 100;
        private int currentValue = 50;
        private Coroutine tweenRoutine;
        private Coroutine flashRoutine;
        private Color baseColor;

        void Awake()
        {
            if (iconFill != null)
            {
                baseColor = iconFill.color;
                // 强制确保配置正确，防止 Inspector 里手滑改错
                iconFill.type        = Image.Type.Filled;
                iconFill.fillMethod  = Image.FillMethod.Vertical;
                iconFill.fillOrigin  = (int)Image.OriginVertical.Bottom;
            }
        }

        /// <summary>
        /// 直接设置数值，无动画。用于场景初始化、重置游戏。
        /// </summary>
        public void SetValueImmediate(int value)
        {
            currentValue = Mathf.Clamp(value, 0, MaxValue);
            ApplyVisual(currentValue);
        }

        /// <summary>
        /// 设置数值，带补间动画 + 颜色闪烁反馈。
        /// 数值变化时调用这个。
        /// </summary>
        public void SetValue(int newValue)
        {
            newValue = Mathf.Clamp(newValue, 0, MaxValue);
            int delta = newValue - currentValue;

            if (tweenRoutine != null) StopCoroutine(tweenRoutine);
            tweenRoutine = StartCoroutine(TweenTo(newValue));

            if (delta != 0) Flash(delta > 0);

            currentValue = newValue;
        }

        private IEnumerator TweenTo(int target)
        {
            int   from = Mathf.RoundToInt(iconFill.fillAmount * MaxValue);
            float t    = 0f;

            while (t < tweenDuration)
            {
                t += Time.unscaledDeltaTime; // unscaled 防止暂停时卡住
                float k = tweenCurve.Evaluate(t / tweenDuration);
                int   v = Mathf.RoundToInt(Mathf.Lerp(from, target, k));
                ApplyVisual(v);
                yield return null;
            }
            ApplyVisual(target);
        }

        private void ApplyVisual(int v)
        {
            if (iconFill != null)
                iconFill.fillAmount = v / (float)MaxValue;
            if (valueText != null)
                valueText.text = v.ToString();
        }

        private void Flash(bool isUp)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine(isUp));
        }

        private IEnumerator FlashRoutine(bool isUp)
        {
            Color target = isUp ? flashUpColor : flashDownColor;
            float t = 0f;

            while (t < flashDuration / 2f)
            {
                t += Time.unscaledDeltaTime;
                iconFill.color = Color.Lerp(baseColor, target,
                    t / (flashDuration / 2f));
                yield return null;
            }
            t = 0f;
            while (t < flashDuration / 2f)
            {
                t += Time.unscaledDeltaTime;
                iconFill.color = Color.Lerp(target, baseColor,
                    t / (flashDuration / 2f));
                yield return null;
            }
            iconFill.color = baseColor;
        }
    }
}