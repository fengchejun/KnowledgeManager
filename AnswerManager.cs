using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

// 答题管理器核心类
public class AnswerManager : MonoBehaviour
{
    #region 单例与外部引用
    public static AnswerManager Instance;

    // UI组件绑定（在Inspector面板中赋值）
    [Header("题目显示组件")]
    public Text QuestionTypeText;
    public Text QuestionTitleText;

    [Header("题型答题区域")]
    public GameObject ChoiceAnswerArea;
    public GameObject TrueFalseAnswerArea;
    public GameObject FillBlankAnswerArea;

    [Header("选择题组件")]
    public ToggleGroup ChoiceToggleGroup;
    public List<Text> ChoiceOptionTexts; // 对应A/B/C/D四个选项的文本

    [Header("判断题组件")]
    public Toggle TrueToggle;
    public Toggle FalseToggle;

    [Header("填空题组件")]
    public InputField FillBlankInput;

    [Header("按钮与结果组件")]
    public Button SubmitAnswerBtn;
    public Text ResultTipText;
    public Button NextQuestionBtn;
    #endregion

    #region 内部状态变量 - 关键修改
    private KnowledgePoint _currentKnowledge; // 当前答题的知识点
    private List<QuestionBase> _currentQuestions; // 改为 QuestionBase
    private int _currentQuestionIndex; // 当前答题的题型索引
    private QuestionBase _currentQuestion; // 改为 QuestionBase

    // 复习模式相关
    private bool _isReviewMode; // 是否处于复习模式
    private Action<bool> _onQuestionAnswerComplete; // 单题完成回调（通知复习队列）
    #endregion

    #region 生命周期与初始化
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 初始状态：隐藏答题面板和下一题按钮
        gameObject.SetActive(false);
        NextQuestionBtn.gameObject.SetActive(false);

        // 绑定按钮事件
        SubmitAnswerBtn.onClick.AddListener(OnSubmitAnswer);
        NextQuestionBtn.onClick.AddListener(OnNextQuestion);

