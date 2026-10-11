using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WarWW3
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        public bool startInBattle;
        IEnumerator Start()
        {
            Time.timeScale = 1;
            GraphicsOptions.Apply(PlayerPrefs.GetInt("graphics", 1));
            var locale = gameObject.AddComponent<GameLocalizer>();
            locale.LoadFallback();
            var ui = gameObject.AddComponent<GameUI>();
            ui.Initialize(locale);
            var camera = new GameObject("RTS Camera", typeof(Camera), typeof(AudioListener), typeof(RtsCamera)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.backgroundColor = new Color(0.055f, 0.09f, 0.12f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.nearClipPlane = 0.2f; camera.farClipPlane = 260;
            var controller = camera.GetComponent<RtsCamera>();
            RuntimeWorld.Create();
            if (startInBattle)
            {
                var battle = gameObject.AddComponent<BattleController>();
                battle.Initialize(camera, ui, locale);
                ui.ShowBattle(battle);
                controller.CanControl = () => !ui.BlocksWorld;
            }
            else
            {
                controller.CanControl = () => false;
                ui.ShowMenu();
            }
            yield return locale.InitializeUnityLocalization();
        }
        public static void StartBattle() { Time.timeScale = 1; SceneManager.LoadScene("Battlefield"); }
        public static void MainMenu() { Time.timeScale = 1; SceneManager.LoadScene("MainMenu"); }
        void OnDestroy() { Time.timeScale = 1; VisualPalette.Dispose(); }
    }
}
