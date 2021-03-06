using System;
using System.Collections.Generic;
using System.Text;
using MPTvClient;

namespace TVServerXBMC.Commands
{
    class ListChannels : CommandHandler
    {
        public ListChannels(ConnectionHandler connection)
            : base(connection)
        {

        }

        public override void handleCommand(string command, string[] arguments, ref TvControl.User me)
        {
            String group;
            List<string> results = new List<string>();

            if(arguments != null)
            {   //we want to list all channels in group arg[0]
                group = arguments[0];
            } else {
                group = "";
            }

            List<ChannelInfo> channels = TVServerConnection.getChannels(group);
            foreach(ChannelInfo c in channels)
            {
                results.Add(writer.makeItemSmart(c));
                Console.WriteLine("CHANNEL : " + c.channelID + ";" + c.name + ";" + c.isScrambled.ToString());
            }

            writer.writeList(results);
        }

        public override string getCommandToHandle()
        {
            return "ListChannels";
        }
    }
}
