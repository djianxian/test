using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Coffee.UIEffects;
using DG.Tweening;
using DG.Tweening.Core;
using TestGame.Data;
using TestGame.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TestGame.View
{
    public class OptionView : MonoBehaviour
    {
        [SerializeField] private UIEffect ueOutline;
        [SerializeField] private UIEffect ueDissolve;
        [SerializeField] private Button btnOption;
        [SerializeField] private Image imgOption;
        private Color colorClose = new(1, 1, 1, 0.3f);
        private OptionData data;
        private static bool isError = false;
        private OptionState lastState;
        private OptionState nextState;

        public UnityAction<OptionData> OptionClickAction { get; set; }
        public UnityAction OptionErrorAction { get; set; }

        public async Task Init(OptionData optionData)
        {
            isError = false;
            UIUtil.SetButtonAction(btnOption, OnOptionClick);
            data = optionData;
            imgOption.sprite = await ResUtil.LoadAsset<Sprite>($"Images/{optionData.sha1}.png");
            ChangeState(optionData.state);
        }

        public void UpdateView()
        {
            ChangeState(data.state);
        }

        public void ChangeState(OptionState state)
        {
            if (lastState == state)
            {
                return;
            }

            switch (state)
            {
                case OptionState.Open:
                    ueOutline.enabled = true;
                    ueOutline.shadowMode = ShadowMode.Shadow3;
                    ueOutline.color = Color.black;
                    btnOption.interactable = true;
                    imgOption.DOColor(Color.white, 0.2f);
                    imgOption.transform.DOScale(Vector3.one * 1.1f, 0.2f);
                    break;
                case OptionState.Close:
                    ueOutline.enabled = false;
                    btnOption.interactable = false;
                    imgOption.DOColor(colorClose, 0.2f);
                    imgOption.transform.DOScale(Vector3.one, 0.2f);
                    break;
                case OptionState.Select:
                    ueOutline.enabled = true;
                    ueOutline.shadowMode = ShadowMode.Outline8;
                    ueOutline.color = Color.white;
                    btnOption.interactable = false;
                    imgOption.DOColor(Color.white, 0.2f);
                    imgOption.transform.DOScale(Vector3.one, 0.2f);
                    DOTween.To(() => ueDissolve.transitionRate, x => ueDissolve.transitionRate = x, 1, 0.5f);
                    break;
                case OptionState.Error:
                    isError = true;
                    transform.localRotation = Quaternion.Euler(0, 0, -10);
                    transform.DORotateQuaternion(Quaternion.Euler(0, 0, 10), 0.5f)
                        .SetLoops(6, LoopType.Yoyo).onComplete = () =>
                    {
                        if (isError)
                        {
                            OptionErrorAction?.Invoke();
                            isError = false;
                        }

                        transform.localRotation = Quaternion.Euler(0, 0, 0);
                    };
                    break;
                default:
                    break;
            }

            if (state != OptionState.Error && lastState == OptionState.Select || lastState == OptionState.Error)
            {
                DOTween.To(() => ueDissolve.transitionRate, x => ueDissolve.transitionRate = x, 0, 0.5f);
            }

            lastState = state;
        }

        private void OnOptionClick()
        {
            OptionClickAction?.Invoke(data);
        }
    }
}