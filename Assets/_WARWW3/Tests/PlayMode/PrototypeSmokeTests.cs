using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WarWW3.Tests
{
    public sealed class PrototypeSmokeTests
    {
        [UnityTest]
        public IEnumerator MenuCanSwitchLanguageAndLoadBattle()
        {
            yield return SceneManager.LoadSceneAsync("MainMenu"); yield return null; yield return null;
            var locale = Object.FindFirstObjectByType<GameLocalizer>(); Assert.IsNotNull(locale);
            string before = locale.Language; locale.ToggleLanguage(); Assert.AreNotEqual(before, locale.Language);
            Assert.IsNotEmpty(locale.Text("menu.skirmish"));
            foreach (var label in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)) Assert.IsFalse(label.text.StartsWith("["), "Missing localization: " + label.text);
            yield return SceneManager.LoadSceneAsync("Battlefield"); yield return null; yield return null;
            var battle = Object.FindFirstObjectByType<BattleController>(); Assert.IsNotNull(battle); Assert.AreEqual(14, battle.Simulation.Units.Count);
            int tick = battle.Simulation.Tick; yield return new WaitForSeconds(0.3f); Assert.Greater(battle.Simulation.Tick, tick);
            battle.Paused = true; tick = battle.Simulation.Tick; yield return new WaitForSeconds(0.2f); Assert.AreEqual(tick, battle.Simulation.Tick);
        }
    }
}
