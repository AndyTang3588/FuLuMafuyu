#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace SuzuFactory.Alterith.Localization
{
    public class LanguageManager
    {
        private static LanguageManager instance;

        public static LanguageManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LanguageManager();
                }

                return instance;
            }
        }

        private Dictionary<string, LocalizationData> languages = new Dictionary<string, LocalizationData>();
        private string currentLanguageCode;
        private const string PrefsKey = "AlterithLanguage";
        private const string DefaultLanguageCode = "en-US";

        private LanguageManager()
        {
            LoadLanguages();
            LoadSavedLanguage();
        }

        private void LoadLanguages()
        {
            string languagesPath = AlterithUtil.GetScriptRelativePath(GetType(), "..", "..", "Languages");

            if (Directory.Exists(languagesPath))
            {
                var languageFiles = Directory.GetFiles(languagesPath, "*.json");

                foreach (string file in languageFiles)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        LocalizationData data = LocalizationData.FromJson(json);
                        languages[data.languageCode] = data;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to load language file: {file}. Error: {e.Message}");
                    }
                }
            }

            VerifyLanguageKeyConsistency();
        }

        private void LoadSavedLanguage()
        {
            string savedLanguage = EditorPrefs.GetString(PrefsKey, "");

            if (string.IsNullOrEmpty(savedLanguage))
            {
                savedLanguage = GetSystemLanguageCode();
            }

            if (languages.ContainsKey(savedLanguage))
            {
                currentLanguageCode = savedLanguage;
            }
            else
            {
                currentLanguageCode = DefaultLanguageCode;
            }
        }

        private string GetSystemLanguageCode()
        {
            SystemLanguage systemLanguage = Application.systemLanguage;
            string languageCode = SystemLanguageToISOCode(systemLanguage);

            if (languages.ContainsKey(languageCode))
            {
                return languageCode;
            }

            return DefaultLanguageCode;
        }

        private string SystemLanguageToISOCode(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.Japanese:
                    return "ja-JP";
                case SystemLanguage.English:
                    return "en-US";
                case SystemLanguage.French:
                    return "fr-FR";
                case SystemLanguage.Spanish:
                    return "es-ES";
                case SystemLanguage.German:
                    return "de-DE";
                case SystemLanguage.Italian:
                    return "it-IT";
                case SystemLanguage.Portuguese:
                    return "pt-BR";
                case SystemLanguage.Russian:
                    return "ru-RU";
                case SystemLanguage.Korean:
                    return "ko-KR";
                case SystemLanguage.ChineseSimplified:
                    return "zh-CN";
                case SystemLanguage.ChineseTraditional:
                    return "zh-TW";
                default:
                    return DefaultLanguageCode;
            }
        }

        public void SaveCurrentLanguage()
        {
            EditorPrefs.SetString(PrefsKey, currentLanguageCode);
        }

        public string GetString(string key)
        {
            if (languages.TryGetValue(currentLanguageCode, out LocalizationData data))
            {
                string result = data.GetString(key);

                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }

            return key;
        }

        public void SetLanguage(string languageCode)
        {
            if (languages.ContainsKey(languageCode))
            {
                currentLanguageCode = languageCode;
                SaveCurrentLanguage();
            }
        }

        public string[] GetAvailableLanguageCodes()
        {
            string[] codes = new string[languages.Count];
            languages.Keys.CopyTo(codes, 0);
            return codes;
        }

        public string[] GetAvailableLanguageNames()
        {
            string[] names = new string[languages.Count];
            int i = 0;

            foreach (var language in languages.Values)
            {
                names[i++] = language.languageName;
            }

            return names;
        }

        public string GetCurrentLanguageCode()
        {
            return currentLanguageCode;
        }

        public int GetCurrentLanguageIndex()
        {
            string[] codes = GetAvailableLanguageCodes();

            for (int i = 0; i < codes.Length; i++)
            {
                if (codes[i] == currentLanguageCode)
                {
                    return i;
                }
            }

            return 0;
        }

        private void VerifyLanguageKeyConsistency()
        {
            if (languages.Count <= 1)
            {
                return;
            }

            HashSet<string> referenceKeys = new HashSet<string>();
            string referenceLanguage = "";

            foreach (var language in languages.Values)
            {
                if (language.languageCode == DefaultLanguageCode)
                {
                    referenceLanguage = language.languageCode;

                    foreach (var entry in language.entries)
                    {
                        referenceKeys.Add(entry.key);
                    }

                    break;
                }
            }

            if (referenceKeys.Count == 0)
            {
                var firstLanguage = languages.Values.GetEnumerator();
                firstLanguage.MoveNext();
                referenceLanguage = firstLanguage.Current.languageCode;

                foreach (var entry in firstLanguage.Current.entries)
                {
                    referenceKeys.Add(entry.key);
                }
            }

            foreach (var language in languages.Values)
            {
                if (language.languageCode == referenceLanguage)
                {
                    continue;
                }

                HashSet<string> languageKeys = new HashSet<string>();

                foreach (var entry in language.entries)
                {
                    languageKeys.Add(entry.key);
                }

                HashSet<string> missingKeys = new HashSet<string>(referenceKeys);
                missingKeys.ExceptWith(languageKeys);

                HashSet<string> extraKeys = new HashSet<string>(languageKeys);
                extraKeys.ExceptWith(referenceKeys);

                if (missingKeys.Count > 0 || extraKeys.Count > 0)
                {
                    if (missingKeys.Count > 0)
                    {
                        Debug.LogWarning($"Language {language.languageCode} is missing {missingKeys.Count} keys that are in {referenceLanguage}:");

                        foreach (string key in missingKeys)
                        {
                            Debug.LogWarning($"  - {key}");
                        }
                    }

                    if (extraKeys.Count > 0)
                    {
                        Debug.LogWarning($"Language {language.languageCode} has {extraKeys.Count} extra keys that are not in {referenceLanguage}:");

                        foreach (string key in extraKeys)
                        {
                            Debug.LogWarning($"  - {key}");
                        }
                    }
                }
            }
        }
    }
}

#endif
