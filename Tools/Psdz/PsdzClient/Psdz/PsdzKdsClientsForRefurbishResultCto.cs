﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [KnownType(typeof(PsdzKdsIdCto))]
    [KnownType(typeof(PsdzKdsFailureResponseCto))]
    [DataContract]
    public class PsdzKdsClientsForRefurbishResultCto : IPsdzKdsClientsForRefurbishResultCto
    {
        [DataMember]
        public IPsdzKdsFailureResponseCto KdsFailureResponse { get; set; }

        [DataMember]
        public IList<IPsdzKdsIdCto> KdsIds { get; set; }
    }
}