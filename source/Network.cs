using System;
using System.Collections.Generic;

namespace DataTransmissionProtocol
{
    public class Network
    {
        public int RunTime;
        public int CurrentTime;
        public int TimeStep;
        public List<Router> Routers;

        public Network()
        {
            Routers = new List<Router>();
        }

        public void Setup(int runTime, int timeStep)
        {
            RunTime = runTime;
            TimeStep = timeStep;
            CurrentTime = 0;
        }

        public void Run(bool TransportEnabled = false)
        {
            while (CurrentTime <= RunTime)
            {
                foreach (var router in Routers)
                {
                    router.CurrentTime = CurrentTime;
                    
                    foreach (var port in router.ConnectedDevices)
                    {
                        Device device = (Device)port.Value;
                        device.CurrentTime = CurrentTime;
                        device.Run(TransportEnabled);
                    }
                    
                    router.Run();
                }

                CurrentTime += TimeStep;
            }
        }

        public void AddRouter(Router router)
        {
            Routers.Add(router);
        }
    }
}