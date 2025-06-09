namespace EXE201_LinhMocStore.Util
{
    public class Commons
    {
        private static readonly Random _random = new Random();
        private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static string GenerateRandomString(int length)
        {
            return new string(Enumerable.Repeat(_chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public static string GenerateBankInfo(int length)
        {
            return "PTS" + GenerateRandomString(length);
        }
    }
}
