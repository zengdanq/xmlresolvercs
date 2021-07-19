using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using NLog;

namespace Org.XmlResolver {
    public class ResolverLogger {
        public const string REQUEST = "request";
        public const string RESPONSE = "response";
        public const string TRACE = "trace";
        public const string ERROR = "error";
        public const string CACHE = "cache";
        public const string CONFIG = "config";
        public const string WARNING = "warning";

        const int DEBUG = 1;
        const int INFO = 2;
        const int WARN = 3;

        private readonly Logger logger;
        private readonly Dictionary<string, int> categories = new();
        private string catalogLogging = null;

        public ResolverLogger(Logger logger) {
            this.logger = logger;
        }

        public string GetCategory(string cat) {
            if (categories.ContainsKey(cat)) {
                if (INFO.Equals(categories[cat])) {
                    return "info";
                } else if (WARN.Equals(categories[cat])) {
                    return "warn";
                }
            }
            return "debug";
        }

        public void SetCategory(string cat, string level) {
            if ("info".Equals(level)) {
                categories.Add(cat, INFO);
            } else if ("warn".Equals(level)) {
                categories.Add(cat, WARN);
            } else {
                categories.Add(cat, DEBUG);
                if (!"debug".Equals(level)) {
                    logger.Info(string.Format("Incorrect logging level specified: {0} treated as debug", level));
                }
            }
        }

        public void Log(string cat, string message, params string[] args) {
            updateLoggingCategories();

            StringBuilder sb = new();
            sb.Append(cat);
            sb.Append(":");

            if (args.Length == 0) {
                sb.Append(message);
            } else {
                sb.Append(string.Format(message, args));
            }

            var defLevel = categories.GetValueOrDefault("*", DEBUG);
            var level = categories.GetValueOrDefault(cat, defLevel);

            switch (level) {
                case WARN:
                    logger.Warn(sb.ToString());
                    break;
                case INFO:
                    logger.Info(sb.ToString());
                    break;
                default:
                    logger.Debug(sb.ToString());
                    break;
            }
        }

        private void updateLoggingCategories() {
            string property = System.Environment.GetEnvironmentVariable("XML_CATALOG_LOGGING");
            if (property == null && catalogLogging == null) {
                return;
            }

            if (property == null) {
                categories.Clear();
                return;
            }

            if (property.Equals(catalogLogging)) {
                return;
            }

            catalogLogging = property;
            categories.Clear();
            foreach (var prop in property.Split(",")) {
                int pos = prop.IndexOf(":");
                if (pos > 0) {
                    var cat = prop.Substring(0, pos).Trim();
                    var level = prop.Substring(pos + 1).Trim();
                    SetCategory(cat, level);
                }
            }
        }
    }
}