using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace AwesomeTechnologies.Utility.Quadtree
{  
    /// <summary>
    /// The QuadTreeNode
    /// </summary>
    public class QuadTreeNode<T> where T : IHasRect
    {
        /// <summary>
        /// Construct a quadtree node with the given rect 
        /// </summary>
        /// <param name="rect"></param>
        public QuadTreeNode(Rect rect)
        {
            _rect = rect;                       
        }
        /// <summary>
        /// The area of this node
        /// </summary>
        Rect _rect;

        /// <summary>
        /// The contents of this node.
        /// Note that the contents have no limit: this is not the standard way to impement a QuadTree
        /// </summary>
        readonly List<T> _contentList = new List<T>();

        /// <summary>
        /// The child nodes of the QuadTree
        /// </summary>
        readonly List<QuadTreeNode<T>> _nodes = new List<QuadTreeNode<T>>(4);

        /// <summary>
        /// Is the node empty
        /// </summary>
        public bool IsEmpty => _rect.width == 0  || _rect.height == 0 || _nodes.Count == 0;

        /// <summary>
        /// Area of the quadtree node
        /// </summary>
        public Rect Rect => _rect;

        /// <summary>
        /// Total number of nodes in the this node and all SubNodes
        /// </summary>
        public int Count
        {
            get
            {
                int count = 0;

                foreach (QuadTreeNode<T> node in _nodes)
                    count += node.Count;

                count += _contentList.Count;

                return count;
            }
        }

        /// <summary>
        /// Return the contents of this node and all subnodes in the true below this one.
        /// </summary>
        public void SubTreeContents(List<T> results)
        {
            for (int i = 0; i <= _nodes.Count - 1; i++)
            {
                _nodes[i].SubTreeContents(results);
            }

            for (int i = 0; i <= _contentList.Count - 1; i++)
            {
                results.Add(_contentList[i]);                
            }             
        }

        /// <summary>
        /// Query the QuadTree for items that are in the given area
        /// </summary>
        /// <param name="queryArea"></param>
        /// <param name="results"></param>
        public void Query(Rect queryArea, List<T> results)
        {
            // create a list of the items that are found
            //List<T> results = new List<T>();

            // this quad contains items that are not entirely contained by
            // it's four sub-quads. Iterate through the items in this quad 
            // to see if they intersect.
            foreach (T item in _contentList)
            {
                if (queryArea.Overlaps(item.Rectangle))
                    results.Add(item);
            }

            foreach (QuadTreeNode<T> node in _nodes)
            {
                if (node.IsEmpty)
                    continue;

                // Case 1: search area completely contained by sub-quad
                // if a node completely contains the query area, go down that branch
                // and skip the remaining nodes (break this loop)
                if (node.Rect.Contains(queryArea))
                {
                   node.Query(queryArea,results);
                    break;
                }

                // Case 2: Sub-quad completely contained by search area 
                // if the query area completely contains a sub-quad,
                // just add all the contents of that quad and it's children 
                // to the result set. You need to continue the loop to test 
                // the other quads
                if (queryArea.Contains(node.Rect))
                {
                    node.SubTreeContents(results);
                    continue;
                }

                // Case 3: search area intersects with sub-quad
                // traverse into this quad, continue the loop to search other
                // quads
                if (node.Rect.Overlaps(queryArea))
                {
                    node.Query(queryArea,results);
                }
            }
        }

        /// <summary>
        /// Insert an item to this node
        /// </summary>
        /// <param name="item"></param>
        public void Insert(T item)
        {
            // if the item is not contained in this quad, there's a problem
            if (!_rect.Contains(item.Rectangle))
            {
                Trace.TraceWarning("feature is out of the rect of this quadtree node");
                return;
            }

            // if the subnodes are null create them. may not be sucessfull: see below
            // we may be at the smallest allowed size in which case the subnodes will not be created
            if (_nodes.Count == 0)
                CreateSubNodes();

            // for each subnode:
            // if the node contains the item, add the item to that node and return
            // this recurses into the node that is just large enough to fit this item
            foreach (QuadTreeNode<T> node in _nodes)
            {
                if (node.Rect.Contains(item.Rectangle))
                {
                    node.Insert(item);
                    return;
                }
            }

            // if we make it to here, either
            // 1) none of the subnodes completely contained the item. or
            // 2) we're at the smallest subnode size allowed 
            // add the item to this node's contents.
            _contentList.Add(item);
        }

        //move quadtree node
        public void Move(Vector2 offset)
        {
            foreach (QuadTreeNode<T> node in _nodes)
            {
                node.Move(offset);
            }
            _rect = new Rect(_rect.xMin + offset.x, _rect.yMin + offset.y, _rect.width, _rect.height);
        }
      
        /// <summary>
        /// Internal method to create the subnodes (partitions space)
        /// </summary>
        private void CreateSubNodes()
        {
            // the smallest subnode has an area 
            if (_rect.height * _rect.width <= 10)
                return;

            float halfWidth = (_rect.width / 2f);
            float halfHeight = (_rect.height / 2f);

            _nodes.Add(new QuadTreeNode<T>(new Rect(new Vector2(_rect.xMin, _rect.yMin), new Vector2(halfWidth, halfHeight))));
            _nodes.Add(new QuadTreeNode<T>(new Rect(new Vector2(_rect.xMin, _rect.yMin + halfHeight), new Vector2(halfWidth, halfHeight))));
            _nodes.Add(new QuadTreeNode<T>(new Rect(new Vector2(_rect.xMin + halfWidth, _rect.yMin), new Vector2(halfWidth, halfHeight))));
            _nodes.Add(new QuadTreeNode<T>(new Rect(new Vector2(_rect.xMin + halfWidth, _rect.yMin + halfHeight), new Vector2(halfWidth, halfHeight))));
        }
    }
}
