using System;
using System.Collections.Generic;
using System.Linq;

namespace DataTransmissionProtocol
{
    public class Device : INetworkEntity
    {
        public string Name;
        public string Address;
        public int CurrentTime;
        public INetworkEntity ConnectedTo;
        public int TransmissionInterval;
        public int TimeToNextTransmission;
        public int TimeStep;
        public string Destination;
        public List<string> Buffer;
        public List<List<string>> Queue;

        public List<string> GeneratePackage()
        {
            var package = new List<string>();
            // generate package
            var from = Address;
            var to = Destination;
            var r = new Random();
            var payload = from + to + Name + r.Next();
            var timeTag = CurrentTime.ToString();
            package.Add(from);
            package.Add(to);
            package.Add(payload);
            package.Add(timeTag);
            return package;
        }

        public void Run(bool transportEnabled)
        {
            var connectedRouter = (Router) ConnectedTo;
            if (TransmissionInterval != -1) //  Device can transmit
            {
                var package = new List<string>();
                
                if (TimeToNextTransmission == 0) // time to transmit new package
                {
                    // generate new package or get from queue
                    if (Queue.Count == 0) // queue is empty -> generate new package
                    {
                        package = GeneratePackage();
                    }
                    else // queue is not empty -> get first package from it
                    {
                        package = Queue[0];
                    }
                    // make send request
                    var transmissionAllowed = false;
                    
                    if (transportEnabled)
                    {
                        transmissionAllowed = connectedRouter.TransportSendRequest(Name);   
                    }
                    else
                    {
                        transmissionAllowed = connectedRouter.SendRequest(Name);
                    }

                    if (transmissionAllowed)
                    {
                        // send to buffer of connected device
                        connectedRouter.SendToBuffer(package);
                        Console.WriteLine("[Time: " + CurrentTime + "] " + Name + " sent package " + package[2] + " to " + connectedRouter.Name);

                        if (Queue.Contains(package)) // check if package was taken from queue
                        {
                            Queue.Remove(package); // remove it
                        }
                    }
                    else
                    {
                        Console.WriteLine("[Time: " + CurrentTime + "] " + Name + " can't send package " + package[2] + " to " + connectedRouter.Name + "[Transmission prohibited]");
                        if (!Queue.Contains(package)) // if package was not taken from queue -> add it to queue
                        {
                            Queue.Add(package);    
                        }
                    }

                    TimeToNextTransmission = TransmissionInterval - 1; // update time to next transmission 
                }
                else
                {
                    TimeToNextTransmission -= TimeStep; // decrement time to next transmission
                }
            }
        }

        public void AddConnectionToPort(INetworkEntity entity, int portNumber)
        {
            ConnectedTo = entity;
        }
        

        public void SendToBuffer(List<string> data, int port=0)
        {
            Buffer = data;
        }

        public void ReceiveData()
        {
            if (Buffer.Count > 0) // if has incoming data -> receive it and clear buffer
            {
                int delay = CurrentTime - Convert.ToInt32(Buffer[3]);
                Console.WriteLine("[Time: " + CurrentTime + "] " + Name + " Got package " + Buffer[2] + " from " +
                                  Buffer[0] + "[Delay: " + Convert.ToString(delay) + "]");
                Buffer.Clear(); // clear buffer for next data
            }
        }

        public int type()
        {
            return 0;
        }

        public void MakeBusy()
        {
            throw new NotImplementedException();
        }

        public Device(string name, int transmissionInterval)
        {
            Name = name;
            Address = name;
            Buffer = new List<string>();
            TransmissionInterval = transmissionInterval;
            TimeToNextTransmission = TransmissionInterval;
            TimeStep = 1;
            Queue = new List<List<string>>();
        }
    }
}