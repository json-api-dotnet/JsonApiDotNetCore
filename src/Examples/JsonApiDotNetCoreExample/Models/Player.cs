using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCoreExample.Models
{
    public class Player: IIdentifiable<Guid>
    {
        [Id]
        public Guid PlayerId { get; set; }

        [Attr]
        public string PlayerName { get; set; }

        [NotMapped]
        [HasManyThrough(nameof(PlayerTeams))]
        public ISet<Team> Teams { get; set; }
        public ISet<TeamPlayer> PlayerTeams { get; set; }

        [NotMapped]
        [HasManyThrough(nameof(PlayerAwards), nameof(PlayerAward.RecipientNo), nameof(PlayerAward.AwardNo))]
        public ISet<Award> Awards { get; set; }
        public ISet<PlayerAward> PlayerAwards { get; set; }

        Guid IIdentifiable<Guid>.Id
        {
            get => PlayerId;
            set => PlayerId = value;
        }
        string IIdentifiable.StringId
        {
            get => PlayerId.ToString();
            set => PlayerId = Guid.Parse(value);
        }
    }
}
