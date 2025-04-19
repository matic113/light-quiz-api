namespace light_quiz_api.Services
{
    public class ShortCodeGeneratorService
    {
        private readonly ApplicationDbContext _context;
        private readonly Random _random = new Random();
        private const string AllowedCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Exclude I, O, 0, 1 for clarity

        public ShortCodeGeneratorService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Generates a unique short share code for a quiz.
        /// </summary>
        /// <param name="length">The desired length of the share code.</param>
        /// <param name="maxAttempts">Maximum attempts to generate a unique code.</param>
        /// <returns>A unique share code.</returns>
        /// <exception cref="Exception">Thrown if a unique code could not be generated within maxAttempts.</exception>
        public async Task<string> GenerateUniqueCodeAsync(int length = 8, int maxAttempts = 10)
        {
            for (int i = 0; i < maxAttempts; i++)
            {
                string code = GenerateRandomCode(length);

                // Check if a quiz with this code already exists
                bool exists = await _context.Quizzes.AnyAsync(q => q.ShortCode == code);

                if (!exists)
                {
                    return code; // Found a unique code!
                }

                // If it exists, loop and try again
            }

            // If loop finishes without finding a unique code, something is wrong (e.g., database is full of codes)
            throw new Exception($"Could not generate a unique share code within {maxAttempts} attempts. Consider increasing length or attempts.");
        }

        /// <summary>
        /// Generates a random code string of the specified length.
        /// </summary>
        private string GenerateRandomCode(int length)
        {
            char[] code = new char[length];
            for (int i = 0; i < length; i++)
            {
                code[i] = AllowedCharacters[_random.Next(AllowedCharacters.Length)];
            }
            return new string(code);
        }
    }

}
