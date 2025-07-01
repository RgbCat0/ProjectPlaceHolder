using System.Collections;
using TMPro;
using UnityEngine;

namespace Enemies
{
    public class HitAnimation : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI hitText;

        [SerializeField]
        private float destroyTime = 1f;

        [SerializeField]
        private AnimationCurve curveX,
            curveY,
            curveZ,
            curveScale;

        public void ShowHitText(string text)
        {
            
            if (hitText != null)
            {
                hitText.text = text;
                hitText.gameObject.SetActive(true);
            }

            StartCoroutine(Animation());
        }

        private IEnumerator Animation()
        {
            var timer = 0f;
            var startPosition = Vector3.up * 1.5f; 
            transform.localScale = Vector3.zero;
            var randomX = Random.Range(-0.4f, 0.4f);
            var randomZ = Random.Range(-0.4f, 0.4f);
            while (timer < destroyTime)
            {
                timer += Time.deltaTime;
                var t = timer / destroyTime;

                // Apply the animation curves to position and scale
                transform.localPosition = new Vector3(
                    startPosition.x + curveX.Evaluate(t) + randomX,
                    startPosition.y + curveY.Evaluate(t),
                    startPosition.z + curveZ.Evaluate(t) + randomZ
                );

                transform.localScale = Vector3.one * curveScale.Evaluate(t);

                yield return null;
            }
            Destroy(gameObject);
        }
    }
}