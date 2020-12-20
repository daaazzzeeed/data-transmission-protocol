using System;
using System.Collections.Generic;
using System.Linq;

namespace DataTransmissionProtocol
{
    public class Router : INetworkEntity
    {
        public string Name;
        public int CurrentTime;
        public Dictionary<int, Dictionary<bool, string>> TransmissionInProgress;  // port - if transmission is in progress
        public int TimeStep;

        public Dictionary<int, bool> PortsStates; // key - port number, value - is port state: true - busy, false - free

        public Dictionary<int, List<string>> Buffer; // Queue with port as key and package as value
        public Dictionary<string, int> PortsInfo; // device name - port number
        public Dictionary<int, INetworkEntity> ConnectedDevices; // port - device instance
        public Dictionary<string, string> CommutationTable; // from device name - to device name
        public List<List<string>> TimeTable;
        
        public Dictionary<int, List<string>> Queue; // queue for packages with port as key 
        public Dictionary<string, Dictionary<int, int>> DelayValues; // Delay values matrix: key - device sender name, value - dict
                                                                  // where key - time,
                                                                  // value - delay value

        public bool SendRequest(string deviceName)
        {
            if (PortsStates[PortsInfo[deviceName]] || PortsStates[PortsInfo[CommutationTable[deviceName]]])
            {
                // if input port is busy or target port is busy -> decline 
                return false;
            }

            // if input port is free and target port is free -> allow
            // mark input and target port as busy
            MakeBusy(deviceName);
            return true;
        }
        
        public void Run()
        {
            var from = 0;
            var to = 1;
            
            // go through each port
            foreach (var port in Buffer.ToList())
            {
                // check if data has appeared in port
                if (port.Value.Count > 0)
                {
                    // route data to target port
                    var package = port.Value;
                    Console.WriteLine("[Time: " + CurrentTime + "] " + Name + " got package " + 
                                      package[2] + " from " + package[0]);
                    var targetDevice = package[to];
                    var originPort = PortsInfo[package[from]];
                    var targetPort = PortsInfo[targetDevice];

                    var delay = CurrentTime - Convert.ToInt32(package[3]);
                    var originAddress = package[0];
                    
                    ConnectedDevices[targetPort].SendToBuffer(package);
                    Console.WriteLine("[Time: " + CurrentTime + "] " + Name + " sent package " + 
                                      package[2] + " to " + package[1]);
                    ((Device)ConnectedDevices[targetPort]).ReceiveData();
                    

                    
                    if (!DelayValues.ContainsKey(originAddress))
                    {
                        DelayValues.Add(originAddress, new Dictionary<int, int> {{CurrentTime, delay}});
                    }
                    else
                    {
                        DelayValues[originAddress].Add(CurrentTime, delay);
                    }
                    
                    PortsStates[targetPort] = false;
                    PortsStates[originPort] = false;
                }
            }
        }


        public bool TransportSendRequest(string deviceName)
        {
            const int time1 = 0;
            const int time2 = 1;
            const int originAddr = 2;
            const int destAddr = 3;
            
            foreach (var timeTableRecord in TimeTable)
            {
                if (CurrentTime >= Convert.ToInt32(timeTableRecord[time1]) && // if stays in secured time slot
                    CurrentTime <= Convert.ToInt32(timeTableRecord[time2]))
                {
                    if (timeTableRecord[originAddr] == deviceName && // if route equals to secured virtual channel
                        timeTableRecord[destAddr] == CommutationTable[deviceName])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void SendData()
        {
            throw new System.NotImplementedException();
        }

        public void SendToBuffer(List<string> data, int port = 0)
        {
            if (port == 0)
            {
                if (!Buffer.ContainsKey(PortsInfo[data[0]]))
                {
                    Buffer.Add(PortsInfo[data[0]], new List<string>());
                }

                Buffer[PortsInfo[data[0]]] = data;
            }
            else
            {
                Buffer[port] = data;
            }
        }

        public void AddConnectionToPort(INetworkEntity entity, int portNumber)
        {
            ConnectedDevices.Add(portNumber, entity);
            TransmissionInProgress.Add(portNumber, new Dictionary<bool, string> {{false, ""}});
            Queue.Add(portNumber, new List<string>());
            PortsStates.Add(portNumber, false);
            
            if (entity.type() == 0)
            {
                entity.AddConnectionToPort(this);
                var device = (Device) entity;
                var deviceIndex = Convert.ToInt32(device.Name.Split("ce")[1])-1;

                if (device.TransmissionInterval != -1)
                {
                    device.Destination = CommutationTable.Values.ToList()[deviceIndex];   
                }
            }

            if (entity.type() == 1)
            {
                entity.AddConnectionToPort(this, portNumber);   
            }
        }

        public int type()
        {
            return 1;
        }

        public void MakeBusy(string deviceName)
        {
            PortsStates[PortsInfo[deviceName]] = true;
            PortsStates[PortsInfo[CommutationTable[deviceName]]] = true;
        }

        public void Free(string deviceName)
        {
            PortsStates[PortsInfo[deviceName]] = false;
            PortsStates[PortsInfo[CommutationTable[deviceName]]] = false;
        }

        public Router(string name, Dictionary<string, string> commutationTable,
            Dictionary<string, int> portsInfo)
        {
           // WorkingWithDevices = new Dictionary<string, int>();
            Name = name;
            Buffer = new Dictionary<int, List<string>>();
            ConnectedDevices = new Dictionary<int, INetworkEntity>();
            CommutationTable = new Dictionary<string, string>();
            TransmissionInProgress = new Dictionary<int, Dictionary<bool, string>>();
            PortsInfo = portsInfo;
            CommutationTable = commutationTable;
            TimeStep = 1;
            Queue = new Dictionary<int, List<string>>();
            TimeTable = new List<List<string>>();
            PortsStates = new Dictionary<int, bool>();
            DelayValues = new Dictionary<string, Dictionary<int, int>>();
        }
        
    }
}