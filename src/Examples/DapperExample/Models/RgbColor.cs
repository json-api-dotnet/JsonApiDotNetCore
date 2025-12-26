using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace DapperExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ClientIdGeneration = ClientIdGenerationMode.Required)]
public sealed class RgbColor : Identifiable<int?>
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public override int? Id
    {
        get => base.Id;
        set => base.Id = value;
    }

    [HasOne]
    public Tag Tag { get; set; } = null!;

    [Attr(Capabilities = AttrCapabilities.AllowView)]
    [NotMapped]
    public byte? Red => Id == null ? null : (byte)((Id & 0xFF_0000) >> 16);

    [Attr(Capabilities = AttrCapabilities.AllowView)]
    [NotMapped]
    public byte? Green => Id == null ? null : (byte)((Id & 0x00_FF00) >> 8);

    [Attr(Capabilities = AttrCapabilities.AllowView)]
    [NotMapped]
    public byte? Blue => Id == null ? null : (byte)(Id & 0x00_00FF);

    public static RgbColor Create(byte red, byte green, byte blue)
    {
        Color color = Color.FromArgb(0xFF, red, green, blue);

        return new RgbColor
        {
            Id = color.ToArgb() & 0x00FF_FFFF
        };
    }

    protected override string? GetStringId(int? value)
    {
        return value?.ToString("X6");
    }

    protected override int? GetTypedId(string? value)
    {
        return value == null ? null : Convert.ToInt32(value, 16) & 0xFF_FFFF;
    }
}
