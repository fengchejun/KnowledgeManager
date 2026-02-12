//using UnityEngine;
//using System.Collections.Generic;


//// 验证知识点和题目是否成功保存/加载
//public class VerifyQuestionSave : MonoBehaviour
//{
//    private void Start()
//    {
//        // 启动时自动验证
//        VerifyAllKnowledgePointsWithQuestions();
//    }

//    // 核心方法：验证所有知识点的题目
//    private void VerifyAllKnowledgePointsWithQuestions()
//    {
//        // 获取所有知识点
//        var allKP = KnowledgeManager.Instance.GetAllKnowledgePoints();

//        // 1. 检查是否有知识点
//        if (allKP.Count == 0)
//        {
//            Debug.LogWarning("?? 本地无任何知识点数据，请先新增知识点！");
//            return;
//        }

//        // 2. 遍历每个知识点，验证题目
//        Debug.Log($"=== 开始验证所有知识点（共{allKP.Count}个）===");
//        foreach (var kp in allKP)
//        {
//            Debug.Log($"\n?? 知识点内容：{kp.content}");
//            Debug.Log($"?? 知识点ID：{kp.id}");

//            // 检查题目列表
//            if (kp.Questions == null || kp.Questions.Count == 0)
//            {
//                Debug.Log($"? 该知识点无题目数据！");
//                continue;
//            }

//            // 验证题目数量（预期3种题型）
//            if (kp.Questions.Count == 3)
//            {
//                Debug.Log($"? 题目数量正确（3种题型）");
//                // 打印每种题目的详情
//                foreach (var q in kp.Questions)
//                {
//                    Debug.Log($"  ├─ {q.Type}：{q.Title}");
//                    Debug.Log($"  │  └─ 正确答案：{q.CorrectAnswer}");
//                    // 选择题额外打印错误选项
//                    if (q is ChoiceQuestion choiceQ)
//                    {
//                        Debug.Log($"  │     错误选项：{string.Join("、", choiceQ.WrongAnswers)}");
//                    }
//                }
//            }
//            else
//            {
//                Debug.LogError($"? 题目数量错误！预期3个，实际{kp.Questions.Count}个");
//                // 打印现有题目（方便排查）
//                foreach (var q in kp.Questions)
//                {
//                    Debug.Log($"  ├─ 现有题目：{q.Type} - {q.Title}");
//                }
//            }
//        }

//        Debug.Log($"\n=== 验证完成 ===");
//    }

//    // 可选：手动触发验证（比如绑定到UI按钮）
//    public void ManualVerify()
//    {
//        Debug.Log("\n=== 手动触发验证 ===");
//        VerifyAllKnowledgePointsWithQuestions();
//    }
//}
