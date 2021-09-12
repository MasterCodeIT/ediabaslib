﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;

namespace PsdzClient.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IFArtExt : INotifyPropertyChanged
    {
        long F_ART_NR { get; }

        string F_ART_TEXT { get; }
    }
}