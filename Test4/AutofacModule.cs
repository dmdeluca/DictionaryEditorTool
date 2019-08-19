using Autofac;
using DictionaryTools;
using FriendlyLocals;
using Google.Cloud.Translation.V2;

namespace KeyTranslation
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DictionaryEditor>()
                .As<IDictionaryEditor>()
                .InstancePerLifetimeScope();
            builder.RegisterType<DictionaryEditHistoryManager>()
                .As<IDictionaryEditHistoryManager>()
                .InstancePerLifetimeScope();
            builder.RegisterType<BulkKeyTranslator>()
                .As<IBulkKeyTranslator>()
                .InstancePerLifetimeScope();
            builder.RegisterInstance(TranslationClient.Create())
                .As<TranslationClient>()
                .InstancePerLifetimeScope();
        }
    }
}
