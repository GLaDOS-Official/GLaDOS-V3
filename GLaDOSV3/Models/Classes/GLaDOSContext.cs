//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Discord;
//using Discord.Commands;
//using Discord.Rest;
//using Discord.WebSocket;

//namespace GLaDOSV3.Models.Classes
//{
//    public class GLaDOSContext : ICommandContext
//    {
//        /// <inheritdoc />
//        public IDiscordClient Client { get; }

//        /// <inheritdoc />
//        public IGuild Guild { get; }

//        /// <inheritdoc />
//        public IMessageChannel Channel { get; }

//        /// <inheritdoc />
//        public IUser User { get; }

//        /// <inheritdoc />
//        public IUserMessage Message { get; }

//        /// <summary> Indicates whether the channel that the command is executed in is a private channel. </summary>
//        public bool IsPrivate => this.Channel is IPrivateChannel;

//        /// <summary>
//        ///     Initializes a new <see cref="T:Discord.Commands.CommandContext" /> class with the provided client and message.
//        /// </summary>
//        /// <param name="client">The underlying client.</param>
//        /// <param name="msg">The underlying message.</param>
//        public GLaDOSContext(IDiscordClient client, IUserMessage msg)
//        {
//            this.Client  = client;
//            this.Guild   = msg.Channel is IGuildChannel channel ? channel.Guild : (IGuild)null;
//            this.Channel = msg.Channel;
//            this.User    = msg.Author;
//            this.Message = msg;
//        }
//        public GLaDOSContext(IDiscordClient client, IUser user, IMessageChannel channel, string message)
//        {
//            this.Client  = client;
//            this.Guild   = channel is IGuildChannel Channel ? Channel.Guild : (IGuild)null;
//            this.Channel = channel;
//            this.User    = user;
//            this.Message = new SocketUserMessage();
//        }
//    }
//}
