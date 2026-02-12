using System;
using System.Collections.Generic;
using UnityEngine;

// 题目接口（统一所有题型的核心属性和行为）
[Serializable]  // 关键：标记为可序列化
public abstract class QuestionBase  // 抽象基类
{
    // 1. 所有子类共有的属性
    [SerializeField]  // 让Unity序列化这些字段
    public string knowledgeId;

    [SerializeField]
    public string title;

    [SerializeField]
    public string correctAnswer;

    [SerializeField]
    public string userAnswer;

    // 2. 抽象属性 - 子类必须实现
    public abstract QuestionType Type { get; }  // 抽象属性

    // 3. 具体方法 - 所有子类共享
    public virtual string ToJson()
    {
        return JsonUtility.ToJson(this);  // 直接序列化自己
    }

    public virtual void FromJson(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);  //  直接反序列化
    }

    // 4. 判断答案是否正确
    public bool IsCorrect()
    {
        return userAnswer == correctAnswer;
    }
}

public enum QuestionType
{
    FillBlank,
    Choice,
    TrueFalse

}




