using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace SignaliEdge
{
    class RenderStyles
    {
        private ArrayList wordsCSS = new ArrayList();//Список свойств CSS
        private int LineCounterCSS = 0;//индекс по каторому нужно вставлять в wordsCSS свойства
        private const string PATHFILESTYLE = @"C:\\Users\\Alexv\\OneDrive\\Рабочий стол\\тесты\\style.css";

        public void RenderCSS(Dictionary<int, ValuesDictionary> BlocksDictionary)
        {
            using (StreamWriter sw = new StreamWriter(PATHFILESTYLE, false, System.Text.Encoding.Default))
            {
                foreach(var item in BlocksDictionary)
                {
                    wordsCSS.Insert(LineCounterCSS, $".defaultBlock{item.Key} {{ \n  " +
                    $"height:{item.Value.height}px;\n  " +
                    $"width: {item.Value.width}px;\n  " +
                    $"border: 1px solid #000;\n  " +
                    $"display: flex;\n  " +
                    $"justify-content: center;\n  " +
                    $"align-items: center;\n  " +
                    $"margin: 10px;\n  " +
                    //$"background: rgb({BlocksDictionaryCSS[i].background});\n  " +
                    $"z-index: {item.Key};\n" +
                    $"}}");
                }

                wordsCSS.Insert(LineCounterCSS, "body {\n  padding: 0;\n  margin: 0; \n}");
                foreach (string w in wordsCSS)
                {
                    sw.WriteLine(w);
                }
                LineCounterCSS++;
            }
        }
    }

    class PropertyCSS
    {
        public int width { get; set; }
        public int height { get; set; }
        public string position { get; set; }
        public string left { get; set; }
        public string right { get; set; }
        public string background { get; set; }

        public PropertyCSS(int width = 0, int height = 0, string background = "")
        {
            this.width = width;
            this.height = height;
            this.background = background;
        }
    }
}
