using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
    public interface IFilter
    {
        DisplayObject target { get; set; }

        void Update();

        void Dispose();
    }
}