using UnityEngine;

namespace AwesomeTechnologies.Utility.Quadtree
{
    /// <summary>
    /// An interface that defines and object with a rectangle
    /// </summary>
    public interface IHasRect
    {
        Rect Rectangle { get; }
    }
}
