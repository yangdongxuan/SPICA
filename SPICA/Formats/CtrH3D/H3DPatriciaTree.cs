﻿using SPICA.Serialization;
using SPICA.Serialization.Attributes;

using System;
using System.Collections.Generic;
using System.Collections;

namespace SPICA.Formats.CtrH3D
{
    public class H3DPatriciaTree : ICustomSerialization, IEnumerable<string>
    {
        [Ignore] private List<H3DPatriciaTreeNode> Nodes;
        [Ignore] private List<string> Names;

        [Ignore] private bool TreeNeedsRebuild;

        public int MaxLength
        {
            get
            {
                int Max = 0;

                foreach (string Name in Names)
                {
                    if (Name.Length > Max) Max = Name.Length;
                }

                return Max;
            }
        }

        public int Count { get { return Nodes.Count; } }

        private const string DuplicateKeysEx = "Tree shouldn't contain duplicate keys!";

        public H3DPatriciaTree()
        {
            Nodes = new List<H3DPatriciaTreeNode> { new H3DPatriciaTreeNode() };
            Names = new List<string>();
        }

        void ICustomSerialization.Deserialize(BinaryDeserializer Deserializer)
        {
            int MaxIndex = 0;
            int Index = 0;

            Nodes.Clear();

            while (Index++ <= MaxIndex)
            {
                H3DPatriciaTreeNode Node = Deserializer.Deserialize<H3DPatriciaTreeNode>();

                MaxIndex = Math.Max(MaxIndex, Node.LeftNodeIndex);
                MaxIndex = Math.Max(MaxIndex, Node.RightNodeIndex);

                if (Nodes.Count > 0) Names.Add(Node.Name);

                Nodes.Add(Node);
            }
        }

        bool ICustomSerialization.Serialize(BinarySerializer Serializer)
        {
            if (TreeNeedsRebuild) RebuildTree();

            Serializer.WriteValue(Nodes);

            return true;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return Names.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        //Implementation
        public bool Contains(string Name)
        {
            return Find(Name) != -1;
        }

        public string Find(int Index)
        {
            return Names[Index];
        }

        public int Find(string Name)
        {
            if (TreeNeedsRebuild) RebuildTree();

            int Output = 0;

            if (Nodes != null && Nodes.Count > 0)
            {
                H3DPatriciaTreeNode Root;

                Output = Traverse(Name, out Root);

                if (Nodes[Output].Name != Name) Output = 0;
            }

            return Output - 1;
        }

        public void Add(string Name)
        {
            Names.Add(Name);

            TreeNeedsRebuild = true;
        }

        public void Insert(int Index, string Name)
        {
            Names.Insert(Index, Name);

            TreeNeedsRebuild = true;
        }

        public void Remove(string Name)
        {
            Names.Remove(Name);

            TreeNeedsRebuild = true;
        }

        public void Clear()
        {
            Names.Clear();

            TreeNeedsRebuild = true;
        }

        private void RebuildTree()
        {
            Nodes.Clear();

            if (Names.Count > 0)
                Nodes.Add(new H3DPatriciaTreeNode { ReferenceBit = uint.MaxValue });
            else
                Nodes.Add(new H3DPatriciaTreeNode());

            foreach (string Name in Names)
            {
                Insert(Name);
            }

            TreeNeedsRebuild = false;
        }

        private void Insert(string Name)
        {
            if (Name == null) return;

            H3DPatriciaTreeNode New = new H3DPatriciaTreeNode();
            H3DPatriciaTreeNode Root;

            uint Bit = (uint)((MaxLength << 3) - 1);
            int Index = Traverse(Name, out Root);

            while (GetBit(Nodes[Index].Name, Bit) == GetBit(Name, Bit))
            {
                if (--Bit == uint.MaxValue) throw new InvalidOperationException(DuplicateKeysEx);
            }

            New.ReferenceBit = Bit;

            if (GetBit(Name, Bit))
            {
                New.LeftNodeIndex  = (ushort)Traverse(Name, out Root, Bit);
                New.RightNodeIndex = (ushort)Nodes.Count;
            }
            else
            {
                New.LeftNodeIndex  = (ushort)Nodes.Count;
                New.RightNodeIndex = (ushort)Traverse(Name, out Root, Bit);
            }

            New.Name = Name;

            int RootIndex = Nodes.IndexOf(Root);

            if (GetBit(Name, Root.ReferenceBit))
                Root.RightNodeIndex = (ushort)Nodes.Count;
            else
                Root.LeftNodeIndex  = (ushort)Nodes.Count;

            Nodes.Add(New);

            Nodes[RootIndex] = Root;
        }

        private int Traverse(string Name, out H3DPatriciaTreeNode Root, uint Bit = 0)
        {
            Root = Nodes[0];

            int Output = Root.LeftNodeIndex;

            H3DPatriciaTreeNode Left = Nodes[Output];

            while (Root.ReferenceBit > Left.ReferenceBit && Left.ReferenceBit > Bit)
            {
                if (GetBit(Name, Left.ReferenceBit))
                    Output = Left.RightNodeIndex;
                else
                    Output = Left.LeftNodeIndex;

                Root = Left;
                Left = Nodes[Output];
            }

            return Output;
        }

        private bool GetBit(string Name, uint Bit)
        {
            int Position = (int)(Bit >> 3);
            int CharBit  = (int)(Bit &  7);

            if (Name != null && Position < Name.Length)
                return ((Name[Position] >> CharBit) & 1) != 0;
            else
                return false;
        }
    }
}