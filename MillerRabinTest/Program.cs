using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MillerRabinTest
{
    class Program
    {
        private static readonly int[] _numberOfBits = {128, 256, 512};
        static void Main(string[] args)
        {
            var foregroundColor = Console.ForegroundColor;
            //To check algorithm
            //CheckForPrimarility(new BigInteger(32416190071));

            Console.WriteLine("Enter 0 for one number at a time and 1 to get a number");
            var answer = int.Parse(Console.ReadLine());
            if (answer==0)
            {
                while (true)
                {
                    Console.ForegroundColor = foregroundColor;
                    Console.WriteLine("Please select the number of bits to be generated. Enter:");
                    for (int i = 0; i < _numberOfBits.Length; i++)
                    {
                        Console.WriteLine($"{i} for {_numberOfBits[i]} bit key");
                    }

                    var length = _numberOfBits[int.Parse(Console.ReadLine())];
                    Console.WriteLine("You have selected number with length " + length);
                    BigInteger oddNumber = GenerateNumber(length);
                    Console.WriteLine(oddNumber);
                    CheckForPrimarility(oddNumber);
                    Console.ForegroundColor = foregroundColor;
                    Console.WriteLine("Press Enter to exit and any other key to restart");
                    switch (Console.ReadKey().Key)
                    {
                        case ConsoleKey.Enter:
                            return;
                        default:
                            continue;

                    }
                }
            }
            else
            {
                Console.WriteLine("Please select the number of bits to be generated. Enter:");
                for (int i = 0; i < _numberOfBits.Length; i++)
                {
                    Console.WriteLine($"{i} for {_numberOfBits[i]} bit key");
                }

                var length = _numberOfBits[int.Parse(Console.ReadLine())];
                BigInteger oddNumber = GenerateNumber(length);
                do
                {
                    oddNumber = GenerateNumber(length);
                    Console.WriteLine(oddNumber);

                } while (!CheckForPrimarility(oddNumber));

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Found Prime number {oddNumber}");
                Console.ForegroundColor = foregroundColor;
            }

            Console.ReadLine();

        }

        private static BigInteger GenerateNumber(int length)
        {
            byte[] bytes = new byte[length/8+1];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }
            bytes[bytes.Length - 1] &= (byte)0x7F; //force sign bit to positive
            BigInteger bigInteger = new BigInteger(bytes);
            BigInteger numberTwo = new BigInteger(2);
            //If bigInteger is not odd make it odd
            if (bigInteger % numberTwo == 0)
            {
                bigInteger += 1;
            }

            return bigInteger;
        }

        private static bool CheckForPrimarility(BigInteger numberToCalculate)
        {
            bool result;
            Console.WriteLine((result=PrimarilityUtility(numberToCalculate))
                ? "Overall success! Number is prime"
                : "Failed. Number is not prime");
            return result;
        }

        private static bool PrimarilityUtility(BigInteger numberToCalculate)
        {
            if (numberToCalculate <= 1 || numberToCalculate == 4)
                return false;
            if (numberToCalculate <= 3)
                return true;

            // Find r such that n = 2^d * r + 1  
            // for some r >= 1 
            BigInteger d = numberToCalculate - 1;

            while (d % 2 == 0)
                d /= 2;

            bool result = true;
            int outterCount = 0;
            Parallel.For(0, 10, (i, x) =>
            {
                if (!MillerRabbinTest(d, numberToCalculate))
                {
                    var oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Test #{i+1} failed");
                    Console.ForegroundColor = oldColor;
                    result = false;
                    x.Break();
                }
                else
                {
                    var oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Test #{i + 1} success!");
                    Console.ForegroundColor = oldColor;
                   
                }

            });
            return result;
        }

        static bool MillerRabbinTest(BigInteger exponent, BigInteger number)
        {

            // Pick a random number in 2 to n-2
            BigInteger a = 2 + RandomIntegerBelow(BigInteger.Subtract(number, 4));

            // Compute a^d % n 
            BigInteger x = BigInteger.ModPow(a, exponent, number);
           

            if (x == 1 || x == number - 1)
                return true;

            // Keep squaring x while one of the 
            // following doesn't happen 
            // (i) d does not reach n-1 this means number is not prime
            // (ii) (x^2) % n is not 1 this means number is not prime
            // (iii) (x^2) % n is not n-1 this means number is prime
            while (exponent != number - 1)
            {
                x = (x * x) % number;
                exponent *= 2;

                if (x == 1)
                    return false;
                if (x == number - 1)
                    return true;
            }

            // Return composite 
            return false;
        }

        public static BigInteger RandomIntegerBelow(BigInteger N)
        {
            byte[] bytes = N.ToByteArray();
            BigInteger R;
            var random = new Random();
            do
            {
                random.NextBytes(bytes);
                bytes[bytes.Length - 1] &= (byte)0x7F; //force sign bit to positive
                R = new BigInteger(bytes);
            } while (R >= N);

            return R;
        }
    }
}
