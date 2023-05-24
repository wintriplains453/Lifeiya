using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SignaliEdge
{
    class HTMLDOM
    {
        public Dictionary<int, ValuesDictionary> BlocksDictionary = new Dictionary<int, ValuesDictionary>();

        //Отрисовка HTML из списка
        public int LineCounterHTML = 0;
        public void HTMLCompletion(Dictionary<int, ValuesDictionary> BlocksDictionary)
        {
            BlocksDictionary = BlocksDictionary.OrderBy(i => i.Value.PointsArea[0].X).ThenBy(i => i.Value.PointsArea[0].Y).ToDictionary(i => i.Key, i => i.Value);
            foreach (var i in BlocksDictionary)
            {
                //Console.WriteLine("contours = " + i.Value.PointsArea[0] + " " + i.Value.PointsArea[1] + " height = " + i.Value.height + " width = " + i.Value.width);
                foreach (var j in BlocksDictionary)
                {
                    if (i.Value.ID != j.Value.ID)
                    {
                        if (i.Value.PointsArea[0].X <= j.Value.PointsArea[0].X && i.Value.PointsArea[1].X >= j.Value.PointsArea[1].X)//проверка входит ли блок j в блок i по ширине
                        {
                            if (i.Value.PointsArea[0].Y <= j.Value.PointsArea[0].Y && i.Value.PointsArea[2].Y >= j.Value.PointsArea[2].Y)//проверка входит ли блок j в блок i по высоте
                            {
                                j.Value.ParentFirst = i.Key;
                            }
                        }
                    }
                }
            }
            foreach (var i in BlocksDictionary)
            {
                List<int> CurrentmaxWidth = new List<int>();
                CurrentmaxWidth.Clear();
                foreach (var j in BlocksDictionary)
                {
                    if (i.Value.PointsArea[0].X < j.Value.PointsArea[0].X && i.Value.PointsArea[1].X > j.Value.PointsArea[1].X)//проверка входит ли блок j в блок i по ширине
                    {
                        if (i.Value.PointsArea[0].Y < j.Value.PointsArea[0].Y && i.Value.PointsArea[2].Y > j.Value.PointsArea[2].Y)//проверка входит ли блок j в блок i по высоте
                        {
                            i.Value.Children.Add(j.Value.ID, new Blocks("", j.Value.width, j.Value.height, i.Key, false));
                            if (i.Key == j.Value.ParentFirst)
                            {
                                CurrentmaxWidth.Add(j.Key);
                            }
                        }
                    }
                }
                i.Value.FirstChild = CurrentmaxWidth;
            }
            this.BlocksDictionary = BlocksDictionary;
        }

        
    }


    class CreateHTML
    {
        static HTMLDOM hTMLDOM = new HTMLDOM();

        //Отрисовка HTML из списка
        private int LineCounterHTML = hTMLDOM.LineCounterHTML;

        private List<string> blocksHTML = new List<string>();
        private int currentChild = 0;
        private string spacing = "";
        private int nesting = 0;
        private string structureDOM = "";
        private int newCount = 0;

        const string PATHFILE = @"C:\\Users\\Alexv\\OneDrive\\Рабочий стол\\тесты\\testindex.html";

        public void CreateDOM(Dictionary<int, ValuesDictionary> BlocksDictionary)
        {
            BlocksDictionary = BlocksDictionary.OrderBy(i => i.Value.ID).ToDictionary(i => i.Key, i => i.Value);
            //заполнение всех найденых элементов дочерними(находится после нахождения всех блоков в функции (findAllsBlocks))
            blocksHTML.Clear();
            using (StreamWriter sw = new StreamWriter(PATHFILE, false, System.Text.Encoding.Default))
            {

                blocksHTML.Insert(LineCounterHTML, "<!DOCTYPE html>\n" +
                    "<html lang=\"ru\">\n<head>\n    " +
                    "<meta charset=\"UTF-8\">\n    " +
                    "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n	" +
                    "<link rel=\"stylesheet\" type=\"text/css\" href=\"style.css\">\n    " +
                    "<title>Project</title>\n" +
                    "</head>\n" +
                    "<body>");
                foreach (string w in blocksHTML)
                {
                    sw.WriteLine(w);
                }
                LineCounterHTML++;
            }

            foreach (var item in BlocksDictionary.Values)
            {
                if (item.Children.Count == 0 && item.ParentFirst == 0)
                {
                    blocksHTML.Insert(LineCounterHTML, $"{" ".PadLeft(2)}<div class=\"defaultBlock{item.height}\"></div>");
                    LineCounterHTML++;
                }
                else
                {
                    LineCounterHTML += currentChild;
                    newCount = 0;
                    nesting = 0;
                    if (!item.Nestled)
                    {
                        parentF(item, structureDOM);
                        spacing = " ".PadLeft(4 * newCount);
                        blocksHTML.Insert(LineCounterHTML, $"{spacing}</div>");
                        LineCounterHTML++;
                    }
                }
            }

            using (StreamWriter sw = new StreamWriter(PATHFILE, false, Encoding.UTF8))
            {
                for (int i = 0; i < blocksHTML.Count; i++)
                {
                    try
                    {
                        //Console.WriteLine(blocksHTML[i]);
                        sw.WriteLine(blocksHTML[i]);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            using (StreamWriter sw = new StreamWriter(PATHFILE, false, Encoding.UTF8))
            {
                blocksHTML.Add("</body>\n</html>");

                foreach (string w in blocksHTML)
                {
                    sw.WriteLine(w);
                }
                LineCounterHTML++;
            }

            string parentF(ValuesDictionary item, string structureDOM)//structureDOM строка дочерних элементов 
            {
                //_______________________________
                //начало постоения HTML структуры
                spacing = " ".PadLeft(4 * newCount);
                if (item.Nestled != true)
                {
                    item.Nestled = true;
                    if (item.structure.Length == 0)
                    {
                        blocksHTML.Insert(LineCounterHTML, item.structure = $"{spacing}<div class=\"defaultBlock{item.height}\">");
                    }
                    else
                    {
                        blocksHTML.Insert(LineCounterHTML, item.structure = spacing + item.structure);
                    }

                    LineCounterHTML++;
                    //Text(item.ID);

                    if(item.FirstChild != null)
                    {
                        foreach (var child in item.FirstChild)//перебор всех вложенных элементов в блоке передаваемым под параметром item
                        {
                            if (BlocksDictionary[child].FirstChild.Count == 1)//если элемент имеет вложенность то запускается рекурсия на повторный пробег по функции и поиск вложенности
                            {
                                newCount++;
                                nesting++;
                                parentF(BlocksDictionary[child], structureDOM);//запуск рекурсии
                                spacing = " ".PadLeft(4 * newCount - 1);
                                blocksHTML.Insert(LineCounterHTML, $"{spacing}</div>");
                                LineCounterHTML++;
                                newCount--;
                            }
                            else if (BlocksDictionary[child].FirstChild.Count > 1)
                            {
                                newCount++;
                                for (int i = 0; i <= BlocksDictionary[child].FirstChild.Count; i++)
                                {
                                    parentF(BlocksDictionary[child], structureDOM);
                                }
                                spacing = " ".PadLeft(4 * newCount - 1);
                                blocksHTML.Insert(LineCounterHTML, $"{spacing}</div>");
                                LineCounterHTML++;
                                newCount--;
                            }
                            else//если вложенности нет то просто вывести 
                            {
                                BlocksDictionary[child].Nestled = true;
                                spacing = " ".PadLeft(4 * (newCount + 1) - 1);
                                if (BlocksDictionary[child].structure.Length == 0)
                                {
                                    BlocksDictionary[child].structure = $"{spacing}<div class=\"defaultBlock{BlocksDictionary[child].height}\"></div>";
                                }
                                else
                                {
                                    BlocksDictionary[child].structure = spacing + BlocksDictionary[child].structure;
                                }
                                blocksHTML.Insert(LineCounterHTML, BlocksDictionary[child].structure);
                                LineCounterHTML++;
                            }
                        }
                        currentChild = 0;
                        return structureDOM;
                    }
                }
                return structureDOM;
            }
        }
    }
}
