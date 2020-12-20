using System.Collections.Generic;

namespace DataTransmissionProtocol
{
    public interface INetworkEntity
    {
        void AddConnectionToPort(INetworkEntity entity, int portNumber=0);

        void SendToBuffer(List<string> data, int port=0);

        int type();
        
    }
}