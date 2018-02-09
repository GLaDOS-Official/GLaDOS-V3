﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace GladosV3.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class TimeoutAttribute : PreconditionAttribute
    {
        private readonly uint _invokeLimit;
        private readonly bool _noLimitInDMs;
        private readonly bool _noLimitForAdmins;
        private readonly bool _applyPerGuild;
        private readonly TimeSpan _invokeLimitPeriod;

        private readonly Dictionary<(ulong, ulong?), CommandTimeout> _tracker =  new Dictionary<(ulong, ulong?), CommandTimeout>();

        /// <summary> Sets how often a user is allowed to use this command. </summary>
        /// <param name="times">The number of times a user may use the command within a certain period.</param>
        /// <param name="period">The amount of time since first invoke a user has until the limit is lifted.</param>
        /// <param name="measure">The scale in which the <paramref name="period"/> parameter should be measured.</param>
        /// <param name="noLimitInDMs">Set whether or not there is no limit to the command in DMs. Defaults to false.</param>
        /// <param name="noLimitForAdmins">Set whether or not there is no limit to the command for guild admins. Defaults to false.</param>
        /// <param name="applyPerGuild">Set whether or not to apply a limit per guild. Defaults to false.</param>
        public TimeoutAttribute(uint times, double period, Measure measure, bool noLimitInDMs = false,
            bool noLimitForAdmins = false, bool applyPerGuild = false)
        {
            _invokeLimit = times;
            _noLimitInDMs = noLimitInDMs;
            _noLimitForAdmins = noLimitForAdmins;
            _applyPerGuild = applyPerGuild;

            switch (measure)
            {
                case Measure.Days:
                    _invokeLimitPeriod = TimeSpan.FromDays(period);
                    break;
                case Measure.Hours:
                    _invokeLimitPeriod = TimeSpan.FromHours(period);
                    break;
                case Measure.Minutes:
                    _invokeLimitPeriod = TimeSpan.FromMinutes(period);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(measure), measure, null);
            }
        }

        /// <summary> Sets how often a user is allowed to use this command. </summary>
        /// <param name="times">The number of times a user may use the command within a certain period.</param>
        /// <param name="period">The amount of time since first invoke a user has until the limit is lifted.</param>
        /// <param name="noLimitInDMs">Set whether or not there is no limit to the command in DMs. Defaults to false.</param>
        /// <param name="noLimitForAdmins">Set whether or not there is no limit to the command for guild admins. Defaults to false.</param>
        /// <param name="applyPerGuild">Set whether or not to apply a limit per guild. Defaults to false.</param>
        public TimeoutAttribute(uint times, TimeSpan period, bool noLimitInDMs = false, bool noLimitForAdmins = false,
            bool applyPerGuild = false)
        {
            _invokeLimit = times;
            _noLimitInDMs = noLimitInDMs;
            _noLimitForAdmins = noLimitForAdmins;
            _invokeLimitPeriod = period;
            _applyPerGuild = applyPerGuild;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            if (IsOwner.CheckPermission(context).GetAwaiter().GetResult()) // bypass for owner of the bot
                return Task.FromResult(PreconditionResult.FromSuccess());
            if (_noLimitInDMs && context.Channel is IPrivateChannel)
                return Task.FromResult(PreconditionResult.FromSuccess());

            if (_noLimitForAdmins && context.User is IGuildUser gu && gu.GuildPermissions.Administrator)
                return Task.FromResult(PreconditionResult.FromSuccess());

            var now = DateTime.UtcNow;
            var key = _applyPerGuild ? (context.User.Id, context.Guild?.Id) : (context.User.Id, null);

            var timeout = (_tracker.TryGetValue(key, out var t) && ((now - t.FirstInvoke) < _invokeLimitPeriod)) ? t : new CommandTimeout(now);

            timeout.TimesInvoked++;

            if (timeout.TimesInvoked <= _invokeLimit)
            {
                _tracker[key] = timeout;
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else
                return Task.FromResult(PreconditionResult.FromError("You're currently in timeout."));
        }

        private class CommandTimeout
        {
            public uint TimesInvoked { get; set; }
            public DateTime FirstInvoke { get; }

            public CommandTimeout(DateTime timeStarted)
            {
                FirstInvoke = timeStarted;
            }
        }
    }

   public enum Measure
   {
        Days,
        Hours,
        Minutes
   }
}