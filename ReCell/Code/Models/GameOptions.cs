﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/**
 * 
 * Author: co
 * 
 **/

namespace Recellection.Code.Models
{
    public sealed class GameOptions : IModel
    {
        /**
         * 
         * varje GameOptions objekt har lagrar alla options i spelet som variabler och varje variabel ska ha en get och set metod.
         * Det är upp till andra modular att kolla vílka options som är aktuella och rätta sig efter det.
         * 
         * TODO: alla options :P
         **/

           
        public static GameOptions Instance { get; private set; }
        
        /// <summary>
        /// Static Constructors are automatically initialized on reference.
        /// </summary>
        static GameOptions() { Instance = new GameOptions(); }

    }
}