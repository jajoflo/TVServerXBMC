using System;
using System.Collections.Generic;
using System.Text;
using MPTvClient;

namespace TVServerXBMC.Commands
{
    class IsRecording : CommandHandler
    {
        public IsRecording(ConnectionHandler connection)
            : base(connection)
        {

        }
        /*
         * No arguments needed
         */
        public override void handleCommand(string command, string[] arguments, ref TvControl.User me)
        {
            bool result = TVServerConnection.IsRecording();
            Console.WriteLine("IsRecording result: " + result.ToString());
            writer.write(result.ToString());
        }

        public override string getCommandToHandle()
        {
            return "IsRecording";
        }
    }
}
