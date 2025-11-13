using UnityEngine;
using UnityEngine.EventSystems;

public interface IDraggable : IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>
    /// The current tile this object is on
    /// </summary>
    Tile CurrentTile { get; }
    
    /// <summary>
    /// Called when the object is dragged up
    /// </summary>
    void OnPickUp();
    
    /// <summary>
    /// Called when the object is dropped on a valid target
    /// </summary>
    /// <param name="targetTile">The tile where the object was dropped</param>
    /// <returns>True if the drop was successful</returns>
    bool OnDrop(Tile targetTile);
    
    /// <summary>
    /// Called when the object needs to return to its original position
    /// </summary>
    void ReturnToStartPosition();
}