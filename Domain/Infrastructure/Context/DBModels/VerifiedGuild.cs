using System;
using System.Collections.Generic;

namespace Domain.Infrastructure.Context;

public partial class VerifiedGuild
{
    public int Id { get; set; }

    public string? GuildId { get; set; }

    public bool? Subscriber { get; set; }

    public DateTime? LastPayment { get; set; }
}
