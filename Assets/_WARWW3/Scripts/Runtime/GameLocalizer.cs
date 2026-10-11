using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace WarWW3
{
    [Serializable] public sealed class TranslationRow { public string key, en, th; }
    [Serializable] public sealed class TranslationCatalog { public TranslationRow[] entries; }

    public sealed class GameLocalizer : MonoBehaviour
    {
        readonly Dictionary<string, TranslationRow> fallback = new Dictionary<string, TranslationRow>();
        StringTable table;
        bool ready;
        public string Language { get; private set; }
        public event Action Changed;
        public void LoadFallback()
        {
            var asset = Resources.Load<TextAsset>("Localization/UI");
            if (asset == null) throw new InvalidOperationException("Missing Resources/Localization/UI.json");
            var data = JsonUtility.FromJson<TranslationCatalog>(asset.text);
            foreach (var row in data.entries) fallback.Add(row.key, row);
            Language = PlayerPrefs.GetString("language", Application.systemLanguage == SystemLanguage.Thai ? "th-TH" : "en-US");
            if (Language != "th-TH" && Language != "en-US") Language = "en-US";
        }
        public IEnumerator InitializeUnityLocalization()
        {
            yield return LocalizationSettings.InitializationOperation;
            ready = true;
            yield return LoadTable();
        }
        public void ToggleLanguage()
        {
            Language = Language == "en-US" ? "th-TH" : "en-US";
            PlayerPrefs.SetString("language", Language); PlayerPrefs.Save(); table = null;
            if (Changed != null) Changed();
            if (ready) StartCoroutine(LoadTable());
        }
        IEnumerator LoadTable()
        {
            string requested = Language;
            var locale = LocalizationSettings.AvailableLocales.GetLocale(requested);
            if (locale == null) yield break;
            LocalizationSettings.SelectedLocale = locale;
            var operation = LocalizationSettings.StringDatabase.GetTableAsync("UI", locale);
            yield return operation;
            if (requested != Language) yield break;
            table = operation.Result;
            if (Changed != null) Changed();
        }
        public string Text(string key, params object[] args)
        {
            string value = null;
            if (table != null) { var entry = table.GetEntry(key); if (entry != null) value = entry.LocalizedValue; }
            TranslationRow row;
            if (string.IsNullOrEmpty(value) && fallback.TryGetValue(key, out row)) value = Language == "th-TH" && !string.IsNullOrEmpty(row.th) ? row.th : row.en;
            if (value == null) value = "[" + key + "]";
            return args.Length == 0 ? value : string.Format(value, args);
        }
    }
}
