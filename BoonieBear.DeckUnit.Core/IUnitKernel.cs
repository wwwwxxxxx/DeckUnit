﻿using TinyMetroWpfLibrary.Controller;

namespace BoonieBear.DeckUnit.ICore
{
    public interface IUnitKernel:IKernel
    {
            IMessageController MessageController { get; }
        
    }
}