namespace BlazorBoilerplate.Shared.Extensions
{
    public static class GuidUtil
    {
        public static string ToCompressedString(this Guid guid)
        {
            return Convert.ToBase64String(guid.ToByteArray())
                .Substring(0, 22)
                .Replace('+', '-')
                .Replace('/', '_');
        }

        public static Guid FromCompressedString(string compressedGuid)
        {
            string base64 = compressedGuid
                .Replace('_', '/')
                .Replace('-', '+')
                + "==";

            return new Guid(Convert.FromBase64String(base64));
        }
    }
}
