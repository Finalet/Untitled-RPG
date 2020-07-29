using System.Collections.Generic;
using UnityEngine;

namespace AwesomeTechnologies.Utility.Quadtree
{
    public class QuadTree<T> where T : IHasRect
    {
        /// <summary>
        /// The root QuadTreeNode
        /// </summary>
        readonly QuadTreeNode<T> _root;

        /// <summary>
        /// The bounds of this QuadTree
        /// </summary>
        private Rect _rect;

        /// <summary>
        /// Create the quadtree
        /// </summary>
        /// <param name="rect"></param>
        public QuadTree(Rect rect)
        {
            _rect = rect;
            _root = new QuadTreeNode<T>(_rect);
        }

        /// <summary>
        /// Get the count of items in the QuadTree
        /// </summary>
        public int Count => _root.Count;

        /// <summary>
        /// Insert the feature into the QuadTree
        /// </summary>
        /// <param name="item"></param>
        public void Insert(T item)
        {
            _root.Insert(item);
        }

        public void Move(Vector2 offset)
        {
            _rect = new Rect(_rect.xMin + offset.x, _rect.yMin + offset.y, _rect.width, _rect.height);
            _root.Move(offset);
        }
        public void Query(Rect area, List<T> results)
        {
           _root.Query(area,results);
        }        
    }
}