        // 初始化选项文本列表（防止空引用）
        if (ChoiceOptionTexts == null || ChoiceOptionTexts.Count == 0)
        {
            ChoiceOptionTexts = ChoiceToggleGroup.GetComponentsInChildren<Text>().ToList();
        }
    }
    #endregion

    #region 外部调用方法
    /// <summary>
    /// 普通模式：开始单个知识点的答题
    /// </summary>
    public void StartAnswer(KnowledgePoint knowledge)
    {
        _isReviewMode = false;
        _onQuestionAnswerComplete = null;
        InitAnswerData(knowledge);
    }

    /// <summary>
    /// 复习模式：开始答题并绑定单题完成回调
    /// </summary>
    public void StartReviewAnswer(KnowledgePoint knowledge, Action<bool> onQuestionComplete)
    {
        _isReviewMode = true;
        _onQuestionAnswerComplete = onQuestionComplete;
        InitAnswerData(knowledge);
    }

    /// <summary>
    /// 判断当前知识点的题目是否全部答完
    /// </summary>
    public bool IsCurrentKPAnswerFinished()
    {
        return _currentQuestionIndex >= _currentQuestions.Count;
    }

    /// <summary>
    /// 结束复习模式，重置状态
    /// </summary>
    public void EndReview()
    {
        _isReviewMode = false;
        _onQuestionAnswerComplete = null;
        gameObject.SetActive(false);
        ResultTipText.text = "";
    }
    #endregion

    #region 内部初始化与题目加载
    /// <summary>
    /// 初始化答题数据
    /// </summary>
    private void InitAnswerData(KnowledgePoint knowledge)
    {
        _currentKnowledge = knowledge;
        _currentQuestions = knowledge.Questions ?? new List<QuestionBase>(); // 改为 QuestionBase
        _currentQuestionIndex = 0;

        // 显示答题面板
        gameObject.SetActive(true);

        // 加载第一题
        LoadCurrentQuestion();
    }

    /// <summary>
    /// 加载当前索引对应的题目
    /// </summary>
    private void LoadCurrentQuestion()
    {
        // 重置UI状态
        ResultTipText.text = "";
        NextQuestionBtn.gameObject.SetActive(false);
        SubmitAnswerBtn.gameObject.SetActive(true);
        HideAllAnswerArea();

        // 检查是否答完所有题目
        if (IsCurrentKPAnswerFinished())
        {
            QuestionTitleText.text = $"🎉 知识点【{_currentKnowledge.content}】的题目已全部答完！";
            SubmitAnswerBtn.gameObject.SetActive(false);
            return;
        }

        // 获取当前题目并显示
        _currentQuestion = _currentQuestions[_currentQuestionIndex];

        // 根据题目类型显示对应的UI
        switch (_currentQuestion.Type) // 直接访问 Type 属性
        {
            case QuestionType.Choice:
                ShowChoiceQuestion(_currentQuestion as ChoiceQuestion);
                break;
            case QuestionType.TrueFalse:
                ShowTrueFalseQuestion(_currentQuestion as TrueFalseQuestion);
                break;
            case QuestionType.FillBlank:
                ShowFillBlankQuestion(_currentQuestion as FillBlankQuestion);
                break;
        }
    }
    #endregion

    #region 题型显示逻辑
    /// <summary>
    /// 显示选择题
    /// </summary>
    private void ShowChoiceQuestion(ChoiceQuestion q)
    {
        if (q == null) return;

        ChoiceAnswerArea.SetActive(true);
        QuestionTypeText.text = "选择题";
        QuestionTitleText.text = q.title;

        // 整理选项并打乱顺序
        List<string> allOptions = new List<string>();
        if (q.wrongAnswers != null)
            allOptions.AddRange(q.wrongAnswers);
        allOptions.Add(q.correctAnswer);
        ShuffleList(allOptions);

        // 填充选项文本（最多显示4个选项）
        for (int i = 0; i < ChoiceOptionTexts.Count; i++)
        {
            if (i < allOptions.Count)
            {
                ChoiceOptionTexts[i].text = $"{(char)('A' + i)}. {allOptions[i]}";
                ChoiceOptionTexts[i].transform.parent.gameObject.SetActive(true);
                // 存储选项内容到Toggle的name属性，方便后续判断
                ChoiceOptionTexts[i].transform.parent.GetComponent<Toggle>().name = allOptions[i];
            }
            else
            {
                ChoiceOptionTexts[i].transform.parent.gameObject.SetActive(false);
            }
        }

        // 重置Toggle选中状态
        ChoiceToggleGroup.SetAllTogglesOff();
    }

    /// <summary>
    /// 显示判断题
    /// </summary>
    private void ShowTrueFalseQuestion(TrueFalseQuestion q)
    {
        if (q == null) return;

        TrueFalseAnswerArea.SetActive(true);
        QuestionTypeText.text = "判断题";
        QuestionTitleText.text = q.title;

        // 重置Toggle选中状态
        TrueToggle.isOn = false;
        FalseToggle.isOn = false;
    }

    /// <summary>
    /// 显示填空题
    /// </summary>
    private void ShowFillBlankQuestion(FillBlankQuestion q)
    {
        if (q == null) return;

        FillBlankAnswerArea.SetActive(true);
        QuestionTypeText.text = "填空题";
        QuestionTitleText.text = q.title;

        // 重置输入框
        FillBlankInput.text = "";
        FillBlankInput.ActivateInputField();
    }

    /// <summary>
    /// 隐藏所有答题区域
    /// </summary>
    private void HideAllAnswerArea()
    {
        ChoiceAnswerArea.SetActive(false);
        TrueFalseAnswerArea.SetActive(false);
        FillBlankAnswerArea.SetActive(false);
    }

    /// <summary>
    /// 打乱列表顺序（用于选择题选项随机）
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
    #endregion

    #region 答案提交与判断
    /// <summary>
    /// 提交答案按钮点击事件
    /// </summary>
    private void OnSubmitAnswer()
    {
        if (_currentQuestion == null) return;

        bool isCorrect = false;
        string userAnswer = "";
        string correctAnswer = _currentQuestion.correctAnswer;

        // 根据题型获取用户答案并判断对错
        switch (_currentQuestion.Type)
        {
            case QuestionType.Choice:
                var selectedToggle = ChoiceToggleGroup.ActiveToggles().FirstOrDefault();
                if (selectedToggle == null)
                {
                    ResultTipText.text = "⚠️ 请选择一个选项！";
                    return;
                }
                userAnswer = selectedToggle.name;
                isCorrect = userAnswer == correctAnswer;
                break;

            case QuestionType.TrueFalse:
                if (TrueToggle.isOn) userAnswer = "正确";
                else if (FalseToggle.isOn) userAnswer = "错误";
                else
                {
                    ResultTipText.text = "⚠️ 请选择'正确'或'错误'！";
                    return;
                }
                isCorrect = userAnswer == correctAnswer;
                break;

            case QuestionType.FillBlank:
                userAnswer = FillBlankInput.text.Trim();
                // 填空题容错：忽略空格、顿号/逗号替换、大小写
                string userAnswerClean = userAnswer.Replace(" ", "").Replace("、", ",").ToLower();
                string correctAnswerClean = correctAnswer.Replace(" ", "").Replace("、", ",").ToLower();
                isCorrect = userAnswerClean == correctAnswerClean;
                break;
        }

        // 显示答题结果
        ShowAnswerResult(isCorrect, correctAnswer);

        // 记录用户答案
        _currentQuestion.userAnswer = userAnswer;

        // 复习模式：自动切题；普通模式：显示下一题按钮
        if (_isReviewMode)
        {
            SubmitAnswerBtn.gameObject.SetActive(false);
            // 触发单题完成回调（通知复习队列统计对错）
            _onQuestionAnswerComplete?.Invoke(isCorrect);
            // 延迟1秒自动切题（给用户看结果的时间）
            Invoke(nameof(OnNextQuestion), 1f);
        }
        else
        {
            NextQuestionBtn.gameObject.SetActive(true);
            SubmitAnswerBtn.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 显示答题结果
    /// </summary>
    private void ShowAnswerResult(bool isCorrect, string correctAnswer)
    {
        if (isCorrect)
        {
            ResultTipText.text = $"✅ 回答正确！";
            ResultTipText.color = Color.green;
        }
        else
        {
            ResultTipText.text = $"❌ 回答错误！正确答案：{correctAnswer}";
            ResultTipText.color = Color.red;
        }
    }

    /// <summary>
    /// 下一题按钮点击事件
    /// </summary>
    private void OnNextQuestion()
    {
        _currentQuestionIndex++;
        LoadCurrentQuestion();
    }
    #endregion
}
