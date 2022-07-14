using DCL;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.Helpers;
using UnityEngine;

namespace ECSSystems.CameraSystem
{
    public static class ECSCameraSystem
    {
        private static readonly DataStore_Camera dataStoreCamera = DataStore.i.camera;

        public static void Update()
        {
            if (!CommonScriptableObjects.rendererState.Get())
                return;

            Transform cameraT = dataStoreCamera.transform.Get();

            UnityEngine.Vector3 cameraPosition = cameraT.position;
            Quaternion cameraRotation = cameraT.rotation;
            UnityEngine.Vector3 worldOffset = CommonScriptableObjects.worldOffset;

            var loadedScenes = ReferencesContainer.loadedScenes;
            var componentsWriter = ReferencesContainer.componentsWriter;

            IParcelScene scene;
            for (int i = 0; i < loadedScenes.Count; i++)
            {
                scene = loadedScenes[i];

                var transform = TransformHelper.SetTransform(scene, ref cameraPosition, ref cameraRotation, ref worldOffset);
                componentsWriter.PutComponent(scene.sceneData.id, SpecialEntityId.CAMERA_ENTITY, ComponentID.TRANSFORM,
                    transform, ECSComponentWriteType.SEND_TO_SCENE);
            }
        }
    }
}