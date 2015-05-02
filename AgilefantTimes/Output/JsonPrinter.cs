using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgilefantTimes.Output
{
    /// <summary>
    /// A class that helps format and print Json
    /// </summary>
    public class JsonPrinter
    {
        /// <summary>
        /// The string that the JsonPrinter uses as an Indent. Defaults
        /// to four spaces.
        /// </summary>
        public string IndentString { get; set; }

        /// <summary>
        /// The string that the JsonPrinter uses as a Newline. Defaults
        /// to '\n'
        /// </summary>
        public string NewLineString { get; set; }

        private TextWriter _writer;

        /// <summary>
        /// Creates a new JsonPrinter that writes to Console.Out
        /// </summary>
        public JsonPrinter()
            : this(Console.Out)
        {

        }

        /// <summary>
        /// Creates a new JsonPrinter 
        /// </summary>
        /// <param name="outputStream">The stream the JsonPrinter will output to</param>
        public JsonPrinter(TextWriter outputStream)
        {
            this._writer = outputStream;

            IndentString = "    ";
            NewLineString = "\n";
        }

        /// <summary>
        /// Prettifies a Json string. Note: This is likely to mangle non json strings..
        /// </summary>
        /// <param name="json">The Json to prettify</param>
        /// <param name="baseIndent">The level of indentation to start at</param>
        /// <returns>The prettified string</returns>
        public string Prettify(string json, int indent = 0)
        {
            json = json.Trim(' ', '\t', '\r', '\n');

            var beginChars = new char[]{
                '{', ',', '['
            };
            var endChars = new char[]{
                '}', ']'
            };

            var formatted = GetIndent(indent);

            for (var i = 0; i < json.Length; ++i)
            {
                var c = json[i];

                if (c == '}' || c == ']')
                    indent--;

                if (c == '{' || c == '[')
                    indent++;

                //If this is one of the ending characters its next line time
                if (endChars.Any(e => e == c))
                {
                    formatted += NewLineString + GetIndent(indent);
                }

                formatted += c;

                //If this is one of the beginnning characters its time to move onto the next line
                if (beginChars.Any(b => b == c))
                {
                    if (i < json.Length - 1)
                        formatted += NewLineString + GetIndent(indent);
                }
            }
            return formatted;
        }

        /// <summary>
        /// Prettifies and then writes a Json string to the specified output stream.
        /// Note: This is likely to mangle non json strings...
        /// </summary>
        /// <param name="json">The json to print</param>
        /// <param name="baseIndent">The base level of indentation</param>
        public void Write(string json, int baseIndent=0)
        {
            var formatted = Prettify(json, baseIndent);
            _writer.Write(formatted);
        }

        /// <summary>
        /// Prettifies and then writes a Json string and adds a trailing newline
        /// </summary>
        /// <param name="json">The json</param>
        /// <param name="baseIndent">The base indent level</param>
        public void WriteLine(string json, int baseIndent = 0)
        {
            Write(json, baseIndent);
            _writer.Write("\n");
        }

        /// <summary>
        /// Gets a string of the IndentChar repeated 
        /// 'indent' times
        /// </summary>
        /// <param name="indent">The amount to indent</param>
        /// <returns>The indentation string</returns>
        private string GetIndent(int indent)
        {
            var result = "";
            for (var i = 0; i < indent; ++i)
                result += IndentString;
            return result;
        }
    }
}
