using DG.Tweening;
using UnityEngine;

namespace Voidwalker
{
    public class TrophyGoldCup : MonoBehaviour
    {
        [SerializeField] private float _cycleLength = 1.5f;
        [SerializeField] private float _lenghtMultiplier = 10f;

        void Start()
        {
            transform.DOMoveY(1.5f, _cycleLength).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            transform.DORotate(new Vector3(0, 360, 0), _cycleLength * _lenghtMultiplier, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
        }

        private void OnDisable()
        {
            transform.DOKill();
        }
    }
}
