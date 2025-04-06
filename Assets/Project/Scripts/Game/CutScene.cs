using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class CutScene : MonoBehaviour
    {
        [SerializeField] private Image fadeImage;
        [SerializeField] private TextMeshProUGUI sayText;

        private bool skip;

        private void Awake()
        {
            G.CutScene = this;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                skip = true;
            }
        }

        public IEnumerator SmartWait(float f)
        {
            skip = false;
            while (f > 0 && !skip)
            {
                f -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
        public void SetBG(float value)
        {
            fadeImage.DOFade(value, 0);
        }
        public IEnumerator DoFade(float from, float to, float duration = 0.5f)
        {
            yield return fadeImage.DOFade(to, duration).From(from).WaitForCompletion();
        }
        public IEnumerator Say(string actionDefinition)
        {
            var visibleLength = TextUtils.GetVisibleLength(actionDefinition);
            if (visibleLength == 0)
                yield break;

            for (var i = 0; i < visibleLength; i++)
            {
                sayText.text = TextUtils.CutSmart(actionDefinition, 1 + i);
                yield return new WaitForEndOfFrame();

                //todo
                //use 'withSound' here
                //withSound - CMS Type
                //G.audio.Play<SFX_TypeChar>();
            }
        }
        public IEnumerator ContinueSay(string actionDefinition)
        {
            var startLength = TextUtils.GetVisibleLength(sayText.text);
            var targetDefinition = sayText.text + actionDefinition;
            var visibleLength = TextUtils.GetVisibleLength(targetDefinition);
            if (visibleLength == 0)
                yield break;

            for (var i = startLength; i < visibleLength; i++)
            {
                sayText.text = TextUtils.CutSmart(targetDefinition, 1 + i);
                yield return new WaitForEndOfFrame();

                //todo
                //use 'withSound' here
                //withSound - CMS Type
                //G.audio.Play<SFX_TypeChar>();
            }
        }
        public IEnumerator Unsay(string actionDefinition)
        {
            var visibleLength = TextUtils.GetVisibleLength(actionDefinition);
            if (visibleLength == 0)
                yield break;

            var str = "";

            for (var i = visibleLength - 1; i >= 0; i--)
            {
                str = TextUtils.CutSmart(actionDefinition, i);
                sayText.text = str;
                yield return new WaitForEndOfFrame();
            }

            sayText.text = "";
        }
        public IEnumerator Unsay()
        {
            var actionDefinition = sayText.text;
            var visibleLength = TextUtils.GetVisibleLength(actionDefinition);
            if (visibleLength == 0)
                yield break;

            var str = "";

            for (var i = visibleLength - 1; i >= 0; i--)
            {
                str = TextUtils.CutSmart(actionDefinition, i);
                sayText.text = str;
                yield return new WaitForEndOfFrame();
            }

            sayText.text = "";
        }
    }
}