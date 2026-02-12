using System;
using UnityEngine;

[Serializable]
public class FillBlankQuestion : QuestionBase
{
    // === 填空题特有属性 ===
    [SerializeField]
    public string blankKeyword;

    // === 重写抽象属性 ===
    public override QuestionType Type
    {
        get { return QuestionType.FillBlank; }
    }

    // === 构造函数 ===
    public FillBlankQuestion()
    {
        // 基类构造函数已自动调用
    }

    // === 填空题特有方法 ===
    public bool ContainsKeyword(string answer)
    {
        return answer?.ToLower().Contains(blankKeyword?.ToLower()) == true;
    }

    // === 重写IsCorrect方法（填空题特殊处理）===
    public new bool IsCorrect()
    {
        if (string.IsNullOrEmpty(userAnswer) || string.IsNullOrEmpty(correctAnswer))
            return false;

        // 填空题容错处理
        string userClean = userAnswer.Replace(" ", "").Replace("、", ",").ToLower();
        string correctClean = correctAnswer.Replace(" ", "").Replace("、", ",").ToLower();
        return userClean == correctClean;
    }
}
