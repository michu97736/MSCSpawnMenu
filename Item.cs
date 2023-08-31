using MSCLoader;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Button;

namespace MscSpawnMenu
{
    public class Item : MonoBehaviour
    {
        public string text;
        void Start()
        {
            gameObject.transform.Find("Text").GetComponent<Text>().text = text;
        }
        void Update()
        {

        }
    }
}