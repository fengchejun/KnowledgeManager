using System;
using System.Collections.Generic;
using UnityEngine;

public class ReviewQueueManager : MonoBehaviour
{
    public static ReviewQueueManager Instance;

    private Queue<KnowledgePoint> _reviewQueue; // 复习队列（先进先出）
    private KnowledgePoint _currentReviewKP; // 当前正在复习的知识点
    private int _correctCount; // 当前知识点答对的题目数

    private void Awake()
    {
        Instance = this;
        _reviewQueue = new Queue<KnowledgePoint>();
    }

    private void Start()
    {
        // 创建一个知识点
        var kp = new KnowledgePoint("测试内容", "测试分类", new List<string> { "标签" });

        // 测试初始状态
        Debug.Log($"初始是否需要复习：{kp.NeedReview()}"); // 应该为 false（10分钟后才需要）

        // 模拟时间流逝
        kp.nextReviewTime = DateTime.Now.AddMinutes(-1); // 设置为1分钟前
        Debug.Log($"超时后是否需要复习：{kp.NeedReview()}"); // 应该为 true

    }

    // 筛选所有待复习的知识点，加入队列
    public void RefreshReviewQueue()
    {
        _reviewQueue.Clear();
        var allKP = KnowledgeManager.Instance.GetAllKnowledgePoints();

        foreach (var kp in allKP)
        {
            if (kp.NeedReview())
            {
                _reviewQueue.Enqueue(kp); // 加入复习队列
            }
        }

        Debug.Log($"✅ 复习队列已刷新，待复习知识点数量：{_reviewQueue.Count}");
    }

    // 开始复习（点击“复习”按钮调用）
    public void StartReview()
    {
        // 先刷新队列
        RefreshReviewQueue();

        if (_reviewQueue.Count == 0)
        {
            Debug.Log("🎉 当前无需要复习的知识点！");
            // 显示提示UI（可选）
            return;
        }

        // 取出队列第一个知识点，开始答题
        _currentReviewKP = _reviewQueue.Dequeue();
        _correctCount = 0; // 重置答对计数

        // 调用答题管理器开始答题
        AnswerManager.Instance.StartReviewAnswer(_currentReviewKP, OnQuestionAnswerComplete);
    }

    // 单个题目答题完成的回调（统计答对数量）
    private void OnQuestionAnswerComplete(bool isCorrect)
    {
        if (isCorrect) _correctCount++;

        // 检查当前知识点的题目是否答完
        if (AnswerManager.Instance.IsCurrentKPAnswerFinished())
        {
            // 完成该知识点复习，更新记忆阶段
            bool isAllCorrect = _correctCount == 3; // 3题全对
            _currentReviewKP.CompleteReview(isAllCorrect);
            // 保存知识点数据
            KnowledgeManager.Instance.SaveKnowledgePoints();

            // 提示结果
            string tip = isAllCorrect
                ? $"✅ 知识点「{_currentReviewKP.content}」复习通过！"
                : $"⚠️ 知识点「{_currentReviewKP.content}」复习未通过，需重新复习！";
            Debug.Log(tip);

            // 继续下一个知识点复习
            if (_reviewQueue.Count > 0)
            {
                _currentReviewKP = _reviewQueue.Dequeue();
                _correctCount = 0;
                AnswerManager.Instance.StartReviewAnswer(_currentReviewKP, OnQuestionAnswerComplete);
            }
            else
            {
                // 复习队列为空，结束复习
                AnswerManager.Instance.EndReview();
                Debug.Log("🎉 所有知识点复习完成！");
            }
        }
    }

    // 获取复习队列数量（用于UI显示）
    public int GetReviewQueueCount()
    {
        return _reviewQueue.Count;
    }
}