using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServerCronTask
{
    /// <summary>
    /// Settings class specific for the arguments accepted by this command line application
    /// Has baked in understandign of arguments to expect.
    /// </summary>
    public class ArgSettings
    {
        public const string CountArgument = "/c:";
        public const string IntervalArgument = "/i:";

        public string Url { get; private set; }
        public int Count { get; private set; }
        public int Interval { get; private set; }

        /// <summary>
        /// Initialise the setting with an args array (usually the same as that from the command line)
        /// </summary>
        /// <param name="args"></param>
        public ArgSettings(string[] args)
        {
            //Check we have some args
            if (args.Length == 0) throw new ArgumentException("Must have some arguments");

            //Set the url argument, should probably do some validation at some point in the future.
            this.Url = args[0];

            //Get the count value
            string countValueString = GetArgumentFromArray(CountArgument, args);
            //Do we have a count argument?
            if (countValueString != null)
            {
                this.Count = GetIntArgument(CountArgument, countValueString);
            }
            else
            {
                this.Count = 1;
            }

            //Get the interval value
            string intervalValueString = GetArgumentFromArray(IntervalArgument, args);
            //Do we have an interval argument?
            if (intervalValueString != null)
            {
                this.Interval = GetIntArgument(IntervalArgument, intervalValueString);
            }
            else
            {
                //Set the default here
                this.Interval = 500;
            }
        }

        /// <summary>
        /// Returns and interger if all well, other wise throws argument exception with nice messages to help user out with understanding
        /// </summary>
        /// <param name="argument">the argument, e.g. /r:</param>
        /// <param name="value">the value of the argument including argument, e.g. /r:400</param>
        /// <returns></returns>
        private int GetIntArgument(string argument, string value)
        {
            int tempInt;
            //Try and parse it, if we can't give the user a chance with a useful error message
            if (!int.TryParse(value.Replace(argument, ""), out tempInt))
                throw new ArgumentException(String.Format("Unable to parse the '{0}' argument correctly as integer, found '{1}'", argument, value), CountArgument);

            return tempInt;
        }

        /// <summary>
        /// Finds the specified argument in the args array
        /// </summary>
        /// <param name="argument">argument to look for, e.g. /r:</param>
        /// <param name="args">the args array</param>
        /// <returns></returns>
        private string GetArgumentFromArray(string argument, string[] args)
        {

            //Search for and fine the argument we wanted
            foreach (string arg in args)
            {
                if (arg.StartsWith(argument))
                    return arg;
            }

            //We didn't find it, return null
            return null;
        }

    }
}
