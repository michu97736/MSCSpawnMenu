using MSCLoader;
using UnityEngine;
using UnityEngine.UI;

namespace MSCSpawnMenu
{
    public class Item : MonoBehaviour
    {
        public string Text;
        public GameObject item;
        private void Start()
        {
            gameObject.transform.Find("Text").GetComponent<Text>().text = Text;
        }
        void Update()
        {

        }
    }
}