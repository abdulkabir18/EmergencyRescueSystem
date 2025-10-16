namespace Application.Common.Helpers
{
    public static class BuildUserFullName
    {
        private static string Capitalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            return char.ToUpper(input[0]) + input[1..].ToLower();
        }

        public static string BuildFullName(string firstName, string lastName)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName)) return string.Empty;

            return $"{Capitalize(firstName)} {Capitalize(lastName)}";
        }
    }
}