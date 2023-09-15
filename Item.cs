using MSCLoader;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Button;

namespace MSCSpawnMenu
{
    public class Item : MonoBehaviour
    {
        public string Text;
        void Start()
        {
            gameObject.transform.Find("Text").GetComponent<Text>().text = Text;
        }
        void Update()
        {

        }
    }
}