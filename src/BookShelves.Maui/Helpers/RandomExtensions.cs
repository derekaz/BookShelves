﻿namespace BookShelves.Maui.Helpers;

public static class RandomExtensions
{
    public static void Shuffle<T>(this Random rng, T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = rng.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }

    public const string AllowableRandomStringCharacters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz#@$^*()";

    public static IEnumerable<string> NextStrings(
        this Random rnd,
        string allowedChars,
        (int Min, int Max) length,
        int count)
    {
        ISet<string> usedRandomStrings = new HashSet<string>();
        (int min, int max) = length;
        char[] chars = new char[max];
        int setLength = allowedChars.Length;

        while (count-- > 0)
        {
            int stringLength = rnd.Next(min, max + 1);

            for (int i = 0; i < stringLength; ++i)
            {
                chars[i] = allowedChars[rnd.Next(setLength)];
            }

            string randomString = new string(chars, 0, stringLength);

            if (usedRandomStrings.Add(randomString))
            {
                yield return randomString;
            }
            else
            {
                count++;
            }
        }
    }
}
