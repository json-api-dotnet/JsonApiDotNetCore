using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCoreExample.Models
{
    /// <summary>
    /// This has a non standard Guid key name
    /// This is to test HasMany and HasOne, using non standard field names
    /// </summary>
    public class Championship : IIdentifiable<Guid>
    {
        [Id]
        [Key]
        public Guid ChampionShipKey { get; set; }
        [Attr]
        public string ChampionShipName { get; set; }

        public Guid WinnerKey { get; set; }

        [ForeignKey(nameof(WinnerKey))]
        [HasOne]
        public Team Winner { get; set; }

        Guid IIdentifiable<Guid>.Id
        {
            get => ChampionShipKey;
            set => ChampionShipKey = value;
        }
        string IIdentifiable.StringId
        {
            get => ChampionShipKey.ToString();
            set => ChampionShipKey = Guid.Parse(value);
        }
    }
}
