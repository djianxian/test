using System.Collections.Generic;
using TestGame.Data;
using TestGame.Model;
using TestGame.View;
using UnityEngine;

namespace TestGame.Controller
{
    public class GameController : MonoBehaviour
    {
        private GameModel model = new GameModel();
        [SerializeField] private GameView view;
        private Stack<OptionData> answers;

        private async void Start()
        {
            await model.Init();
            view.OptionClickAction = OnOptionClick;
            view.OptionErrorAction = OnOptionErrorCB;
            await view.SetOptions(model.NowQuestion());
            view.SetAudioClick(OnAudioClick);
            view.SetResetClick(OnResetClick);
            view.SetSubmitClick(OnSubmitClick);
            view.PlayAudio(model.NowQuestion().questionAudioSha1);
            view.DrawTo(model.GetAnswer(0));
        }

        private void OnAudioClick()
        {
            view.PlayAudio(model.NowQuestion().questionAudioSha1);
        }

        private void OnResetClick()
        {
            if (answers != null && answers.Count > 0)
            {
                return;
            }

            answers = model.GetAllAnswers();
            if (answers.Count == 0)
            {
                return;
            }

            InvokeRepeating("RemoveAnswer", 0, 0.5f);
        }

        private async void OnSubmitClick()
        {
            if (model.CheckTryTimes())
            {
                if (model.SubmitResult())
                {
                    var nextQuestion = model.NextQuestion();
                    if (nextQuestion != null)
                    {
                        await view.SetOptions(nextQuestion);
                    }

                    UpdateView();
                }
            }
            else
            {
                model.Reset();
            }

            UpdateView();
        }

        private void OnOptionClick(OptionData optionData)
        {
            int answersCount = model.GetAnswersCount();
            int answersMax = model.GetAnswersMax();
            if (answersCount >= answersMax)
            {
                return;
            }
            view.DrawTo(optionData);
            model.AddAnswer(optionData);
            UpdateView();
        }

        private void OnOptionErrorCB()
        {
            if (answers != null && answers.Count > 0)
            {
                return;
            }

            answers = model.GetErrorAnswers();
            if (answers.Count == 0)
            {
                return;
            }

            InvokeRepeating("RemoveAnswer", 0, 0.5f);
        }

        private void RemoveAnswer()
        {
            var optionData = answers.Pop();
            bool isLast = answers.Count == 0;
            model.RemoveAnswer(optionData, isLast);
            view.DrawTo(model.GetAnswer(model.GetAnswersCount()-1), true);
            if (isLast)
            {
                CancelInvoke("RemoveAnswer");
            }

            UpdateView();
        }

        private void UpdateView()
        {
            view.UpdateView();
            view.ShowTryTimes(model.GetAnswersCount(), model.GetAnswersMax());
        }
    }
}