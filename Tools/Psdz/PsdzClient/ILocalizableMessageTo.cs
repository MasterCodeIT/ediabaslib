﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface ILocalizableMessageTo : ILocalizableMessage
    {
        string Description { get; }
    }
}