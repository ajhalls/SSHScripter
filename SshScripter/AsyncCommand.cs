using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Renci.SshNet;

namespace SshScripter
{
    public static class SshCommandExtensions
    {
        public static void WriteStream(string cmd, ShellStream stream)
        {
            stream.WriteLine(cmd + "; echo this-is-the-end;");
            while (stream.Length == 0)
                Thread.Sleep(500);
        }

        public static string ReadStream(ShellStream stream)
        {
            StringBuilder result = new StringBuilder();

            string line;
            while ((line = stream.ReadLine()) != "this-is-the-end")
                result.AppendLine(line);

            return result.ToString();
        }

        public static void SwithToRoot(string password, ShellStream stream)
        {
            // Get logged in and get user prompt
            string prompt = stream.Expect(new Regex(@"[$>]"));
            //Console.WriteLine(prompt);

            // Send command and expect password or user prompt
            stream.WriteLine("su - root");
            prompt = stream.Expect(new Regex(@"([$#>:])"));
            //Console.WriteLine(prompt);

            // Check to send password
            if (prompt.Contains(":"))
            {
                // Send password
                stream.WriteLine(password);
                prompt = stream.Expect(new Regex(@"[$#>]"));
                //Console.WriteLine(prompt);
            }
        }

    }
}
