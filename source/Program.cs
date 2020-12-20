using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DataTransmissionProtocol
{
    class Program
    {
        static void Main(string[] args)
        {
            var device1 = new Device("device1", 2); // transmission 1 time at 2 secs
            var device2 = new Device("device2", -1); // no transmission
            var device3 = new Device("device3", 3); // transmission 1 time at 3 secs
            var device4 = new Device("device4", 5); // transmission 1 time at 5 secs
            var device5 = new Device("device5", 7); // transmission 1 time at 6 secs

            var commutationTable = new Dictionary<string, string>
            {
                {"device1", "device2"}, 
                {"", ""},
                {"device3", "device2"},
                {"device4", "device2"},
                {"device5", "device2"}
            };

            var portsInfo = new Dictionary<string, int> 
            {
                {"device1", 1},
                {"device2", 2},
                {"device3", 3},
                {"device4", 4},
                {"device5", 5}
            };
            
            var timeTable = new List<List<string>>
            {
                new List<string> {"2", "3", "device1", "device2"},
                new List<string> {"4", "5", "device1", "device2"},
                new List<string> {"8", "9", "device1", "device2"},
                new List<string> {"16", "17", "device1", "device2"},
                new List<string> {"20", "21", "device1", "device2"},
                new List<string> {"22", "23", "device1", "device2"},
                new List<string> {"26", "27", "device1", "device2"},
                
                new List<string> {"3", "4", "device3", "device2"},
                new List<string> {"6", "7", "device3", "device2"},
                new List<string> {"9", "10", "device3", "device2"},
                new List<string> {"12", "13", "device3", "device2"},
                new List<string> {"18", "19", "device3", "device2"},
                new List<string> {"24", "25", "device3", "device2"},
                new List<string> {"27", "28", "device3", "device2"},
                
                new List<string> {"5", "6", "device4", "device2"},
                new List<string> {"10", "11", "device4", "device2"},
                new List<string> {"15", "16", "device4", "device2"},
                new List<string> {"25", "26", "device4", "device2"},
                new List<string> {"30", "31", "device4", "device2"},
                
                new List<string> {"7", "8", "device5", "device2"},
                new List<string> {"14", "15", "device5", "device2"},
                new List<string> {"21", "22", "device5", "device2"},
                new List<string> {"28", "29", "device5", "device2"}
            };
                
            var router1 = new Router("router1",commutationTable, portsInfo);

            router1.AddConnectionToPort(device1, 1);
            router1.AddConnectionToPort(device2, 2);
            router1.AddConnectionToPort(device3, 3);
            router1.AddConnectionToPort(device4, 4);
            router1.AddConnectionToPort(device5, 5);

            router1.TimeTable = timeTable;
            
            var network = new Network();
            network.Setup(32, 1);
            network.AddRouter(router1);
            network.Run(true);
            
            JsonSerializer serializer = new JsonSerializer();

            using (StreamWriter sw = new StreamWriter(@"json1.txt"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, network.Routers[0].DelayValues);
            }
        }
    }
}