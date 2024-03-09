using System.Collections;
using System.Runtime.CompilerServices;
using MSCLoader;
using UnityEngine;
using UnityEngine.UI;

namespace MSCSpawnMenu
{
    public class ThumbnailGenerator : MonoBehaviour
    {
        private static Camera captureCamera;
        private static GameObject GenPivot;
        private static RenderTexture genTexture = new RenderTexture(256, 256, 0);
        public static Image iconComp;
        public static ThumbnailGenerator instance;

        private void Start()
        {
            captureCamera = gameObject.transform.Find("Camera").GetComponent<Camera>();
            GenPivot = gameObject.transform.Find("ObjectPivot").gameObject;
            captureCamera.targetTexture = genTexture;
            instance = this;
        }

        public Image GenerateImage(GameObject prefab)
        {
            iconComp = null;
            captureCamera.enabled = true;

            captureCamera.targetTexture = genTexture;

            GameObject spawnedObject = Instantiate<GameObject>(prefab);
            spawnedObject.transform.SetParent(GenPivot.transform, false);

            StartCoroutine(CaptureImage());
            
            captureCamera.targetTexture = null;
            captureCamera.enabled = false;
            Destroy(spawnedObject);
            return iconComp;
        }

        private IEnumerator CaptureImage()
        {
            yield return new WaitForEndOfFrame();

            RenderTexture.active = genTexture;
            Texture2D image = new Texture2D(genTexture.width, genTexture.height);
            image.ReadPixels(new Rect(0, 0, genTexture.width, genTexture.height), 0, 0);
            image.Apply();

            iconComp.sprite = Sprite.Create(image, new Rect(0, 0, genTexture.width, genTexture.height), Vector2.zero);
        }
    }
}