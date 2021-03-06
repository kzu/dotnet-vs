﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Devlooped
{
    class LogCommand : Command<LogCommandDescriptor>
    {
        readonly WhereService whereService;

        public LogCommand(LogCommandDescriptor descriptor, WhereService whereService) : base(descriptor) =>
            this.whereService = whereService;

        public override async Task ExecuteAsync(TextWriter output)
        {
            var instances = await whereService.GetAllInstancesAsync(Descriptor.Options);
            var instance = new Chooser().Choose(instances, output);

            if (instance != null)
            {
                var instanceDir = instance.InstallationVersion.Major + ".0_" + instance.InstanceId;
                if (Descriptor.IsExperimental)
                    instanceDir += "Exp";

                var path = Path.Combine(
                    Environment.ExpandEnvironmentVariables("%AppData%"),
                    @"Microsoft\VisualStudio",
                    instanceDir,
                    "ActivityLog.xml");

                if (File.Exists(path))
                    Process.Start(new ProcessStartInfo("explorer.exe", $"/select, \"{path}\"") { UseShellExecute = true });
            }
        }
    }
}
