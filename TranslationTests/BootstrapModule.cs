using Autofac;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TranslationTests
{
    public class BootstrapModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Register dependencies, populate the services from
            // the collection, and build the container. If you want
            // to dispose of the container at the end of the app,
            // be sure to keep a reference to it as a property or field.
            var includeAssemblies = new List<string>
            {
                "KeyTranslation.dll"
            };

            var appComponentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            // Scan for assemblies containing autofac modules in the bin folder
            var assemblies = Directory.EnumerateFiles(appComponentPath, "*.dll", SearchOption.AllDirectories)
                .Where(filename => includeAssemblies.Any(x => x == Path.GetFileName(filename)))
                .Select(Assembly.LoadFrom);

            foreach (var assembly in assemblies)
            {
                builder.RegisterAssemblyModules(assembly);
            }
        }
    }
}