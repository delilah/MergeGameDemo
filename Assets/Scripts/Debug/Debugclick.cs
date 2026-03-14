using UnityEngine;
using UnityEngine.EventSystems;

namespace MergeGame.MergeDebug
{
    public class DebugClick : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("Clicked on " + gameObject.name);
        }
    }
}
