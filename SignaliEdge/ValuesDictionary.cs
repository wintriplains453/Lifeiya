using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignaliEdge
{
    class ValuesDictionary
    {
        public bool Nestled { get; set; }
        public List<int> PointsArea { get; private set; }
        public int ID { get; private set; }
        public List<int> ParentArea { get; private set; }
        public int ParentArr { get; set; }
        public Dictionary<int, Blocks> Children { get; set; }
        public Dictionary<int, BlocksTextP> BlockTextP { get; set; }//99 - значение текста
        public int width { get; set; }
        public int height { get; set; }
        public int ParentFirst { get; set; }
        public string structure { get; set; }
        public List<int> FirstChild { get; set; }
        public int CountFirstChild { get; set; }

        public ValuesDictionary(bool displayName, List<int> pointsArea, int ID, int width, int height, string structure, Dictionary<int, Blocks> Children, Dictionary<int, BlocksTextP> BlockTextP, int ParentFirst)
        {
            Nestled = displayName;
            PointsArea = pointsArea;
            this.ID = ID;
            this.width = width;
            this.height = height;
            this.Children = Children;
            this.BlockTextP = BlockTextP;
            this.ParentArr = ParentArr;
            this.structure = structure;
            this.ParentFirst = ParentFirst;
        }
        public ValuesDictionary(List<int> parentArea, int[] ParendChildArr)
        {
            ParentArea = parentArea;
        }
        public ValuesDictionary(List<int> FirstChild)
        {
            this.FirstChild = FirstChild;
        }
        public ValuesDictionary() { }
    }

    class Blocks
    {
        public string structure { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int AllParent { get; set; }
        public bool used { get; set; }

        public Blocks(string structure, int width, int height, int Parent, bool used)
        {
            this.structure = structure;
            this.width = width;
            this.height = height;
            AllParent = Parent;
            this.used = used;
        }
    }
    class TextDictionary {
        public List<int> TextPoints { get; private set; }
        public int width { get; set; }
        public int height { get; set; }
        public string text { get; set; }

        public TextDictionary(List<int> TextPoints, int width, int height, string text)
        {
            this.TextPoints = TextPoints;
            this.width = width;
            this.height = height;
            this.text = text;
        }
    }


    class BlocksTextP
    {
        public string structure { get; set; }
        public List<string> parent { get; set; }
        public int ID { get; set; }
        public List<int> firstPouintText { get; set; }
        public List<int> thirdPouintText { get; set; }

        public BlocksTextP(string structure, int ID, List<int> arr1, List<int> arr3)
        {
            this.structure = structure;
            this.ID = ID;
            firstPouintText = arr1;
            thirdPouintText = arr3;
        }
    }
}
