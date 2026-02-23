using System.Text;

using Jint;
using Jint.Native;
using ReportsWebApp.DB.Models;
using System.Text.RegularExpressions;

namespace ReportsWebApp.DB.Services
{
    public class ConditionalEvaluator
    {
        private Dictionary<string, string> SanitizedKeysMap;
        private Engine engine;
        private readonly object lockObject = new object(); // Lock object for thread safety

        public TemplateEvaluation ProcessConditionals(string template)
        {

            try
            {
                var tokens = ParseConditionals(template);

                foreach (var baseToken in tokens)
                {
                    var token = ReplaceKeys(baseToken.Substring(2, baseToken.Length - 4));
                    JsValue conditionResult = engine.Evaluate(token);
                    template = template.Replace(baseToken, conditionResult.ToString());
                }

                return new TemplateEvaluation
                {
                    Success = true,
                    Result = template
                };
            }
            catch (Exception e)
            {
                return new TemplateEvaluation
                {
                    Success = false,
                    Message = $"There is an error in the provided code '{template}': {e.Message}"
                };
            }
        }

        // Find any tokens surrounded by double braces, e.g. {{ DashtPlanning.JobName }}
        public List<string> ParseConditionals(string template)
        {
            int level = 0;
            StringBuilder sb = new StringBuilder();
            List<string> tokens = new();
            for (var i = 0; i < template.Length; i++)
            {
                if (template[i] == '{' && template[i + 1] == '{')
                {
                    level++;
                    i += 2;
                    sb.Clear();
                    sb.Append("{{");
                }

                if (template[i] == '}' && template[i + 1] == '}')
                {
                    level--;
                    i += 2;
                    tokens.Add(sb + "}}");
                    sb.Clear();
                }

                if (level > 0)
                {
                    sb.Append(template[i]);
                }
            }
            return tokens;
        }

        public void InitializeEngine(Dictionary<string, string> data)
        {
            lock (lockObject) // Ensure thread-safe access
            {
                if (engine == null)
                    engine = new Engine();

                SanitizedKeysMap = new Dictionary<string, string>();

                if (data.Count == 0)
                {
                    Console.WriteLine("No data provided to initialize the engine.");
                    return;
                }

                var sanitizedKeys = new HashSet<string>(); // To check for unique keys

                foreach (var key in data.Keys)
                {
                    var sanitizedKey = SanitizeKey(key);
                    if (!sanitizedKeys.Add(sanitizedKey))
                    {
                        Console.WriteLine($"Duplicate sanitized key found: {sanitizedKey}");
                        continue; // Skip this key or handle duplicates appropriately
                    }

                    SanitizedKeysMap.Add(key, sanitizedKey);
                    if (!data.TryGetValue(key, out var value))
                    {
                        value = string.Empty;
                    }
                    engine.SetValue(sanitizedKey, value);
                }
            }
        }

        private string SanitizeKey(string key)
        {
            int minusCount = 0;
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] == '_' || key[i] == '.')
                {
                    minusCount++;
                }
                else if (key[i] == 'D')
                {
                    if (i + 4 <= key.Length)
                    {
                        if (key[i + 1] == 'a' &&
                            key[i + 2] == 's' &&
                            key[i + 3] == 'h' &&
                            key[i + 4] == 't') //i know this looks stupid but i dont know how the compiler will optimize a compare
                        {
                            minusCount += 5;
                        }
                    }
                }
            }

            if (minusCount == 0)
            {
                return key;
            }
            
            return string.Create(key.Length - minusCount, key, (cc, s) =>
            {
                int realIdx = 0;
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == '_' || s[i] == '.')
                    {
                        continue;
                    }

                    if (s[i] == 'D')
                    {
                        if (i + 4 <= s.Length)
                        {
                            if (s[i + 1] == 'a' &&
                                s[i + 2] == 's' &&
                                s[i + 3] == 'h' &&
                                s[i + 4] == 't')
                            {
                                i += 4;
                                continue;
                            }
                        }
                    }
                    cc[realIdx++] = s[i];
                }
            });
        }

        private string ReplaceKeys(string token)
        {
            if (SanitizedKeysMap.ContainsKey(token.Trim()))
            {
                return SanitizedKeysMap[token.Trim()];
            }

            // Fallback if token is complex
            foreach (var (originalKey, sanitizedKey) in SanitizedKeysMap)
            {
                token = token.Replace(originalKey, sanitizedKey);
            }
            return token;
        }
    }
}
