using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WarWW3
{
    public static class GraphicsOptions
    {
        public static void Apply(int preset)
        {
            preset = Mathf.Clamp(preset, 0, 2);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = preset == 0 ? 30 : 60;
            QualitySettings.shadows = preset == 0 ? ShadowQuality.Disable : ShadowQuality.HardOnly;
            QualitySettings.shadowDistance = preset == 0 ? 0 : 100;
            QualitySettings.lodBias = preset == 0 ? 0.65f : 1;
            var pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (pipeline != null)
            {
                pipeline.renderScale = preset == 0 ? 0.75f : 1;
                pipeline.msaaSampleCount = preset == 2 ? 4 : 1;
                pipeline.shadowDistance = preset == 0 ? 0 : 100;
            }
            PlayerPrefs.SetInt("graphics", preset); PlayerPrefs.Save();
        }
    }
}
