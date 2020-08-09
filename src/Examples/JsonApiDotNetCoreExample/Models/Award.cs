using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCoreExample.Models
{
    /// <summary>
    /// This has a non standard key.
    /// It referenced in a one to many relationship through PlayerAward
    /// </summary>
    public class Award : IIdentifiable<short>
    {
        [Id]
        [Key]
        public short AwardNo { get; set; }
        [Attr]
        public string AwardName { get; set; }

        [NotMapped]
        [HasManyThrough(nameof(AwardRecipients), nameof(PlayerAward.AwardNo), nameof(PlayerAward.RecipientNo))]
        public ISet<Player> Recipients { get; set; }
        public ISet<PlayerAward> AwardRecipients { get; set; }

        short IIdentifiable<short>.Id
        {
            get => AwardNo;
            set => AwardNo = value;
        }
        string IIdentifiable.StringId
        {
            get => AwardNo.ToString();
            set => AwardNo = short.Parse(value);
        }
    }
}
