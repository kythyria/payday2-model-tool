using System.Collections.Generic;

namespace PD2ModelParser.Importers
{
    public interface IOptionReceiver
    {
        void AddOption(string name, string value);

        string GetOption(string name);
    }

    public class GenericOptionReceiver : IOptionReceiver
    {
        public Dictionary<string, string> Options { get; private set; } = new Dictionary<string, string>();
        public void AddOption(string name, string value) => Options.Add(name, value);
        public string GetOption(string name) => Options.GetValueOrDefault(name, null);
    }
}
