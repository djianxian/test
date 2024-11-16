using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LitJson;
using TestGame.Data;
using TestGame.Utils;
using UnityEngine;

namespace TestGame.Model
{
    public class GameModel
    {
        private int questionIndex;
        private List<OptionData> answers;
        private QuestionData[] questionDatas;
        private int tryTimesNow;
        private const int TRY_TIMES_MAX = 2;

        #region public function

        public async Task Init()
        {
            TextAsset jsonData = await ResUtil.LoadAsset<TextAsset>("Configs/data.json");
            ParseJsonData(jsonData.text);
            Reset();
        }

        public void Reset()
        {
            tryTimesNow = 0;
            var nowQuestion = NowQuestion();
            for (int i = 0; i < nowQuestion.options.GetLength(0); i++)
            {
                for (int j = 0; j < nowQuestion.options.GetLength(1); j++)
                {
                    nowQuestion.options[i, j].state = OptionState.Close;
                }
            }

            answers = new List<OptionData>();
            AddAnswer(StartOption());
        }

        public int GetAnswersCount()
        {
            return answers.Count;
        }

        public int GetAnswersMax()
        {
            return NowQuestion().answers.Length;
        }
        
        public OptionData GetAnswer(int index)
        {
            return answers[index];
        }

        public void AddAnswer(OptionData optionData)
        {
            optionData.state = OptionState.Select;
            if (answers.Count > 0)
            {
                EnableRoundOption(false, answers[answers.Count - 1]);
            }

            answers.Add(optionData);
            if (answers.Count < NowQuestion().answers.Length)
            {
                EnableRoundOption(true, optionData);
            }
        }

        public QuestionData NextQuestion()
        {
            questionIndex++;
            if (questionIndex >= questionDatas.Length)
            {
                questionIndex = 0;
                return null;
            }

            Reset();
            return NowQuestion();
        }

        public QuestionData NowQuestion()
        {
            return questionDatas[questionIndex];
        }

        public bool CheckTryTimes()
        {
            tryTimesNow++;
            if (tryTimesNow > TRY_TIMES_MAX)
            {
                tryTimesNow = 0;
                return false;
            }

            return true;
        }

        public bool SubmitResult()
        {
            var nowQuestion = NowQuestion();
            bool isRight = true;
            for (int i = 0; i < answers.Count; i++)
            {
                var answer = answers[i];
                var index = answer.rowIndex * nowQuestion.optionsCols + answer.colIndex;
                if (nowQuestion.answers[i] != index)
                {
                    answer.state = OptionState.Error;
                    isRight = false;
                }
            }

            return isRight;
        }

        public void RemoveAnswer(OptionData optionData, bool isLast)
        {
            EnableRoundOption(false, optionData);
            optionData.state = OptionState.Close;
            answers.Remove(optionData);
            if (isLast)
            {
                EnableRoundOption(true, answers[answers.Count - 1]);
            }
        }

        public Stack<OptionData> GetErrorAnswers()
        {
            var stack = new Stack<OptionData>();
            int firstErrorIndex = -1;
            for (int i = 0; i < answers.Count; i++)
            {
                var answer = answers[i];
                if (answer.state == OptionState.Error)
                {
                    firstErrorIndex = i;
                }

                if (firstErrorIndex > 0 && i >= firstErrorIndex)
                {
                    stack.Push(answer);
                }
            }

            return stack;
        }

        public Stack<OptionData> GetAllAnswers()
        {
            var stack = new Stack<OptionData>();
            for (int i = 1; i < answers.Count; i++)
            {
                var answer = answers[i];
                stack.Push(answer);
            }

            return stack;
        }

        #endregion

        #region private function

        private OptionData StartOption()
        {
            var nowQuestion = NowQuestion();
            var index = nowQuestion.answers[0];
            int i = index / nowQuestion.optionsCols;
            int j = index % nowQuestion.optionsCols;
            return nowQuestion.options[i, j];
        }

        private void EnableRoundOption(bool enable, OptionData optionData)
        {
            int i = optionData.rowIndex;
            int j = optionData.colIndex;
            OptionState state = enable ? OptionState.Open : OptionState.Close;
            SetOptionEable(state, i - 1, j);
            SetOptionEable(state, i + 1, j);
            SetOptionEable(state, i, j - 1);
            SetOptionEable(state, i, j + 1);
        }

        private void SetOptionEable(OptionState state, int i, int j)
        {
            var nowQuestion = NowQuestion();
            if (nowQuestion.optionsRows <= i || nowQuestion.optionsCols <= j || i < 0 || j < 0)
            {
                return;
            }

            if (nowQuestion.options[i, j].state == OptionState.Select || nowQuestion.options[i, j].state == OptionState.Error)
            {
                return;
            }

            nowQuestion.options[i, j].state = state;
        }

        private void ParseJsonData(string jsonData)
        {
            JsonData dataJD = JsonMapper.ToObject(jsonData);
            JsonData questionsJD = dataJD[0]["Activity"]["Questions"];
            questionDatas = new QuestionData[questionsJD.Count];
            for (int i = 0; i < questionsJD.Count; i++)
            {
                QuestionData questionData = new QuestionData();
                questionDatas[i] = questionData;
                JsonData questionJD = questionsJD[i]["Body"];
                questionData.optionsRows = Int32.Parse(questionJD["tags"][0]["rows"].ToString());
                questionData.optionsCols = Int32.Parse(questionJD["tags"][0]["cols"].ToString());
                Debug.Log($"rows: {questionData.optionsRows}, cols: {questionData.optionsCols}");
                questionData.options = new OptionData[questionData.optionsRows, questionData.optionsCols];
                JsonData optionsJD = questionJD["options"];
                for (int j = 0; j < optionsJD.Count; j++)
                {
                    string sha1 = optionsJD[j]["image"]["sha1"].ToString();
                    int rowIndex = Int32.Parse(optionsJD[j]["rowIndex"].ToString());
                    int colIndex = Int32.Parse(optionsJD[j]["colIndex"].ToString());
                    OptionData optionData = new OptionData();
                    optionData.sha1 = sha1;
                    optionData.state = OptionState.Close;
                    optionData.rowIndex = rowIndex - 1;
                    optionData.colIndex = colIndex - 1;
                    questionData.options[optionData.rowIndex, optionData.colIndex] = optionData;
                    Debug.Log($"sha1: {sha1}, rowIndex: {rowIndex}, colIndex: {colIndex}");
                }

                JsonData answersJD = questionJD["answers"][0];
                questionData.answers = new int[answersJD.Count];
                for (int j = 0; j < answersJD.Count; j++)
                {
                    questionData.answers[j] = Int32.Parse(answersJD[j].ToString());
                    Debug.Log($"answers: {questionData.answers[j]}");
                }

                questionData.questionAudioSha1 =
                    questionsJD[i]["stimulusOfQuestion"]["Body"]["item"]["audio"]["sha1"].ToString();
                Debug.Log($"questionAudioSha1: {questionData.questionAudioSha1}");
            }
        }

        #endregion
    }
}