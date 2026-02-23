namespace PT.Common.Security;

public static class PasswordGenerator
{
    private const string lowerCaseLetters = "abcdefghijklmnopqrstuvwxyz";
    private const string upperCaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string numbers = "1234567890";
    private const string specialChars = "!@#$%^&*()";
    private const string allChars = lowerCaseLetters + upperCaseLetters + numbers + specialChars;

    public static string GeneratePassword(int a_length)
    {
        Random random = new ();

        // Generate one character from each category
        string password = lowerCaseLetters[random.Next(lowerCaseLetters.Length)].ToString() + upperCaseLetters[random.Next(upperCaseLetters.Length)] + numbers[random.Next(numbers.Length)] + specialChars[random.Next(specialChars.Length)];

        // Generate the rest of the characters randomly from all categories
        password += new string(Enumerable.Repeat(allChars, a_length - 4)
                                         .Select(s => s[random.Next(s.Length)])
                                         .ToArray());

        // Shuffle the password so the first four characters aren't always the same categories
        return Shuffle(random, password);
    }

    /// <summary>
    /// Randomly shuffle a string
    /// Uses the Fisher-Yates algorithm, which is a simple and efficient method of shuffling arrays.
    /// </summary>
    /// <param name="a_random"></param>
    /// <param name="a_string"></param>
    /// <returns>Returns a new randomly shuffled string</returns>
    private static string Shuffle(Random a_random, string a_string)
    {
        char[] charArray = a_string.ToCharArray();
        int n = charArray.Length;
        for (int i = 0; i < n; i++)
        {
            // The following line ensures that the shuffle is unbiased.
            // Each item is equally likely to end up in any spot.
            int r = i + a_random.Next(n - i);
            char temp = charArray[r];
            charArray[r] = charArray[i];
            charArray[i] = temp;
        }

        return new string(charArray);
    }
}