#define QUADTREE_TRACK_STATS

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BarnoGames.QuadTree
{
    public class QuadTree : MonoBehaviour
    {

        class Node
        {
            Rect Bounds;
            Node[] Children;
            int Depth = -1;

            HashSet<ISpacialData2D> Data;

            public Node(Rect inBounds, int depth = 0)
            {
                Bounds = inBounds;
                Depth = depth;
            }

            public void Draw()
            {
                Color color = Color.Lerp(Color.red, Color.green, (float)Depth / Depth); // You need to define maxDepth or get it from somewhere
                Gizmos.DrawWireCube(new Vector3(Bounds.center.x, 0f, Bounds.center.y), new Vector3(Bounds.size.x, 0f, Bounds.size.y));


                if (Children == null)
                {
                    return;

                }
                // If this node has children, recursively draw each child
                foreach (Node child in Children)
                {
                    child.Draw();
                }

            }

            public void AddData(QuadTree owner, ISpacialData2D newData)
            {
                if (Children == null)
                {
                    // FIRST DATA FOR NODE ?
                    if (Data == null)
                        Data = new();

                    // REACT THE SPLIT POINT AND PREMISSTED TO ?
                    if (((Data.Count + 1) >= owner.PrefferedMaxDataPerNode) && CanSplit(owner))
                    {
                        SplitNode(owner);
                    }
                    else
                        Data.Add(newData);

                    return;
                }

                AddDataToChildren(owner, newData);
            }

            private bool CanSplit(QuadTree owner)
            {
                return (Bounds.width >= (owner.MinimumNodeSize * 2)) &&
                       (Bounds.height >= (owner.MinimumNodeSize * 2));
            }

            private void SplitNode(QuadTree owner)
            {
                float halfWidth = Bounds.width / 2f;
                float halfHeight = Bounds.height / 2f;
                int newDepth = Depth + 1;

#if QUADTREE_TRACK_STATS
                owner.NewNodesCreated(4, newDepth);
#endif // QUADTREE_TRACK_STATS

                Children = new Node[4]
                {
                    new Node(new Rect(Bounds.xMin,             Bounds.yMin,              halfWidth, halfHeight), newDepth),     // BOTTOM LEFT
                    new Node(new Rect(Bounds.xMin + halfWidth, Bounds.yMin,              halfWidth, halfHeight), newDepth),     // BOTTOM RIGHT
                    new Node(new Rect(Bounds.xMin,             Bounds.yMin + halfHeight, halfWidth, halfHeight), newDepth),     // TOP LEFT
                    new Node(new Rect(Bounds.xMin + halfWidth, Bounds.yMin + halfHeight, halfWidth, halfHeight), newDepth)      // TOP RIGHT          
                };

                // DISTRIBUTE THE DATA
                foreach (var _data in Data)
                {
                    AddDataToChildren(owner, _data);
                }

                Data = null;
            }

            private void AddDataToChildren(QuadTree owner, ISpacialData2D newData)
            {
                foreach (var child in Children)
                {
                    if (child.Overlaps(newData.GetBounds()))
                        child.AddData(owner, newData);
                }
            }

            private bool Overlaps(Rect other)
            {
                return Bounds.Overlaps(other);
            }

            public void FindDataInBox(Rect searchRect, HashSet<ISpacialData2D> outFoundData)
            {
                if (Children == null)
                {
                    if (Data == null || Data.Count == 0)
                        return;

                    outFoundData.UnionWith(Data);

                    return;
                }


                foreach (Node child in Children)
                {
                    if (child.Overlaps(searchRect))
                        child.FindDataInBox(searchRect, outFoundData);
                }
            }

            public void FindDataInRange(Vector2 searchLocation, float searchRange, HashSet<ISpacialData2D> outFoundData)
            {
                if (Depth != 0)
                {
                    throw new System.InvalidOperationException("FindDataInRange cannot run on anything other than the root Node!!!");
                }

                Rect searchRect = new Rect(searchLocation.x - searchRange,
                                           searchLocation.y - searchRange,
                                           width: searchRange * 2f,
                                           height: searchRange * 2f);

                FindDataInBox(searchRect, outFoundData);

                // MAKE RADIUS ISTEAD OF BOX
                outFoundData.RemoveWhere(data =>
                {
                    float testRange = searchRange + data.GetRadius();

                    return (searchLocation - data.GetLocation()).sqrMagnitude > (testRange * testRange);
                });
            }
        }


        [field: SerializeField] public int PrefferedMaxDataPerNode { get; private set; } = 50;
        [field: SerializeField] public int MinimumNodeSize { get; private set; } = 2;

        Node RootNode;

        public void PrepareTree(Rect bounds)
        {
            RootNode = new Node(bounds);

#if QUADTREE_TRACK_STATS
            TrackedMaxDepth = -1;
            TackedNumNodes = 0;
#endif // #if QUADTREE_TRACK_STATS
        }

        public void AddData(ISpacialData2D data)
        {
            RootNode.AddData(this, data);
        }

        public void AddData(List<ISpacialData2D> data)
        {
            foreach (ISpacialData2D item in data)
                AddData(data);
        }

        public void ShowStats()
        {
#if QUADTREE_TRACK_STATS
            Debug.Log("==== QuadTree Stats =====");
            Debug.Log($"Max Depth   : {TrackedMaxDepth}");
            Debug.Log($"Node Number : {TackedNumNodes}");

#endif // #if QUADTREE_TRACK_STATS

        }

        public void DrawWireFrame()
        {
            RootNode.Draw();
        }

        public HashSet<ISpacialData2D> FindDataInRange(Vector2 searchLocation, float searchRange)
        {
#if QUADTREE_TRACK_STATS
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
#endif // QUADTREE_TRACK_STATS

            HashSet<ISpacialData2D> foundData = new();
            RootNode.FindDataInRange(searchLocation, searchRange, foundData);

#if QUADTREE_TRACK_STATS
            stopWatch.Stop();
            Debug.Log($"Search Found {foundData.Count}, results in {stopWatch.ElapsedMilliseconds} ms");
#endif // QUADTREE_TRACK_STATS

            return foundData;
        }

#if QUADTREE_TRACK_STATS
        int TrackedMaxDepth = -1;
        int TackedNumNodes = 0;
        private void NewNodesCreated(int numberAdded, int newDepth)
        {
            TrackedMaxDepth = Mathf.Max(TrackedMaxDepth, newDepth);
            TackedNumNodes += numberAdded;
        }

#endif // QUADTREE_TRACK_STATS
    }
}
