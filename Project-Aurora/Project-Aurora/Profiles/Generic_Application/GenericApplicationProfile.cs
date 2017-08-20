﻿using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Generic_Application
{
    public class GenericApplicationProfile : ApplicationProfile
    {
        //Generic
        public string ApplicationName;

        [Newtonsoft.Json.JsonIgnore]
        public bool _simulateNighttime = false;

        [Newtonsoft.Json.JsonIgnore]
        public bool _simulateDaytime = false;

        public ObservableCollection<Layer> Layers_NightTime { get; set; }

        public GenericApplicationProfile() : base()
        {
            //Generic
            ApplicationName = "New Application Profile";
        }

        public GenericApplicationProfile(string appname) : base()
        {
            ApplicationName = appname;
        }

        public override void Reset()
        {
            base.Reset();
            Layers_NightTime = new ObservableCollection<Layer>();

        }
    }
}
