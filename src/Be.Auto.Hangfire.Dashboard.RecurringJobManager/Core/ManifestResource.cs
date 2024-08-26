﻿using System.IO;
using System.Resources;
using Hangfire;
using Newtonsoft.Json;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core
{

    public static class ManifestResource
    {
        public static string ReadStringResource(string resourceName)
        {
            var assembly = typeof(ManifestResource).Assembly;
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new MissingManifestResourceException($"Cannot find resource {resourceName}");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
