#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuzuFactory.Alterith.Localization
{
    [Serializable]
    public class LocalizationData
    {
        [Serializable]
        public class StringEntry
        {
            public string key;
            public string value;

            public StringEntry(string key, string value)
            {
                this.key = key;
                this.value = value;
            }
        }

        public string languageCode;
        public string languageName;
        public List<StringEntry> entries = new List<StringEntry>();

        public LocalizationData(string languageCode, string languageName)
        {
            this.languageCode = languageCode;
            this.languageName = languageName;
        }

        public void AddEntry(string key, string value)
        {
            entries.Add(new StringEntry(key, value));
        }

        public string GetString(string key)
        {
            foreach (var entry in entries)
            {
                if (entry.key == key)
                {
                    return entry.value;
                }
            }

            return key;
        }

        public static LocalizationData FromJson(string json)
        {
            return JsonUtility.FromJson<LocalizationData>(json);
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }
    }
}

#endif
