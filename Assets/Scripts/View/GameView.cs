using System.Threading.Tasks;
using DG.Tweening;
using TestGame.Data;
using TestGame.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TestGame.View
{
    public class GameView : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Button btnAudio;
        [SerializeField] private Button btnReset;
        [SerializeField] private Button btnSubmit;
        [SerializeField] private Text txtCount;
        [SerializeField] private GridLayoutGroup gridOptions;
        [SerializeField] private GameObject goOption;
        [SerializeField] private GameObject goCount;
        [SerializeField] private UIMask scratchCard;

        private OptionView[,] optionViews;
        private QuestionData data;

        public UnityAction<OptionData> OptionClickAction { get; set; }
        public UnityAction OptionErrorAction { get; set; }

        public void SetAudioClick(UnityAction action) => UIUtil.SetButtonAction(btnAudio, action);

        public void SetResetClick(UnityAction action) => UIUtil.SetButtonAction(btnReset, action);

        public void SetSubmitClick(UnityAction action) => UIUtil.SetButtonAction(btnSubmit, action);

        public async Task SetOptions(QuestionData questionData)
        {
            data = questionData;
            optionViews = new OptionView[questionData.optionsRows, questionData.optionsCols];
            gridOptions.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridOptions.constraintCount = questionData.optionsCols;
            for (int i = 0; i < questionData.optionsRows; i++)
            {
                for (int j = 0; j < questionData.optionsCols; j++)
                {
                    OptionView optionView;
                    var index = i * questionData.optionsCols + j;
                    if (index < gridOptions.transform.childCount)
                    {
                        var go = gridOptions.transform.GetChild(index).gameObject;
                        optionView = go.GetComponent<OptionView>();
                        await optionView.Init(questionData.options[i, j]);
                        optionViews[i, j] = optionView;
                    }
                    else
                    {
                        var go = Instantiate(goOption);
                        optionView = go.GetComponent<OptionView>();
                        await optionView.Init(questionData.options[i, j]);
                        go.transform.SetParent(gridOptions.transform);
                        optionView.OptionClickAction = OptionClickAction;
                        optionView.OptionErrorAction = OptionErrorAction;
                        optionViews[i, j] = optionView;
                    }
                }
            }
        }

        public void UpdateView()
        {
            for (int i = 0; i < optionViews.GetLength(0); i++)
            {
                for (int j = 0; j < optionViews.GetLength(1); j++)
                {
                    optionViews[i, j].UpdateView();
                }
            }

            btnSubmit.gameObject.SetActive(data.answers.Length == data.answers.Length);
        }

        public void ShowTryTimes(int now, int max)
        {
            goCount.SetActive(now < max);
            btnSubmit.gameObject.SetActive(now >= max);
            if (now < max)
            {
                txtCount.text = $"{now}/{max}";
            }
        }

        public OptionView GetOption(OptionData optionData)
        {
            return optionViews[optionData.rowIndex, optionData.colIndex];
        }

        public async void PlayAudio(string sha1)
        {
            var clip = await ResUtil.LoadAsset<AudioClip>($"Audios/{sha1}.mp3");
            audioSource.clip = clip;
            audioSource.Play();
        }

        public void DrawTo(OptionData optionData, bool isClear = false)
        {
            if (isClear)
            {
                scratchCard.ClearTo(GetOption(optionData).transform.position);
            }
            else
            {
                scratchCard.FromTo(GetOption(optionData).transform.position);
            }
        }
    }
}