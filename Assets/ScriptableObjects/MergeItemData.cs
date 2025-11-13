using UnityEngine;

[CreateAssetMenu(menuName = "Data/Merge Item Data")]
public class MergeItemData : ScriptableObject
{
    public string itemName;
    public int level;
    public Sprite sprite;
    public MergeItemData nextItem;
    public bool isFinal;
}
