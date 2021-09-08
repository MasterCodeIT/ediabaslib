﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public interface IPsdzCoding1NcdEntry
    {
        int BlockAdress { get; }

        byte[] UserData { get; }

        bool IsWriteable { get; }
    }
}