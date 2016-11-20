using System;
using System.Diagnostics.Eventing.Reader;
using System.Security;

namespace EventQuery
{
    class EventQueryExample
    {
        static void Main(string[] args)
        {
            EventQueryExample ex = new EventQueryExample();
            ex.QueryActiveLog();
            ex.QueryExternalFile();
            ex.QueryRemoteComputer();
        }

        public void QueryActiveLog()
        {
            // Query two different event logs using a structured query.
            string queryString =
                "<QueryList>" +
                "  <Query Id=\"0\" Path=\"Application\">" +
                "    <Select Path=\"Application\">" +
                "        *[System[(Level &lt;= 3) and" +
                "        TimeCreated[timediff(@SystemTime) &lt;= 86400000]]]" +
                "    </Select>" +
                "    <Suppress Path=\"Application\">" +
                "        *[System[(Level = 2)]]" +
                "    </Suppress>" +
                "    <Select Path=\"System\">" +
                "        *[System[(Level=1  or Level=2 or Level=3) and" +
                "        TimeCreated[timediff(@SystemTime) &lt;= 86400000]]]" +
                "    </Select>" +
                "  </Query>" +
                "</QueryList>";

            EventLogQuery eventsQuery = new EventLogQuery( "Application", PathType.LogName, queryString);
            EventLogReader logReader = new EventLogReader(eventsQuery);

            // Display event info
            DisplayEventAndLogInformation(logReader);

        }

        public void QueryExternalFile()
        {
            string queryString = "*[System/Level=2]"; // XPATH Query
            string eventLogLocation = @"C:\MyEvents.evtx";
            EventLogQuery eventsQuery = new EventLogQuery(eventLogLocation, PathType.FilePath, queryString);

            try
            {
                EventLogReader logReader = new EventLogReader(eventsQuery);

                // Display event info
                DisplayEventAndLogInformation(logReader);
            }
            catch (EventLogNotFoundException e)
            {
                Console.WriteLine("Could not find the external log to query! " + e.Message);
                return;
            }
        }

        public void QueryRemoteComputer()
        {
            string queryString = "*[System/Level=2]"; // XPATH Query
            SecureString pw = GetPassword();

            EventLogSession session = new EventLogSession(
                "RemoteComputerName",                               // Remote Computer
                "Domain",                                  // Domain
                "Username",                                // Username
                pw,
                SessionAuthentication.Default);

            pw.Dispose();

            // Query the Application log on the remote computer.
            EventLogQuery query = new EventLogQuery("Application", PathType.LogName, queryString);
            EventLogQuery query2 = new EventLogQuery()
            query.Session = session;

            try
            {
                EventLogReader logReader = new EventLogReader(query);

                // Display event info
                DisplayEventAndLogInformation(logReader);
            }
            catch (EventLogException e)
            {
                Console.WriteLine("Could not query the remote computer! " + e.Message);
                return;
            }
        }

        /// <summary>
        /// Displays the event information and log information on the console for 
        /// all the events returned from a query.
        /// </summary>
        private void DisplayEventAndLogInformation(EventLogReader logReader)
        {
            for (EventRecord eventInstance = logReader.ReadEvent();
                null != eventInstance; eventInstance = logReader.ReadEvent())
            {
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine("Event ID: {0}", eventInstance.Id);
                Console.WriteLine("Publisher: {0}", eventInstance.ProviderName);

                try
                {
                    Console.WriteLine("Description: {0}", eventInstance.FormatDescription());
                }
                catch (EventLogException)
                {
                    // The event description contains parameters, and no parameters were 
                    // passed to the FormatDescription method, so an exception is thrown.

                }

                // Cast the EventRecord object as an EventLogRecord object to 
                // access the EventLogRecord class properties
                EventLogRecord logRecord = (EventLogRecord)eventInstance;
                Console.WriteLine("Container Event Log: {0}", logRecord.ContainerLog);
            }
        }

        /// <summary>
        /// Read a password from the console into a SecureString
        /// </summary>
        /// <returns>Password stored in a secure string</returns>
        public static SecureString GetPassword()
        {
            SecureString password = new SecureString();
            Console.WriteLine("Enter password: ");

            // get the first character of the password
            ConsoleKeyInfo nextKey = Console.ReadKey(true);

            while (nextKey.Key != ConsoleKey.Enter)
            {
                if (nextKey.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password.RemoveAt(password.Length - 1);

                        // erase the last * as well
                        Console.Write(nextKey.KeyChar);
                        Console.Write(" ");
                        Console.Write(nextKey.KeyChar);
                    }
                }
                else
                {
                    password.AppendChar(nextKey.KeyChar);
                    Console.Write("*");
                }

                nextKey = Console.ReadKey(true);
            }

            Console.WriteLine();

            // lock the password down
            password.MakeReadOnly();
            return password;
        }
    }
}