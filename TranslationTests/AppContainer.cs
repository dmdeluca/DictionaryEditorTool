using Autofac;
using DictionaryTools;
using FriendlyLocals;
using Google.Cloud.Translation.V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TranslationTests
{
    public class App
    {
        private bool _alreadyBuilt;
        private ILifetimeScope _scope;

        public App()
        {
            _alreadyBuilt = false;
        }
        public IComponentContext Build()
        {
            var builder = new ContainerBuilder();

            var dictionaryParameterName = "dictionary";

            builder.Register(
                (context, parameters) => 
                new DictionaryEditHistoryManager(parameters.Named<Dictionary<string, string>>(dictionaryParameterName)))
                .As<IDictionaryEditHistoryManager>()
                .InstancePerLifetimeScope();

            builder.Register(
                (context, parameters) =>
                {
                    var dictionary = parameters.Named<Dictionary<string, string>>(dictionaryParameterName);
                    var historyManager = context.Resolve<IDictionaryEditHistoryManager>(new NamedParameter(dictionaryParameterName, dictionary));
                    return new DictionaryEditor(dictionary, historyManager);
                })
                .As<IDictionaryEditor>()
                .InstancePerLifetimeScope();

            foreach (var translationModel in Enum.GetValues(typeof(TranslationModel)).Cast<TranslationModel>())
            {
                builder.RegisterInstance(new BulkKeyTranslator(translationModel))
                    .Keyed<IBulkKeyTranslator>(translationModel);
            }

            builder.RegisterInstance(TranslationClient.Create())
                .As<TranslationClient>();

            return builder.Build();
        }

        public T Get<T>()
            where T: class
        {
            if (_alreadyBuilt)
                return _scope.Resolve<T>();
            return Build().Resolve<T>();
        }

        public T Get<T>(object name)
            where T : class
        {
            if (_alreadyBuilt)
                return _scope.ResolveKeyed<T>(name);
            return Build().ResolveKeyed<T>(name);
        }

    }
}
