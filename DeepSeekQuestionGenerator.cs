using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class DeepSeekQuestionGenerator : MonoBehaviour
{
    public static DeepSeekQuestionGenerator Instance;

    // API配置（移除空格）
    private const string DEEPSEEK_API_KEY = "sk-7b0de10e33124631929cf34351ff9d6e";

    // 多个备用方案（移除所有空格）
    private const string DEEPSEEK_API_URL = "https://api.deepseek.com/v1/chat/completions";
    private const string DEEPSEEK_API_IP = "8.219.83.66"; // 主IP
    private const string DEEPSEEK_API_URL_IP = "https://8.219.83.66/v1/chat/completions";
    private const string DEEPSEEK_API_URL_AI = "https://api.deepseek.ai/v1/chat/completions";
    private const string DEEPSEEK_API_URL_CHAT = "https://api.deepseek-chat.com/v1/chat/completions";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 防止场景切换丢失
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 为知识点生成三种题目（修复版）
    public IEnumerator GenerateQuestionsForKnowledge(KnowledgePoint kp, Action<bool> onComplete)
    {
        Debug.Log("=== 开始生成题目（修复版）===");

        // 1. 获取可用的API端点
        string availableEndpoint = GetCurrentEndpoint();
        Debug.Log($"使用端点：{availableEndpoint}");

        // 2. 构造Prompt
        string prompt = BuildPrompt(kp.content);

        // 3. 发送请求
        yield return StartCoroutine(SendRequestWithRetry(availableEndpoint, prompt, kp, onComplete));
    }

    // 获取当前可用的端点
    private string GetCurrentEndpoint()
    {
        // 优先使用IP地址方案（绕过DNS）
        return DEEPSEEK_API_URL;
    }

    // 构建Prompt
    private string BuildPrompt(string knowledgeContent)
    {
        return $@"请仅输出JSON格式的题目，不要任何多余内容，不要加代码块标记：
知识点：{knowledgeContent}
JSON结构必须严格符合：
{{
  ""choiceQuestion"": {{
    ""title"": ""选择题题目"",
    ""correctAnswer"": ""正确答案"",
    ""wrongAnswers"": [""错误选项1"",""错误选项2"",""错误选项3""]
  }},
  ""trueFalseQuestion"": {{
    ""title"": ""判断题题目"",
    ""correctAnswer"": ""正确""或""错误""
  }},
  ""fillBlankQuestion"": {{
    ""title"": ""填空题题目，包含___"",
    ""correctAnswer"": ""填空答案"",
    ""blankKeyword"": ""填空关键词""
  }}
}}";
    }

    // 带重试的请求发送
    private IEnumerator SendRequestWithRetry(string endpoint, string prompt, KnowledgePoint kp, Action<bool> onComplete)
    {
        int maxRetries = 3;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            Debug.Log($"尝试第 {attempt} 次请求...");

            bool success = false;
            yield return StartCoroutine(SendSingleRequest(endpoint, prompt, kp,
                () => { success = true; },
                error => { Debug.Log($"第{attempt}次失败：{error}"); }));

            if (success)
            {
                onComplete?.Invoke(true);
                yield break;
            }

            if (attempt < maxRetries)
            {
                Debug.Log($"等待 {attempt * 2} 秒后重试...");
                yield return new WaitForSeconds(attempt * 2);

                // 尝试切换端点
                endpoint = SwitchEndpoint(endpoint);
            }
        }

        Debug.LogError("所有重试都失败了");
        onComplete?.Invoke(false);
    }

    // 发送单个请求
    private IEnumerator SendSingleRequest(string endpoint, string prompt, KnowledgePoint kp, Action onSuccess, Action<string> onError)
    {
        // 清理API Key
        string cleanApiKey = DEEPSEEK_API_KEY.Trim()
            .Replace("\n", "")
            .Replace("\r", "")
            .Replace("\t", "")
            .Replace(" ", "");

        if (string.IsNullOrEmpty(cleanApiKey) || !cleanApiKey.StartsWith("sk-"))
        {
            onError?.Invoke("API Key格式错误");
            yield break;
        }

        // 1. 构建正确的请求数据模型（可序列化）
        var requestData = new DeepSeekRequestData
        {
            model = "deepseek-chat",
            messages = new List<DeepSeekMessage>
            {
                new DeepSeekMessage { role = "user", content = prompt }
            },
            temperature = 0.7f,
            max_tokens = 1000,
            stream = false
        };

        // 2. 使用JsonUtility正确序列化
        string requestJson = JsonUtility.ToJson(requestData);
        if (string.IsNullOrEmpty(requestJson))
        {
            onError?.Invoke("请求数据序列化失败");
            yield break;
        }

        using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(requestJson);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // 设置请求头
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + cleanApiKey);

            // 如果是IP地址，需要添加Host头
            if (endpoint.Contains(DEEPSEEK_API_IP))
            {
                request.SetRequestHeader("Host", "api.deepseek.com");
                // 跳过SSL证书验证（IP直连时需要）
#if UNITY_EDITOR || UNITY_STANDALONE
                request.certificateHandler = new BypassCertificate();
#endif
            }

            request.timeout = 30;

            Debug.Log($"发送请求到：{endpoint}");
            Debug.Log($"请求体：{requestJson}");

            yield return request.SendWebRequest();

            // 清理证书处理器
            if (request.certificateHandler != null)
            {
                request.certificateHandler.Dispose();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ 请求成功");
                Debug.Log($"响应：{request.downloadHandler.text}");

                // 处理响应（修复了解析逻辑）
                if (ProcessResponse(request.downloadHandler.text, kp))
                {
                    onSuccess?.Invoke();
                }
                else
                {
                    onError?.Invoke("响应处理失败");
                }
            }
            else
            {
                string errorMsg = $"请求失败：{request.error}";
                if (request.responseCode != 0)
                {
                    errorMsg += $"，响应码：{request.responseCode}";
                }
                onError?.Invoke(errorMsg);
            }
        }
    }

    // 切换端点
    private string SwitchEndpoint(string currentEndpoint)
    {
        if (currentEndpoint == DEEPSEEK_API_URL_IP)
            return DEEPSEEK_API_URL_AI;
        else if (currentEndpoint == DEEPSEEK_API_URL_AI)
            return DEEPSEEK_API_URL_CHAT;
        else
            return DEEPSEEK_API_URL_IP;
    }

    // 处理响应（修复了解析逻辑）
    private bool ProcessResponse(string responseText, KnowledgePoint kp)
    {
        try
        {
            // 1. 先解析DeepSeek的标准响应
            var apiResponse = JsonUtility.FromJson<DeepSeekApiResponse>(responseText);
            if (apiResponse == null || apiResponse.choices == null || apiResponse.choices.Count == 0)
            {
                Debug.LogError("API响应格式错误，无choices数据");
                return false;
            }

            // 2. 获取AI返回的题目JSON内容并彻底清理
            string questionsJson = apiResponse.choices[0].message.content.Trim();
            Debug.Log($"AI返回的原始内容：{questionsJson}");

            // ========== 关键修复：清理所有非JSON字符 ==========
            // 移除```json开头标记
            questionsJson = questionsJson.Replace("```json", "").Replace("```JSON", "");
            // 移除末尾的```标记
            questionsJson = questionsJson.Replace("```", "");
            // 移除所有换行/制表符（可选，增强兼容性）
            questionsJson = questionsJson.Trim().Replace("\n", "").Replace("\t", "");
            // =================================================

            Debug.Log($"清理后的题目JSON：{questionsJson}");

            // 3. 解析题目JSON
            var questionsData = JsonUtility.FromJson<QuestionsData>(questionsJson);
            if (questionsData == null)
            {
                Debug.LogError("题目JSON解析失败");
                return false;
            }

            // 后续逻辑保持不变...
            // 4. 构建题目列表
            List<QuestionBase> questions = new List<QuestionBase>();

            // 选择题
            if (questionsData.choiceQuestion != null)
            {
                questions.Add(new ChoiceQuestion
                {
                    knowledgeId = kp.id,
                    title = questionsData.choiceQuestion.title,
                    correctAnswer = questionsData.choiceQuestion.correctAnswer,
                    wrongAnswers = questionsData.choiceQuestion.wrongAnswers
                });
            }

            // 判断题
            if (questionsData.trueFalseQuestion != null)
            {
                questions.Add(new TrueFalseQuestion
                {
                    knowledgeId = kp.id,
                    title = questionsData.trueFalseQuestion.title,
                    correctAnswer = questionsData.trueFalseQuestion.correctAnswer
                });
            }

            // 填空题
            if (questionsData.fillBlankQuestion != null)
            {
                questions.Add(new FillBlankQuestion
                {
                    knowledgeId = kp.id,
                    title = questionsData.fillBlankQuestion.title,
                    correctAnswer = questionsData.fillBlankQuestion.correctAnswer,
                    blankKeyword = questionsData.fillBlankQuestion.blankKeyword
                });
            }

            // 验证题目数量
            if (questions.Count == 3)
            {
                kp.Questions = questions;
                Debug.Log($"✅ 为知识点生成题目成功：{questions.Count}题");
                return true;
            }
            else
            {
                Debug.LogError($"题目数量错误：{questions.Count}/3");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"处理响应异常：{e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    #region 数据模型（完整可序列化）
    // DeepSeek API请求模型
    [Serializable]
    private class DeepSeekRequestData
    {
        public string model;
        public List<DeepSeekMessage> messages;
        public float temperature;
        public int max_tokens;
        public bool stream;
    }

    [Serializable]
    private class DeepSeekMessage
    {
        public string role;
        public string content;
    }

    // DeepSeek API响应模型
    [Serializable]
    private class DeepSeekApiResponse
    {
        public string id;
        public string _object; // 避开关键字object
        public long created;
        public string model;
        public List<DeepSeekChoice> choices;
        public DeepSeekUsage usage;
    }

    [Serializable]
    private class DeepSeekChoice
    {
        public int index;
        public DeepSeekMessage message;
        public string finish_reason;
    }

    [Serializable]
    private class DeepSeekUsage
    {
        public int prompt_tokens;
        public int completion_tokens;
        public int total_tokens;
    }

    // 题目数据模型
    [Serializable]
    private class QuestionsData
    {
        public ChoiceQuestionData choiceQuestion;
        public TrueFalseQuestionData trueFalseQuestion;
        public FillBlankQuestionData fillBlankQuestion;
    }

    [Serializable]
    private class ChoiceQuestionData
    {
        public string title;
        public string correctAnswer;
        public List<string> wrongAnswers;
    }

    [Serializable]
    private class TrueFalseQuestionData
    {
        public string title;
        public string correctAnswer;
    }

    [Serializable]
    private class FillBlankQuestionData
    {
        public string title;
        public string correctAnswer;
        public string blankKeyword;
    }
    #endregion

    #region 辅助类
    // 跳过SSL证书验证
    private class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
    #endregion

    
}