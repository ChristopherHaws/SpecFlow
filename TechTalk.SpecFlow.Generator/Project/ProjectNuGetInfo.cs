using System;
using System.Collections.Generic;
using System.IO;
using TechTalk.SpecFlow.Generator.Configuration;
using TechTalk.SpecFlow.Generator.Interfaces;
using TechTalk.SpecFlow.Utils;

namespace TechTalk.SpecFlow.Generator.Project
{

    public class ProjectNuGetInfo
    {
        public ProjectNuGetStyle Style { get; set; }

        public String NuGetPath { get; set; }
    }
}