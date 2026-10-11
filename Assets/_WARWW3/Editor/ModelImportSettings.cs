using UnityEditor;

namespace WarWW3.Editor
{
    public sealed class ModelImportSettings : AssetPostprocessor
    {
        void OnPreprocessModel()
        {
            if (!assetPath.StartsWith("Assets/_WARWW3/Models/")) return;
            var model = (ModelImporter)assetImporter;
            model.importAnimation = false; model.importCameras = false; model.importLights = false;
            model.materialImportMode = ModelImporterMaterialImportMode.None; model.isReadable = false;
            model.meshCompression = ModelImporterMeshCompression.Low; model.globalScale = 1;
        }
    }
}
