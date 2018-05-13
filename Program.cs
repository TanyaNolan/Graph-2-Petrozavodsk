using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace graphs2
{
    class Program
    {
        private static double minlon;
        private static double maxlat;
        private static double maxlon;
        private static double minlat;

        private static SortedDictionary<long, point> Nodes = new SortedDictionary<long, point>();
        private static SortedDictionary<long, List<long>> AddjestedList = new SortedDictionary<long, List<long>>();
        private static List<string> ValidHighways = new List<string>() {"motorway", "motorway_link", "trunk", "trunk_link", "primary", "primary_link", "secondary",
                 "secondary_link", "tertiary", "tertiary_link", "unclassified", "road", "service", "living_street",
                 "residential" };
        struct point
        {
            public double lat;
            public double lon;
        }

        private static SortedDictionary<long, pointInfo> NotCheckedNodesInfo = new SortedDictionary<long, pointInfo>();
        private static SortedDictionary<long, pointInfo> CheckedNodesInfo = new SortedDictionary<long, pointInfo>();
        struct pointInfo
        {
            public long id;
            public double X;
            public double Y;
            public double weight;
            public int prevPoint;
            public bool isChecked;
        }
        private static List<long> EndPoints = new List<long>();

        private static List<long>[] DijkstraWays = new List<long>[10];
        private static pointInfo[] Arr;
        static void FindDijkstraWay(int numbWay,long EndPoint, int N)
        {
            int iWay = -1;
            double weight=-1;
            for (int i = 0; i < N; ++i)
                if (Arr[i].id == EndPoint)
                {
                    iWay = i;
                    weight = Arr[i].weight;
                    break;
                }
            if (iWay == -1)
            {
                Console.WriteLine("No way to the {0} point!", numbWay);
                return;
            }
            while (iWay != 0)
            {
                DijkstraWays[numbWay].Add(Arr[iWay].id);
                iWay = Arr[iWay].prevPoint;
            }
            DijkstraWays[numbWay].Add(Arr[iWay].id);
            //foreach (long i in DijkstraWays[numbWay])
            //{
            //    Console.WriteLine(i);
            //}
            //Console.WriteLine("Weight {0}", weight);
        }
        static void DijkstraAlg(long startPoint, long EndPoint, int numbOfWay)
        {
            //инициализация
            ICollection<long> keys = AddjestedList.Keys;
            int N = keys.Count();
            Arr=new pointInfo[N];
            int k = 0;
            Arr[k].id = startPoint;
            Arr[k].X = CoordsWork.lonToX(Nodes[startPoint].lon);
            Arr[k].Y = CoordsWork.latToY(Nodes[startPoint].lat);
            Arr[k].weight = 0;
            Arr[k].prevPoint = 0;
            Arr[k].isChecked = false;
            k++;
            int iEndPoint = -1;
            foreach (long i in keys)
            {
                if (i != startPoint)
                {
                    if (i == EndPoint) iEndPoint = k;
                    Arr[k].id = i;
                    Arr[k].X = CoordsWork.lonToX(Nodes[i].lon);
                    Arr[k].Y = CoordsWork.latToY(Nodes[i].lat);
                    Arr[k].weight = Double.PositiveInfinity;
                    Arr[k].prevPoint = 0;
                    Arr[k].isChecked = false;
                    k++;
                }
            }
            if (iEndPoint == -1) { Console.WriteLine("ERROR endPont id!!!!!!"); return; }
            //алгоритм дейкстры 
            for (int j=0; j<keys.Count(); ++j)
            {
                double minWeight= Double.PositiveInfinity;
                long currPoint=0;
                int iCurrPoint=0;
                for (int i=0; i<N;++i)
                {
                    if (Arr[i].weight!= Double.PositiveInfinity && Arr[i].weight < minWeight && !Arr[i].isChecked)
                    {
                        minWeight = Arr[i].weight;
                        currPoint = Arr[i].id;
                        iCurrPoint = i;
                    }
                }

                if (currPoint == EndPoint)//Дошли до нужной точки
                { //Ищем пути от конечных точек
                    DijkstraWays[numbOfWay] = new List<long>();                    
                    FindDijkstraWay(numbOfWay, EndPoint, N);
                    return;
                }

                if (currPoint != 0)
                {
                    for (int i = 0; i < AddjestedList[currPoint].Count(); ++i)
                    {
                        long nextPoint = AddjestedList[currPoint][i];
                        int iNextPoint=0;
                        for(int p = 1; p < N; ++p)
                        {
                            if (Arr[p].id == nextPoint)
                            {
                                iNextPoint = p;
                                break;
                            }
                        }
                        if (!Arr[iNextPoint].isChecked)
                        {
                            double weightCurrEdge = Math.Sqrt(Math.Pow(Arr[iCurrPoint].X - Arr[iNextPoint].X, 2.0) + Math.Pow(Arr[iCurrPoint].Y - Arr[iNextPoint].Y, 2.0));
                            if (Arr[iNextPoint].weight > Arr[iCurrPoint].weight + weightCurrEdge)
                            {
                                Arr[iNextPoint].weight = Arr[iCurrPoint].weight + weightCurrEdge;
                                Arr[iNextPoint].prevPoint = iCurrPoint;//записываем номер ячейки массива!!!!!!!
                            }
                        }
                    }
                    Arr[iCurrPoint].isChecked = true;
                }
                else
                {
                    break;
                }
            }
        }

        private static List<long>[] LevitWays = new List<long>[10];
        private static pointInfo[] ArrLevit;
        static void FindLevitWay(int numbWay, long EndPoint, int N)
        {
            int iWay = -1;
            double weight = -1;
            for (int i = 0; i < N; ++i)
                if (ArrLevit[i].id == EndPoint)
                {
                    iWay = i;
                    weight = ArrLevit[i].weight;
                    break;
                }
            if (iWay == -1)
            {
                Console.WriteLine("No way to the {0} point!", numbWay);
                return;
            }
            while (iWay != 0)
            {
                LevitWays[numbWay].Add(ArrLevit[iWay].id);
                iWay = ArrLevit[iWay].prevPoint;
            }
            LevitWays[numbWay].Add(ArrLevit[iWay].id);
            //foreach (long i in LevitWays[numbWay])
            //{
            //    Console.WriteLine(i);
            //}
            //Console.WriteLine("Weight {0}", weight);
        }
        static void LevitAlg(long startPoint)
        {
            //инициализация
            List<long> Counted = new List<long>();
            List<long> NotCounted = new List<long>();
            Queue<long> MainQueue = new Queue<long>();
            Queue<long> UrgentQueue = new Queue<long>();

            ICollection<long> keys = AddjestedList.Keys;
            int N = keys.Count();
            ArrLevit = new pointInfo[N];

            int k = 0;
            ArrLevit[k].id = startPoint;
            ArrLevit[k].X = CoordsWork.lonToX(Nodes[startPoint].lon);
            ArrLevit[k].Y = CoordsWork.latToY(Nodes[startPoint].lat);
            ArrLevit[k].weight = 0;
            ArrLevit[k].prevPoint = 0;
            ArrLevit[k].isChecked = false;
            MainQueue.Enqueue(startPoint);
            k++;
            foreach (long i in keys)
            {
                if (i != startPoint)
                {
                    ArrLevit[k].id = i;
                    ArrLevit[k].X = CoordsWork.lonToX(Nodes[i].lon);
                    ArrLevit[k].Y = CoordsWork.latToY(Nodes[i].lat);
                    ArrLevit[k].weight = Double.PositiveInfinity;
                    ArrLevit[k].prevPoint = 0;
                    ArrLevit[k].isChecked = false;
                    NotCounted.Add(i);
                    k++;
                }
            }

            //алгоритм Левита
            while (MainQueue.Count() != 0 || UrgentQueue.Count() != 0)
            {
                long idCurrPoint;
                if (UrgentQueue.Count() != 0)
                {
                    idCurrPoint = UrgentQueue.Dequeue();
                }
                else
                {
                    idCurrPoint = MainQueue.Dequeue();
                }
                int iCurrPoint = -1; ;
                for (int i=0; i<N; ++i)
                {
                    if (ArrLevit[i].id == idCurrPoint)
                    {
                        iCurrPoint = i;
                        break;
                    }
                }
                if (iCurrPoint == -1) { Console.WriteLine("Error iCurrPoint!"); break; }
                for (int i = 0; i < AddjestedList[idCurrPoint].Count(); ++i)
                {
                    long idNextPoint = AddjestedList[idCurrPoint][i];
                    int iNextPoint = -1; ;
                    for (int p = 0; p < N; ++p)
                    {
                        if (ArrLevit[p].id == idNextPoint)
                        {
                            iNextPoint = p;
                            break;
                        }
                    }
                    if (iCurrPoint == -1) { Console.WriteLine("Error iCurrPoint!"); break; }

                    double weightCurrEdge = Math.Sqrt(Math.Pow(ArrLevit[iCurrPoint].X - ArrLevit[iNextPoint].X, 2.0) + Math.Pow(ArrLevit[iCurrPoint].Y - ArrLevit[iNextPoint].Y, 2.0));
                    
                    if (NotCounted.Contains(idNextPoint))
                    {
                        MainQueue.Enqueue(idNextPoint);
                        NotCounted.Remove(idNextPoint);
                        if (ArrLevit[iNextPoint].weight > ArrLevit[iCurrPoint].weight + weightCurrEdge)
                        {
                            ArrLevit[iNextPoint].weight = ArrLevit[iCurrPoint].weight + weightCurrEdge;
                            ArrLevit[iNextPoint].prevPoint = iCurrPoint;
                        }
                    }
                    else {
                        if (UrgentQueue.Contains(idNextPoint))
                        {
                            if (ArrLevit[iNextPoint].weight > ArrLevit[iCurrPoint].weight + weightCurrEdge)
                            {
                                ArrLevit[iNextPoint].weight = ArrLevit[iCurrPoint].weight + weightCurrEdge;
                                ArrLevit[iNextPoint].prevPoint = iCurrPoint;
                            }
                        }
                        else {
                            if (MainQueue.Contains(idNextPoint))
                            {
                                if (ArrLevit[iNextPoint].weight > ArrLevit[iCurrPoint].weight + weightCurrEdge)
                                {
                                    ArrLevit[iNextPoint].weight = ArrLevit[iCurrPoint].weight + weightCurrEdge;
                                    ArrLevit[iNextPoint].prevPoint = iCurrPoint;
                                }
                            }
                            else {
                                if (Counted.Contains(idNextPoint) && ArrLevit[iNextPoint].weight > ArrLevit[iCurrPoint].weight + weightCurrEdge) {
                                    UrgentQueue.Enqueue(idNextPoint);
                                    Counted.Remove(idNextPoint);
                                    ArrLevit[iNextPoint].weight = ArrLevit[iCurrPoint].weight + weightCurrEdge;
                                    ArrLevit[iNextPoint].prevPoint = iCurrPoint;
                                }                                        
                            }
                        }
                    }              
                }
                Counted.Add(idCurrPoint);
            }

            for (int i = 0; i < 10; i++)
            {
                LevitWays[i] = new List<long>();
            }
            
            for (int i = 0; i < 10; ++i)
            {
                FindLevitWay(i, idRest[i], N);
            }
        }

        struct pointInfoAStar
        {
            public long id;
            public double X;
            public double Y;
            public double weight;
            public double h;
            public double f; //f=h+weight
            public int prevPoint;
            public bool isChecked;
        }
        private static List<long>[] AStarWays = new List<long>[10];
        private static List<double> AStarWeigth = new List<double>();
        private static pointInfoAStar[] AStarArr;
        static double FindAStarWay(int numbWay, long EndPoint, int N)
        {
            int iWay = -1;
            double weight = -1;
            for (int i = 0; i < N; ++i)
                if (AStarArr[i].id == EndPoint)
                {
                    iWay = i;
                    weight = AStarArr[i].weight;
                    break;
                }
            if (iWay == -1)
            {
                Console.WriteLine("No way to the {0} point!", numbWay);
                return 0;
            }
            while (iWay != 0)
            {
                AStarWays[numbWay].Add(AStarArr[iWay].id);
                iWay = AStarArr[iWay].prevPoint;
            }
            AStarWays[numbWay].Add(AStarArr[iWay].id);
            //foreach (long i in AStarWays[numbWay])
            //{
            //    Console.WriteLine(i);
            //}
            //Console.WriteLine("Weight {0}", weight);
            return weight;
        }
        static double FuncH(int iThisPoint, int iEndPoint)
        {
            return Math.Abs(AStarArr[iThisPoint].X - AStarArr[iEndPoint].X) + Math.Abs(AStarArr[iThisPoint].Y - AStarArr[iEndPoint].Y);            
        }
        static void AStarAlg(long startPoint, long EndPoint, int numbOfWay)
        {
            //инициализация
            ICollection<long> keys = AddjestedList.Keys;
            int N = keys.Count();
            AStarArr = new pointInfoAStar[N];
            int k = 0;
            AStarArr[k].id = startPoint;
            AStarArr[k].X = CoordsWork.lonToX(Nodes[startPoint].lon);
            AStarArr[k].Y = CoordsWork.latToY(Nodes[startPoint].lat);
            AStarArr[k].weight = 0;
            AStarArr[k].h = 0;
            AStarArr[k].f = 0;
            AStarArr[k].prevPoint = 0;
            AStarArr[k].isChecked = false;
            k++;
            int iEndPoint=-1;
            foreach (long i in keys)
            {
                if (i != startPoint)
                {
                    if (i == EndPoint) iEndPoint = k;
                    AStarArr[k].id = i;
                    AStarArr[k].X = CoordsWork.lonToX(Nodes[i].lon);
                    AStarArr[k].Y = CoordsWork.latToY(Nodes[i].lat);
                    AStarArr[k].weight = Double.PositiveInfinity;
                    AStarArr[k].h = Double.PositiveInfinity;
                    AStarArr[k].f = Double.PositiveInfinity;
                    AStarArr[k].prevPoint = 0;
                    AStarArr[k].isChecked = false;
                    k++;
                }
            }
            if (iEndPoint == -1) { Console.WriteLine("ERROR endPont id!!!!!!"); return; }
            //алгоритм A*
            for (int j = 0; j < keys.Count(); ++j)
            {
                double minF = Double.PositiveInfinity;
                long currPoint = 0;
                int iCurrPoint = 0;
                for (int i = 0; i < N; ++i)
                {
                    if (AStarArr[i].f != Double.PositiveInfinity && AStarArr[i].f < minF && !AStarArr[i].isChecked)
                    {
                        minF = AStarArr[i].f;
                        currPoint = AStarArr[i].id;
                        iCurrPoint = i;
                    }
                }
                if (currPoint == EndPoint)//Дошли до нужной точки
                { //Ищем пути от конечных точек
                    AStarWays[numbOfWay] = new List<long>();
                    AStarWeigth.Add(FindAStarWay(numbOfWay, EndPoint, N));
                    return;
                }

                if (currPoint != 0)
                {
                    for (int i = 0; i < AddjestedList[currPoint].Count(); ++i)
                    {
                        long nextPoint = AddjestedList[currPoint][i];
                        int iNextPoint = 0;
                        for (int p = 1; p < N; ++p)
                        {
                            if (AStarArr[p].id == nextPoint)
                            {
                                iNextPoint = p;
                                break;
                            }
                        }
                        if (!AStarArr[iNextPoint].isChecked)
                        {
                            double weightCurrEdge = Math.Sqrt(Math.Pow(AStarArr[iCurrPoint].X - AStarArr[iNextPoint].X, 2.0) + Math.Pow(AStarArr[iCurrPoint].Y - AStarArr[iNextPoint].Y, 2.0));
                            if (AStarArr[iNextPoint].weight > AStarArr[iCurrPoint].weight + weightCurrEdge)
                            {
                                AStarArr[iNextPoint].weight = AStarArr[iCurrPoint].weight + weightCurrEdge;
                                AStarArr[iNextPoint].h = FuncH(iNextPoint, iEndPoint);
                                AStarArr[iNextPoint].f = AStarArr[iNextPoint].weight + AStarArr[iNextPoint].h;
                                AStarArr[iNextPoint].prevPoint = iCurrPoint;//записываем номер ячейки массива!!!!!!!
                            }
                        }
                    }
                    AStarArr[iCurrPoint].isChecked = true;
                }
                else
                {
                    break;
                }
            }
        }

        private static List<long> idRest = new List<long>();
        static void ReadOsm(string addrOsm)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(addrOsm);
            XmlElement xmlRoot = xmlDoc.DocumentElement;

            maxlat = double.Parse(xmlRoot.SelectSingleNode("bounds").Attributes["maxlat"].Value, CultureInfo.InvariantCulture);
            minlon = double.Parse(xmlRoot.SelectSingleNode("bounds").Attributes["minlon"].Value, CultureInfo.InvariantCulture);
            maxlon = double.Parse(xmlRoot.SelectSingleNode("bounds").Attributes["maxlon"].Value, CultureInfo.InvariantCulture);
            minlat = double.Parse(xmlRoot.SelectSingleNode("bounds").Attributes["minlat"].Value, CultureInfo.InvariantCulture);

            XmlNodeList nodes = xmlRoot.SelectNodes("node");
            foreach (XmlNode n in nodes)
            {
                long id = long.Parse(n.SelectSingleNode("@id").Value);
                double lat = double.Parse(n.SelectSingleNode("@lat").Value, CultureInfo.InvariantCulture);
                double lon = double.Parse(n.SelectSingleNode("@lon").Value, CultureInfo.InvariantCulture);
                point Nodepoint;
                Nodepoint.lat = lat;
                Nodepoint.lon = lon;
                Nodes.Add(id, Nodepoint);
            }
            ValidHighways.Sort();
            XmlNodeList ways = xmlRoot.SelectNodes("//way[.//tag[@k = 'highway']]");
            foreach (XmlNode n in ways)
            {
                string highway = n.SelectSingleNode("tag[@k = 'highway']").Attributes["v"].Value;
                if (ValidHighways.BinarySearch(highway) >= 0)
                {
                    XmlNodeList nd = n.SelectNodes("nd");
                    List<long> id_nodes = new List<long>();
                    foreach (XmlNode m in nd)
                    {
                        long id = long.Parse(m.SelectSingleNode("@ref").Value);
                        id_nodes.Add(id);
                    }
                    for (int i = 0; i < id_nodes.Count(); ++i)
                    {
                        if (i < id_nodes.Count() - 1)
                        {
                            if (AddjestedList.ContainsKey(id_nodes[i]))
                            {
                                AddjestedList[id_nodes[i]].Add(id_nodes[i + 1]);
                            }
                            else
                            {
                                AddjestedList.Add(id_nodes[i], new List<long>());
                                AddjestedList[id_nodes[i]].Add(id_nodes[i + 1]);
                            }
                        }
                        if (i >= 1)
                        {
                            if (AddjestedList.ContainsKey(id_nodes[i]))
                            {
                                AddjestedList[id_nodes[i]].Add(id_nodes[i - 1]);
                            }
                            else
                            {
                                AddjestedList.Add(id_nodes[i], new List<long>());
                                AddjestedList[id_nodes[i]].Add(id_nodes[i - 1]);
                            }
                        }
                    }
                }
            }
            XmlNodeList restaurants = xmlRoot.SelectNodes("//node[.//tag[@k = 'amenity']]");
            int count = 0;
            foreach (XmlNode n in restaurants)
            {
                string type = n.SelectSingleNode("tag[@k = 'amenity']").Attributes["v"].Value;
                if (type == "restaurant")
                {
                    double lat = double.Parse(n.SelectSingleNode("@lat").Value, CultureInfo.InvariantCulture);
                    double lon = double.Parse(n.SelectSingleNode("@lon").Value, CultureInfo.InvariantCulture);
                    long id = findNearestPointToCoordinates(lat, lon);
                    idRest.Add(id);
                    count++;
                    if (count == 10) { idRest.Remove(id); }
                    if (count == 11) { idRest.Remove(id); }
                }
                if (count > 11)
                    break;
            }
            Console.WriteLine("ReadOSM complete!");
        }

        static void WriteToSvg(long startPointId)
        {
            System.IO.StreamWriter textFile = new System.IO.StreamWriter("graph.svg");
            textFile.WriteLine("<svg version = \"1.1\" baseProfile = \"full\" xmlns = \"http://www.w3.org/2000/svg\" >");
            ICollection<long> keys = AddjestedList.Keys;
            foreach (long i in keys)
            {
                for (int j = 0; j < AddjestedList[i].Count(); ++j)
                {
                    string LineToSvg = "<line ";
                    LineToSvg += "x1=\"" + System.Convert.ToString(CoordsWork.lonToX(Nodes[i].lon) - CoordsWork.lonToX(minlon)).Replace(",", ".") + "\" x2=\"" + System.Convert.ToString(CoordsWork.lonToX(Nodes[AddjestedList[i][j]].lon) - CoordsWork.lonToX(minlon)).Replace(",", ".") + "\" y1=\"" + System.Convert.ToString(-CoordsWork.latToY(Nodes[i].lat) + CoordsWork.latToY(maxlat)).Replace(",", ".") + "\" y2=\"" + System.Convert.ToString(-CoordsWork.latToY(Nodes[AddjestedList[i][j]].lat) + CoordsWork.latToY(maxlat)).Replace(",", ".") + "\" ";
                    LineToSvg += "stroke = \"black\" stroke-width= \"1\" />";
                    textFile.WriteLine(LineToSvg);
                }
            }

            
            string newLine = "<circle ";
            newLine += "cx=\"" + System.Convert.ToString(CoordsWork.lonToX(Nodes[startPointId].lon) - CoordsWork.lonToX(minlon)).Replace(",", ".") + "\" cy=\"" + System.Convert.ToString(-CoordsWork.latToY(Nodes[startPointId].lat) + CoordsWork.latToY(maxlat)).Replace(",", ".") + "\" r=\"20\" fill=\"blue\" />";
            textFile.WriteLine(newLine);
            
            int min_i = 0;
            for (int i = 1; i < 10; ++i)
            {
                if (AStarWeigth[i] < AStarWeigth[min_i])
                {
                    min_i = i;
                }
            }
            Console.WriteLine("To the nearest point: "+AStarWeigth[min_i]*6.2/1000+" km - "+ AStarWeigth[min_i] * 6.2 / 1000/40+" h");
      
            for (int i = 0; i < 10; ++i)
            {
                if (i == min_i)
                {
                    newLine = "<circle ";
                    newLine += "cx=\"" + System.Convert.ToString(CoordsWork.lonToX(Nodes[idRest[i]].lon) - CoordsWork.lonToX(minlon)).Replace(",", ".") + "\" cy=\"" + System.Convert.ToString(-CoordsWork.latToY(Nodes[idRest[i]].lat) + CoordsWork.latToY(maxlat)).Replace(",", ".") + "\" r=\"20\" fill=\"red\" />";
                    textFile.WriteLine(newLine);
                }
                else
                {
                    newLine = "<circle ";
                    newLine += "cx=\"" + System.Convert.ToString(CoordsWork.lonToX(Nodes[idRest[i]].lon) - CoordsWork.lonToX(minlon)).Replace(",", ".") + "\" cy=\"" + System.Convert.ToString(-CoordsWork.latToY(Nodes[idRest[i]].lat) + CoordsWork.latToY(maxlat)).Replace(",", ".") + "\" r=\"20\" fill=\"yellow\" />";
                    textFile.WriteLine(newLine);
                }
            }

            for (int i = 0; i < 10; ++i)
            {
                for (int j = 0; j < AStarWays[i].Count() - 1; ++j)
                {
                    newLine = "<line ";
                    newLine += "x1=\"" + System.Convert.ToString(CoordsWork.lonToX(Nodes[AStarWays[i][j]].lon) - CoordsWork.lonToX(minlon)).Replace(",", ".") + "\" x2=\"" + System.Convert.ToString(CoordsWork.lonToX(Nodes[AStarWays[i][j + 1]].lon) - CoordsWork.lonToX(minlon)).Replace(",", ".") + "\" y1=\"" + System.Convert.ToString(-CoordsWork.latToY(Nodes[AStarWays[i][j]].lat) + CoordsWork.latToY(maxlat)).Replace(",", ".") + "\" y2=\"" + System.Convert.ToString(-CoordsWork.latToY(Nodes[AStarWays[i][j + 1]].lat) + CoordsWork.latToY(maxlat)).Replace(",", ".") + "\" ";
                    newLine += "stroke = \"green\" stroke-width= \"7\" />";
                    textFile.WriteLine(newLine);
                }

            }
            textFile.WriteLine("</svg>");
            textFile.Close();
            Console.WriteLine("graph.svg complete!");
        }

        static void WriteToCsv()
        {
            System.IO.StreamWriter textFile = new System.IO.StreamWriter("Addjested_list.csv");
            textFile.WriteLine("Nodes;Addjested Nodes");
            ICollection<long> keys = AddjestedList.Keys;
            foreach (long i in keys)
            {
                string LineToCsv = "";
                LineToCsv += i;
                LineToCsv += ";";
                LineToCsv += "{";
                for (int j = 0; j < AddjestedList[i].Count(); ++j)
                {
                    LineToCsv += AddjestedList[i][j];
                    LineToCsv += ",";
                }
                LineToCsv += "}";
                textFile.WriteLine(LineToCsv);
            }
            textFile.Close();
            Console.WriteLine("Addjested_list.csv complete!");
        }

        static long findNearestPointToCoordinates(double lat, double lon)
        {
            long id = 0;
            double min_razl = 100000;
            ICollection<long> keys = AddjestedList.Keys;
            foreach (long i in keys)
            {
                if ((Math.Abs(Nodes[i].lat - lat) + Math.Abs(Nodes[i].lon - lon)) < min_razl)
                {
                    id = i;
                    min_razl = Math.Abs(Nodes[i].lat - lat) + Math.Abs(Nodes[i].lon - lon);
                }
            }

            return id;
        }

        static void Main(string[] args)
        {
            ReadOsm("map.osm");

            double lat = 0.0;
            while (lat < minlat || lat > maxlat)
            {
                Console.WriteLine("Start point LAT [{0} {1}] : ", minlat, maxlat);
                lat = Double.Parse(Console.ReadLine());
            }
            double lon = 0.0;
            while (lon < minlon || lon > maxlon)
            {
                Console.WriteLine("Start point LON [{0} {1}] : ", minlon, maxlon);
                lon = Double.Parse(Console.ReadLine());
            }
            long startPointId = findNearestPointToCoordinates(lat, lon);

            //---------------------------------------------------------Дейкстра---------------------------------------------------------------
            //Stopwatch watches = new Stopwatch();
            //watches.Start();

            //for (int i = 0; i < 10; ++i)
            //{
            //    DijkstraAlg(startPointId, idRest[i], i);
            //}
            
            //watches.Stop();
            //TimeSpan ts = watches.Elapsed;
            //string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            //ts.Hours, ts.Minutes, ts.Seconds,
            //ts.Milliseconds / 10);
            //Console.WriteLine("RunTime " + elapsedTime);
            //-----------------------------------------------------------Левит-----------------------------------------------------------------
            //watches.Restart();

            //LevitAlg(startPointId);
            
            //watches.Stop();
            //ts = watches.Elapsed;
            //elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            //ts.Hours, ts.Minutes, ts.Seconds,
            //ts.Milliseconds / 10);
            //Console.WriteLine("RunTime " + elapsedTime);
            //-------------------------------------------------------------А*------------------------------------------------------------------
            //watches.Restart();

            for (int i = 0; i < 10; ++i)
            {
                AStarAlg(startPointId, idRest[i], i);
            }
            
            //watches.Stop();
            //ts = watches.Elapsed;
            //elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            //ts.Hours, ts.Minutes, ts.Seconds,
            //ts.Milliseconds / 10);
            //Console.WriteLine("RunTime " + elapsedTime);

            WriteToSvg(startPointId);
            
            Console.WriteLine("\nPress ENTER to end.");
            Console.ReadLine();

            //WriteToCsv();
            //
        }
    }
}


