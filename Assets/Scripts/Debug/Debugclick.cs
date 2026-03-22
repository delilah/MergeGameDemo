using UnityEngine;
using UnityEngine.EventSystems;

namespace MergeGame.MergeDebug
{
    public class DebugClick : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
#if UNITY_EDITOR
            Debug.Log("Clicked on " + gameObject.name);
#endif
        }
    }
}
