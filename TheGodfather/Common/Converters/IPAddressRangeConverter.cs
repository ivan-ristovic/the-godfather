namespace TheGodfather.Common.Converters
{
    public class IPAddressRangeConverter : BaseArgumentConverter<IPAddressRange>
    {
        public override bool TryConvert(string value, out IPAddressRange result)
            => IPAddressRange.TryParse(value, out result!);
    }
}