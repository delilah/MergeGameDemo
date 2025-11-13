using UnityEngine;
using UnityEngine.EventSystems;
public class debugclick : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked on " + gameObject.name);
    }
}
