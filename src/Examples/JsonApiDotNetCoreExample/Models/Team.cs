using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCoreExample.Models
{
    public class Team : IIdentifiable<Guid>
    {
        [Id]
        public Guid TeamId { get; set; }
        public string TeamName { get; set; }
        [HasMany]
        public IEnumerable<Championship> Won { get; set; }
        [NotMapped]
        [HasManyThrough(nameof(TeamPlayers))]
        public ISet<Player> Players { get; set; }
        public ISet<TeamPlayer> TeamPlayers { get; set; }
        Guid IIdentifiable<Guid>.Id
        {
            get => TeamId;
            set => TeamId = value;
        }
        string IIdentifiable.StringId
        {
            get => TeamId.ToString();
            set => TeamId = Guid.Parse(value);
        }
    }
}
