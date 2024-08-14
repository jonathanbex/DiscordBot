using Discord.Commands;

namespace BotApplication.Methods
{
  public static class GuildInfoHelper
	{
		public static readonly string LootRulesMessage = @"
**Loot Rules for Raid1**

- **Softres**: MS > OS with 2 reserves. +1 for non-softressed items.
- **Main-tank**: Priority on tier and some off-pieces and Thunderfury.
- **Offtank**: Priority on some tier-pieces and shield.

**Addon Requirement**
- Download the addon **Gargul** for raids.

**Member Info**
- Remember to get your pre-raid BIS.
- Enchant your gear.
- Bring the proper consumes.
- Get the world buffs that make sense.
- Member priority on softres (still has to SR it).
- Member priority on MS > OS until +1.
- Only members can do Gbank requests.
- Try to be ready when invites are going out.

**For Sulfuron Legendary**
- You need to show me you can afford to craft it to roll for it.

**For Molten Core**
- Put HS in Kargath.

**Item Priorities (Under Construction/Evaluation)**
```markdown
Deathbringer - Onyxia (Shaman/Warr)
Empyrean Destroyer - Kazzak (Warr)
Eskhandar - Ony + WB + MC (Shaman)
Core Hound Tooth - MC (Rogue)
Brutality Blade - MC (Warr/Rogue (if sword rogue))
Hammer of the Black Anvil - (Shaman, maybe Warr)
Perdition's Blade - (Rogue Prio)
Accuria - (Rogue/Tank/Feral Tank Prio)
Quickstrike Ring - (Warr/Sham/Rogue/Feral)
Blastershot Launcher - (Warr)
Striker's Mark - (Warrior/Tank/Rogue)
Leaf - (Sinew Prio otherwise Softres/MS Hunters)
Cloak of the Shrouded Mists - (Hunter)
Puissant Cape - (Rogue/Feral)
Majordomo Cape - (Warr/Feral)
Deceit - (Rogue/Tank Prio)
```
";
		public static async Task RelayGuildInfo(SocketCommandContext context, string command)
		{
			await context.Channel.SendMessageAsync(LootRulesMessage);
		}
	}
}
