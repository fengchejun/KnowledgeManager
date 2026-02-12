using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChoiceQuestion : QuestionBase
{
    // === 选择题特有属性 ===
    [SerializeField]
    public List<string> wrongAnswers = new List<string>();

    // === 重写抽象属性 ===
    public override QuestionType Type
    {
        get { return QuestionType.Choice; }
    }

    // === 构造函数 ===
    public ChoiceQuestion()
    {
        // 基类构造函数已自动调用
    }

    // === 选择题特有方法 ===
    public List<string> GetAllOptions()
    {
        List<string> options = new List<string>(wrongAnswers);
        options.Add(correctAnswer);
        return options;
    }

    public void AddWrongAnswer(string answer)
    {
        if (!wrongAnswers.Contains(answer))
            wrongAnswers.Add(answer);
    }

    // === 重写序列化方法（可选）===
    public override string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public override void FromJson(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
    }
}
