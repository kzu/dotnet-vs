﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VisualStudio
{
    class InstallerService
    {
        public async Task RunAsync(string command, Channel? channel, Sku? sku, IEnumerable<string> args, TextWriter output)
        {
            var uri = new StringBuilder("https://aka.ms/vs/16/");
            if (channel == Channel.Preview)
                uri = uri.Append("pre/");
            else if (channel == Channel.IntPreview)
                uri = uri.Append("intpreview/");
            else if (channel == Channel.Master)
                uri = uri.Append("int.master/");
            else
                uri = uri.Append("release/");

            uri = uri.Append("vs_");
            switch (sku)
            {
                case Sku.Community:
                    uri = uri.Append("community");
                    break;
                case Sku.Professional:
                    uri = uri.Append("professional");
                    break;
                case Sku.Enterprise:
                    uri = uri.Append("enterprise");
                    break;
                default:
                    break;
            }
            uri = uri.Append(".exe");

            var bootstrapper = await DownloadAsync(uri.ToString(), output);

            var psi = new ProcessStartInfo(bootstrapper);

            // install command should be empty
            if (command != "install")
                psi.ArgumentList.Add(command);

            foreach (var arg in args)
                psi.ArgumentList.Add(arg);

            psi.Log(output);
            var process = Process.Start(psi);
            process.WaitForExit();
        }

        async Task<string> DownloadAsync(string bootstrapperUrl, TextWriter output)
        {
            using (var client = new HttpClient())
            {
                output.WriteLine($"Downloading {bootstrapperUrl}...");
                var request = new HttpRequestMessage(HttpMethod.Get, bootstrapperUrl);
                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                var filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), new Uri(bootstrapperUrl).Segments.Last());
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                using (var httpStream = await response.Content.ReadAsStreamAsync())
                {
                    using (var fileStream = File.Create(filePath))
                    {
                        await httpStream.CopyToAsync(fileStream, 8 * 1024);
                    }
                }

                return filePath;
            }
        }
    }
}