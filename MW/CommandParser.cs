using MW.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MW
{   
    public static class CommandParser
    {
        private static List<(MethodInfo MethodInfo, CommandAttribute Command)>? _commands;
        private static List<(MethodInfo MethodInfo, CommandAttribute Command)> commands
        {
            get
            {
                if (_commands == null)
                {
                    _commands = FindCommands();
                }
                return _commands;
            }
        }

        public static bool Parse(string prompt)
        {
            var pSplit = prompt.Split([' '], 2, StringSplitOptions.RemoveEmptyEntries);
            var command = pSplit[0].ToLowerInvariant();
            var tail = pSplit.Length > 1 ? pSplit[1] : string.Empty;

            if (command == "quit")
            {
                return true;
            }

            ExecuteCommand(command, tail);

            return false;
        }

        [Command(name: "help", alt: "h", description: "Show this list")]
        public static void ShowHelp()
        {
            var commandNameMaxLength = commands.Max(c => c.Command.NameWithAlt.Length);
            var commandArgumentMaxLength = commands.Max(c => c.Command.Arguments.Length);

            Show.Info(Tab("quit | q", commandNameMaxLength + 2) + Tab("", commandArgumentMaxLength + 2) + "Quit application");

            foreach (var command in commands)
            {
                Show.Info(Tab(command.Command.NameWithAlt, commandNameMaxLength + 2) + Tab(command.Command.Arguments, commandArgumentMaxLength + 2) + command.Command.Description);
            }
        }

        private static List<(MethodInfo MethodInfo, CommandAttribute Command)> FindCommands()
        {
            var asm = Assembly.GetExecutingAssembly();

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

            return asm
                .GetTypes()
                .SelectMany(t => t.GetMethods(flags))
                .Select(m => (Method: m, Attr: m.GetCustomAttribute<CommandAttribute>(inherit: false)!))
                .Where(x => x.Attr != null).ToList();
        }

        private static void ExecuteCommand(string commandName, string tail)
        {
            var command = commands.FirstOrDefault(c => c.Command.Name == commandName || c.Command.Alt == commandName);

            if (command != default)
            {
                var parameters = command.MethodInfo.GetParameters();

                // Zero parameters
                if (parameters.Length == 0)
                {
                    command.MethodInfo.Invoke(null, []);
                }
                else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                {
                    command.MethodInfo.Invoke(null, [tail]);
                }
                else
                {
                    Show.Error("Command not compatible");
                }
            }
            else
            {
                Show.Info($"Unknown command '{commandName}'"); 
            }
        }

        private static string Tab(string word, int length)
        {
            var padding = "                                        ".Substring(1, length - word.Length);
            return word + padding;
        }

    }
}
