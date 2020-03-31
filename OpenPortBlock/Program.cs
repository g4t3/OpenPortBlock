using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace OpenPortBlock
{
    class Program
    {
        // Code by G4t3
        static readonly string[] help = 
        {
            "%help = Shows all commands.",
            "%list = Shows the list of all blocked ports.",
            "%block <port> = Blocks the entered port.",
            "%unblock <port> = Unblocks the entered port.",
            "%activate = Enables the port blocker.",
            "%deactivate = Disables the port blocker." 
        };

        static Dictionary<int, BlockPort> portblock = new Dictionary<int, BlockPort>();

        static void Main(string[] args)
        {
            Console.Title = "OpenPortBlock";
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("OpenPortBlock by G4t3");
            Console.WriteLine("%help = Commands");
            Console.WriteLine();

            FirstStart();
        }

        static void FirstStart()
        {
            if (!File.Exists("opp.installed") || !File.Exists("portlist.txt") || !Directory.Exists("portlog"))
            {
                string content_opp = "OpenPortBlock installed. Do not delete this file.",
                    content_list = "";

                File.WriteAllText("opp.installed", content_opp);
                File.WriteAllText("portlist.txt", content_list);
                Directory.CreateDirectory("portlog");

                Console.WriteLine("First start.");

                Input();
            }
            else
            {
                Console.WriteLine("Loading ports...");
                int[] port = null;
                try { port = Array.ConvertAll(File.ReadAllText("portlist.txt").Split(','), int.Parse); }
                catch 
                {
                    Console.WriteLine("Error => portlist (empty or syntax(1,2,3) error)");
                    Console.ReadKey();
                    Environment.Exit(0x1);
                }

                foreach (int _port in port)
                    if (!portblock.ContainsKey(_port))
                    {
                        Thread.Sleep(20);
                        BlockPort block = new BlockPort(_port);
                        block.Activated = true;
                        portblock.Add(_port, block);
                    }
                    else
                        Console.WriteLine($"Port {port} is already exists.");

                Thread.Sleep(100);

                Console.Write("Loaded. (");
                Array.ForEach(port, x => Console.Write(x + ","));
                Console.Write(")");
                Console.WriteLine();

                Input();
            }     
        }

        static void Input()
        {
            while (true)
            {
                string input = Console.ReadLine();
                if (input.StartsWith("%"))
                    Command(input);
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Use the prefix '%' to enter a command.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
        }

        static void Command(string _input)
        {
            if (!_input.Contains("help"))
            {
                string raw = _input,
                     arguments = "";
                bool isInputRight = true;

                try { _input = _input.Substring(1, _input.IndexOf(" ")).Replace(" ",""); }
                catch { _input = _input.Remove(0, 1); }

                arguments = raw.Remove(0, _input.Length + 2);

                if ((_input.Equals("block") | _input.Equals("unblock")) && (arguments.Length == 0 | arguments.Length == 1))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("This Command needs an argument. Try it again.");
                    Console.WriteLine(help[2]);
                    Console.WriteLine(help[3]);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    isInputRight = false;
                }

                if (isInputRight)
                    switch (_input)
                    {
                        case "list":        Command_List();                             break;
                        case "block":       Command_Block(arguments);                   break;
                        case "unblock":     Command_Unblock(arguments);                 break;
                        case "activate":    Command_Activate();                         break;
                        case "deactivate":  Command_Deactivate();                       break;
                        default:            Console.WriteLine("Command not found.");    break;
                    }
            }
            else
                foreach (string command in help)
                    Console.WriteLine(command);  
        } 

        static void Command_List()
        {
            int count = 0;

            foreach (int port in Array.ConvertAll(File.ReadAllText("portlist.txt").Split(','), int.Parse))
                Console.WriteLine($"[{++count}] {port}");

            count = 0;
        }

        static void Command_Block(string _arguments)
        {
            foreach (int port in Array.ConvertAll(_arguments.Split(','), int.Parse))
                if (!portblock.ContainsKey(port))
                {
                    string list = File.ReadAllText("portlist.txt");
                    list += $",{port}"; 

                    File.WriteAllText("portlist.txt", list);

                    BlockPort block = new BlockPort(port);
                    block.Activated = true;

                    portblock.Add(port, block);
                } 
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Port {port} is already exists.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                    
        }

        static void Command_Unblock(string _arguments)
        {
            int count = 0;

            foreach (int port in Array.ConvertAll(_arguments.Split(','), int.Parse))
                if (portblock.ContainsKey(port))
                {
                    portblock[port].Activated = false;
                    portblock.Remove(port);

                    File.Delete($"portlog/{port}.log");

                    string list = File.ReadAllText("portlist.txt");

                    try { list = list.Replace($"{port},", ""); } 
                    catch { list = list.Replace($"{port}", ""); };

                    File.WriteAllText("portlist.txt", list);

                    Console.WriteLine($"[{++count}] Port {port}: Unblocked");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Port {port} is not exists.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
        }

        static void Command_Activate()
        {
            foreach (BlockPort blockPort in portblock.Values)
                if (!blockPort.GeneralActivation)
                    blockPort.GeneralActivation = true;
                else Command_Deactivate();
            Console.WriteLine("PortBlock activated.");
        }

        static void Command_Deactivate()
        {
            foreach (BlockPort blockPort in portblock.Values)
                if (blockPort.GeneralActivation)
                    blockPort.GeneralActivation = false;
                else Command_Activate();
            Console.WriteLine("PortBlock deactivated.");
        }
    }
}
