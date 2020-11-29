using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace getingPeopleCount
{
    public class people_in_train
    {
        public ObjectId Id { get; set; }
        public string station_id { get; set; }
        public string train_id { get; set; }
        public string time { get; set; }
        public string date { get; set; }
        public string inPeople { get; set; }
        public string ounPeople { get; set; }
    }
    class Program
    {
        private static int k;

        static void Main(string[] args)
        {

            List<int[,]> IRMatrixList1 = getTemperatureMatrix();
            //List<int[,]> IRMatrixList2 = getTemperatureMatrix();
            //List<int[,]> IRMatrixList3 = getTemperatureMatrix();
            setPeopleCountDocumentsAsync(initMongoPeopleCounCollection(), peopleCountTemperatureMatrix1(IRMatrixList1.ElementAt(0)));
            //peopleCountTemperatureMatrix2(IRMatrixList1.ElementAt(1));
            //logicFunc();
        }

        public static IMongoCollection<people_in_train> initMongoPeopleCounCollection()
        {
            MongoClient dbClient = new MongoClient("mongodb://localhost:27017/?readPreference=primary&appname=MongoDB%20Compass&ssl=false");

            var database = dbClient.GetDatabase("dbTimesAndStations");
            var collection = database.GetCollection<people_in_train>("people_in_train");
            bool isMongoLive = database.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait(1000);

            return collection;
        }
        public static async Task setPeopleCountDocumentsAsync(IMongoCollection<people_in_train> collection, int[] counts)
        {
            DateTime localDate = DateTime.Now;
            people_in_train ticket = new people_in_train();
            string str = "";
            using (var sr = new StreamReader("buf.txt"))
            {
                str = sr.ReadLine();
            }
            string[] index = str.Split(new char[] { ' ' });

            ticket.station_id = "1";
            ticket.train_id = "1";
            ticket.inPeople = counts[1].ToString();
            ticket.ounPeople = counts[2].ToString();
            ticket.time = localDate.Hour.ToString() + ":" + localDate.Minute.ToString() + ":" + localDate.Second.ToString();
            ticket.date = localDate.Day.ToString() + "." + localDate.Month.ToString() + "." + localDate.Year.ToString();
            await collection.InsertOneAsync(ticket);
        }
    

    static void logicFunc()
        {
            SerialPort port = connectToPort();
            if (port!=null)
            {
                //Thread.Sleep(1000);
                string a = read(port,'a', 3).ToString();

                port.Close();
            }
        }
        static char[] read(SerialPort port, char startByte, int count)
        {
            char[] ch = new char[count];
            if (port.IsOpen)
            {
                int buferSize = port.BytesToRead;
                bool startRead = false;
                int a = 0;
                for (int i = 0; i < buferSize; ++i)
                {
                    //  читаем по одному байту
                    char bt = (char)port.ReadByte();
                    if (bt== startByte)
                    {
                        startRead = true;
                    }
                    if (startRead)
                    {
                        if (a < count)
                        {
                            ch[a] = bt;
                            a++;
                        }
                    }
                }
            }
            return ch;
        }
        static void write(SerialPort port, string toCOM)
        {
            try
            {
                port.Write(toCOM);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        static SerialPort connectToPort()
        {
            SerialPort port = new SerialPort();
            string[] ports = SerialPort.GetPortNames();
            if(ports.Length != 0)
            {
                for (int i = 0; i < ports.Length; i++)
                {
                    Console.WriteLine("[" + i.ToString() + "] " + ports[i].ToString());
                }
                string n = Console.ReadLine();
                int num = int.Parse(n);
                try
                {
                    port.PortName = ports[num];
                    port.BaudRate = 9600;
                    port.DataBits = 8;
                    port.Parity = System.IO.Ports.Parity.None;
                    port.StopBits = System.IO.Ports.StopBits.One;
                    port.ReadTimeout = 1000;
                    port.WriteTimeout = 1000;
                    port.Open();
                    return port;
                }
                catch (Exception e)
                {
                    sendErrorToLogFile("ERROR: невозможно открыть порт:" + e.ToString());
                    return null;
                }
            }
            sendErrorToLogFile("No COMPort");
            return null;
        }
        static void sendErrorToLogFile(string txt)
        {
                using (StreamWriter sw = new StreamWriter("log.txt", true))
                {
                    sw.WriteLine(txt);
                }
        }

        static List<int[,]> getTemperatureMatrix()
        {
            int[,] tambureDoorMatrix = new int[8,8];
            int[,] inDoorMatrix = new int[8,8];
            int[,] outDoorMatrix = new int[16,8];
            List<int[,]> list = new List<int[,]>();
            Random rnd = new Random();

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    tambureDoorMatrix[i, j] = rnd.Next(13, 16);
                    Console.Write(tambureDoorMatrix[i, j] + " ");
                }
                Console.WriteLine("");
            }
            list.Add(tambureDoorMatrix);

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    inDoorMatrix[i, j] = rnd.Next(13, 16);
                }
            }
            list.Add(inDoorMatrix);

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    
                    outDoorMatrix[i, j] = rnd.Next(13, 16);

                }
            }
            list.Add(outDoorMatrix);

            return list;
        }

        static int[] peopleCountTemperatureMatrix1(int[,] door)
        {
            int[] s = new int[3];
            int sum = 0;

            int dirl = 0;
            int dirr = 0;

            int dirSuma = 0;
            int dirSumb = 0;
            int people_count = 0;
            int new_sum = 0;
            int prevnew_sum = 0;
            Random rnd = new Random();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    sum += door[i, j];
                }
            }
            int a = 8;
            int b = 8;
            int c = 8;
            int d = 8;

            int k = rnd.Next(1, 20);

            while (k > 0)
            {
                prevnew_sum = new_sum;
                new_sum = 0;
                int rand1 = rnd.Next(0, 2);
                
                if (rand1 == 1)
                {
                    a = 3 + rnd.Next(-3, 1);
                    b = 6 + rnd.Next(-1, 1);
                }
                else
                {
                    a = 8;
                    b = 8;
                }
                for (int i = b; i < 8; i++)
                {
                    for (int j = a; j < a + 3; j++)
                    {
                        door[i, j] = rnd.Next(36, 37);
                    }
                }
                if (d != 8)
                {
                    d = d - 4;
                    for (int i = d; i < d + 3; i++)
                    {
                        for (int j = c; j < c + 4; j++)
                        {
                            door[i, j] = rnd.Next(36, 37);
                        }
                    }
                }
                Thread.Sleep(500);
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        dirSuma = dirSuma + door[i, j];
                    }
                }
                for (int i = 4; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        dirSumb = dirSumb + door[i, j];
                    }
                }
                //
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        new_sum += door[i, j];
                    }
                }
                if (prevnew_sum-sum < 200 && new_sum - sum > 200 && dirSuma > dirSumb)
                {
                    dirl++;
                }
                if (prevnew_sum - sum < 200 && new_sum - sum > 200 && dirSuma <dirSumb)
                {
                    dirr++;
                }prevnew_sum = new_sum;
                new_sum = 0;
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        Console.Write(door[i, j] + " ");
                        door[i, j] = rnd.Next(14, 17);

                    }
                    Console.WriteLine("");
                }
                Console.WriteLine("");
                int rand2 = rnd.Next(0, 1);
                if (rand2 == 1)
                {
                    c = 3 + rnd.Next(-3, 1);
                    d = 6 + rnd.Next(-1, 1);
                }
                else
                {
                    c = 8;
                    d = 8;
                }
                
                for (int i = d; i < 8; i++)
                {
                    for (int j = c; j < c + 2; j++)
                    {
                        door[i, j] = rnd.Next(35, 36);
                    }
                }
                if (b != 8)
                {
                    b = b - 4;
                    for (int i = b; i < b + 3; i++)
                    {
                        for (int j = a; j < a + 4; j++)
                        {
                            door[i, j] = rnd.Next(36, 37);
                        }
                    }
                }
                prevnew_sum = new_sum;
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        new_sum += door[i, j];
                    }
                }
                if (new_sum - sum > 400)
                {
                    people_count = people_count + 2;
                }
                else if (new_sum - sum < 400 && new_sum - sum > 200)
                {
                    people_count = people_count + 1;
                }
                Thread.Sleep(500);
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        Console.Write(door[i, j] + " ");
                        door[i, j] = rnd.Next(14, 17);
                    }
                    Console.WriteLine("");
                }
                k--;
                Console.WriteLine("");
            }
            Console.WriteLine(people_count);
            Console.WriteLine(people_count-dirr);
            Console.WriteLine(dirr);
            s[0] = people_count;
            s[1] = people_count-dirr;
            s[2] = dirr;
            return s;
            }
       
    }
    }

