using NLog;
using PsISEProjectExplorer.UI.ViewModel;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PsISEProjectExplorer.Config
{
    public class ApplicationConfig
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private Container container;

        public void ConfigureApplication(ProjectExplorerWindow mainWindow)
        {
            this.ConfigureLogging();
            this.container = this.ConfigureDependencyInjection(mainWindow);
            Application.Current.Dispatcher.UnhandledException += DispatcherUnhandledExceptionHandler;
        }

        public T GetInstance<T>() where T : class
        {
            return this.container.GetInstance<T>();
        }

        private void ConfigureLogging()
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string logFileName = Path.Combine(assemblyFolder, "NLog.config");
            var config = new NLog.Config.XmlLoggingConfiguration(logFileName);
            LogManager.Configuration = config;
        }

        private Container ConfigureDependencyInjection(ProjectExplorerWindow mainWindow)
        {
            Container container = new Container();
            var components = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && Attribute.IsDefined(t, typeof(Component)));

            foreach (var component in components)
            {
                container.Register(component, component, Lifestyle.Singleton);
            }
            container.RegisterSingleton(typeof(ProjectExplorerWindow), mainWindow);
            container.RegisterSingleton(typeof(ApplicationConfig), this);
            return container;
        }

        private static void DispatcherUnhandledExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            Exception e = args.Exception;
            Logger.Error(e, "Unhandled Dispatcher exception");

            StringBuilder sources = new StringBuilder().Append("Sources: ");
            string firstSource = null;
            var innerException = e.InnerException;

            while (innerException != null)
            {
                if (firstSource == null)
                {
                    firstSource = innerException.Source;
                }

                sources.Append(innerException.Source).Append(",");
                innerException = innerException.InnerException;
            }

            Logger.Error(sources.ToString());
            args.Handled = true;
        }
    }
}
