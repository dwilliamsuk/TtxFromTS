using System;
using System.Collections.Generic;
using Cinegy.TsDecoder.TransportStream;
using Cinegy.TsDecoder.Tables;

namespace TtxFromTS.DVB
{
    /// <summary>
    /// Wraps the <see cref="T:Cinegy.TsDecoder.Tables.ServiceDescriptionTableFactory"/> class,
    /// changing the return of SDT tables from an event to a method return of a list of <see cref="T:TtxFromTS.DVB.Service"/>.
    /// </summary>
    public class SDTFactory : ServiceDescriptionTableFactory
    {
        private bool _changed = false;

        public SDTFactory() => TableChangeDetected += SDTChanged;

        private void SDTChanged(object sender, TransportStreamEventArgs args)
        {
            if (!ServiceDescriptionTable.ItemsIncomplete)
            {
                _changed = true;
            }
        }

        public new List<Service>? AddPacket(TsPacket packet)
        {
            base.AddPacket(packet);
            if (_changed)
            {
                List<Service> services = new List<Service>();
                foreach (ServiceDescriptionItem serviceInfo in ServiceDescriptionItems)
                {
                    if (serviceInfo.RunningStatus != 0 && serviceInfo.RunningStatus != 4)
                    {
                        continue;
                    }
                    ServiceDescriptor? serviceDescriptor = (ServiceDescriptor?)serviceInfo.Descriptors.Find(x => x.DescriptorTag == 0x48);
                    Service service = new Service
                    {
                        PID = serviceInfo.ServiceId,
                        Name = serviceDescriptor != null ? serviceDescriptor.ServiceName.ToString() : string.Empty
                    };
                    services.Add(service);
                }
                return services;
            }
            else
            {
                return null;
            }
        }
    }
}
