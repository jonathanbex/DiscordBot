﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BotApplication.Methods
{
	public static class ClearingHelper
	{
		public static async Task Clear(SocketCommandContext context, string command)
		{
			// Example: Check if the user has the "Manage Messages" permission
			if (context.Guild == null || context.User is not SocketGuildUser guildUser)
			{
				return;
			}

			var channel = context.Channel as ITextChannel;

			// Ensure the channel is a text channel
			if (channel == null)
			{

				return;
			}

			// Check if the user has the "Manage Messages" permission in the specific channel
			var permissions = guildUser.GetPermissions(channel);

			if (!permissions.ManageMessages)
			{

				return;
			}

			var parts = command.Split(' ');
			if (parts.Length > 1 && int.TryParse(parts[1], out var quantity))
			{
				if (quantity > 0 && quantity <= 100)
				{
					var messages = await context.Channel.GetMessagesAsync(quantity + 1).FlattenAsync();

					var twoWeeksAgo = DateTimeOffset.UtcNow.AddDays(-14);
					var recentMessages = messages.Where(m => m.CreatedAt > twoWeeksAgo).ToList();

					var messagesToDelete = recentMessages.Count();
					var messageChunks = recentMessages.Chunk(100); // Adjust chunk size if necessary

					foreach (var chunk in messageChunks)
					{
						await (context.Channel as ITextChannel).DeleteMessagesAsync(chunk);
					}

					var confirmationMessage = await context.Channel.SendMessageAsync($"{messagesToDelete} messages deleted!");
					await Task.Delay(TimeSpan.FromSeconds(5));
					await confirmationMessage.DeleteAsync();
				}
				else
				{
					var invalidQuantityMessage = await context.Channel.SendMessageAsync("Please specify a number between 1 and 100.");

					await Task.Delay(TimeSpan.FromSeconds(5));
					await invalidQuantityMessage.DeleteAsync();
				}
			}
		}
	}
}