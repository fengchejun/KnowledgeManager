using System;
using UnityEngine;

[Serializable]
public class TrueFalseQuestion : QuestionBase
{
    // === 重写抽象属性 ===
    public override QuestionType Type
    {
        get { return QuestionType.TrueFalse; }
    }

    // === 构造函数 ===
    public TrueFalseQuestion()
    {
        // 基类构造函数已自动调用
    }

    // === 判断题特有方法 ===
    public bool IsValidAnswer()
    {
        return userAnswer == "正确" || userAnswer == "错误";
    }

    // === 重写IsCorrect方法（判断题特殊处理）===
    public new bool IsCorrect()
    {
        return userAnswer == correctAnswer;
    }
}
