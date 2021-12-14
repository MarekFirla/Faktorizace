using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Diagnostics;
using System.Numerics;
using System.IO;
using System.Collections;
using System.Security.Cryptography;
using System.Globalization;


namespace Faktorizace
{
    public partial class MainMenu : Form
    {

        UnicodeEncoding ByteConverter = new UnicodeEncoding();
        RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
        byte[] plaintext;
        byte[] encryptedtext;

        public MainMenu()
        {
            InitializeComponent();
        }

        private void buttonFaktorizace_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch1 = new Stopwatch();
            Stopwatch stopwatch2 = new Stopwatch();

            Console.Text = "";

            if (BigInteger.TryParse(textBoxCislo.Text, out BigInteger ZadaneCislo))
            {
                if (ZadaneCislo > 0)
                {
                    Console.Text = ("Platné číslo: " + ZadaneCislo);

                    stopwatch1.Start();
                    string fakt1 = Faktorizace1(ZadaneCislo);
                    textBoxFaktorizace1.Text = fakt1.ToString();
                    textBoxCas1.Text = stopwatch1.ElapsedMilliseconds.ToString();

                    stopwatch2.Start();
                    string fakt2 = Faktorizace2(ZadaneCislo);
                    textBoxFaktorizace2.Text = fakt2.ToString();
                    textBoxCas2.Text = stopwatch2.ElapsedMilliseconds.ToString();             
                }
                else
                {
                    Console.Text = "Cilo musí být větší než 0";
                }
            }
            else
            {
                Console.Text = ("Neplatné číslo");
            }
        }

        private string Faktorizace1(BigInteger cislo)
        {
            BigInteger zbytek = cislo;
            string fakt = "";

            for (BigInteger i = 2; i <= (DruhaOdmocnina(zbytek)); i++)
            {
                while (zbytek % i == 0)
                {                   
                    zbytek = zbytek / i;
                    fakt = fakt + i + " x ";
                }
            }
            if (zbytek > 1)
            {
                fakt += zbytek;
            }
            return fakt;
        }

        private string Faktorizace2(BigInteger cislo)
        {
            BigInteger zbytek = cislo;
            string fakt = "";

            using (StreamReader sr = new StreamReader(@"prvocisla.txt"))
            {
                string s;
                BigInteger i;
                while ((s = sr.ReadLine()) != null && (i = (BigInteger.Parse(s))) <= (DruhaOdmocnina(zbytek)))
                {                     
                        while (zbytek % i == 0)
                        {
                            zbytek = zbytek / i;
                            fakt = fakt + i + " x ";
                        }
                }

                if (zbytek > 1 && s != null) 
                {
                    fakt += zbytek;
                }

                else
                {
                    for (BigInteger j = 999983; j <= (DruhaOdmocnina(zbytek)); j++)
                    {
                        while (zbytek % j == 0)
                        {
                            zbytek = zbytek / j;
                            fakt = fakt + j + " x ";
                        }
                    }
                    if (zbytek > 1)
                    {
                        fakt += zbytek;
                    }
                  
                }
                return fakt;  
            }
        }

        static BigInteger DruhaOdmocnina(BigInteger value)
        {
            if (value <= ulong.MaxValue) // small enough for Math.Sqrt() or negative?
            {
                return (ulong)Math.Sqrt((ulong)value);
            }

            BigInteger root; // now filled with an approximate value
            int byteLen = value.ToByteArray().Length;
            if (byteLen < 128) // small enough for direct double conversion?
            {
                root = (BigInteger)Math.Sqrt((double)value);
            }
            else // large: reduce with bitshifting, then convert to double (and back)
            {
                root = (BigInteger)Math.Sqrt((double)(value >> (byteLen - 127) * 8)) << (byteLen - 127) * 4;
            }

            for (; ; )
            {
                var root2 = value / root + root >> 1;
                if ((root2 == root || root2 == root + 1) && IsSqrt(value, root)) return root;
                root = value / root2 + root2 >> 1;
                if ((root == root2 || root == root2 + 1) && IsSqrt(value, root2)) return root2;
            }
        }

        static bool IsSqrt(BigInteger value, BigInteger root)
        {
            var lowerBound = root * root;

            return value >= lowerBound && value <= lowerBound + (root << 1);
        }

        static BigInteger NahodneCislo(int PocetDesMist)
        {
            BigInteger Nahodne;
            string s = null;
            int d = 0;
            Random random = new Random();

            while (PocetDesMist > 0)
            {
                if (BigInteger.Pow(10, PocetDesMist+1) < int.MaxValue)
                {
                    Nahodne = random.Next((int)Math.Pow(10, PocetDesMist - 1), (int)(Math.Pow(10, PocetDesMist) - 1));
                    d = Nahodne.ToString().Length;
                }
                else
                {
                    Nahodne = random.Next((int)Math.Pow(10, 8), (int)(Math.Pow(10, 9) - 1));
                    d = Nahodne.ToString().Length;
                }
                PocetDesMist -= d;
                s += Nahodne.ToString();
                
            }

            Nahodne = BigInteger.Parse(s);
            return Nahodne;
        }

        private void buttonNahodnaCisla_Click(object sender, EventArgs e)
        {
            Console.Text = "";

            int PocetDesMist = 0;
            BigInteger VygenerovaneCislo = 0;

            if (int.TryParse(textBoxPocetDesMist.Text, out PocetDesMist))
            {
                if (PocetDesMist > 0)
                {
                    VygenerovaneCislo = NahodneCislo(PocetDesMist);
                    Console.Text = ("Náhodné číslo: " + VygenerovaneCislo);
                    textBoxCislo.Text = VygenerovaneCislo.ToString();
                }

                else
                {
                    Console.Text = ("Zadejte hodnotu větší než 0");
                }
            }
            else
            {
                Console.Text = ("Neplatné číslo");
            }
        }

        private void textBoxCas1_TextChanged(object sender, EventArgs e)
        {

        }

        private void buttonZasifruj_Click(object sender, EventArgs e)
        {
            Console.Text = "";
            BigInteger p;
            BigInteger q;
            BigInteger Modul;
            BigInteger HodnotaCarmichaelovyFunkce;
            int VerejnySifrovaciExponent=0;
            BigInteger SoukromySifrovaciExponent;
            BigInteger Zprava;
            BigInteger ZasifrovanaZprava;
            if (BigInteger.TryParse(textBoxp.Text, out p) && BigInteger.TryParse(textBoxq.Text, out q) && BigInteger.TryParse(textBoxZprava.Text, out Zprava))
            {
                if (p > 0 && q > 0 && Zprava > 0 && JePrvocislo(p) && JePrvocislo(q)) 
                {
                    Modul = p * q;
                    textBoxModul.Text = Modul.ToString();
                    HodnotaCarmichaelovyFunkce = NejmensiSpolecnyNasobek((p - 1),(q - 1));
                    textBoxCarmichaelovaFunkce.Text = HodnotaCarmichaelovyFunkce.ToString();

                    int Limit = 1000;
                    if (Limit > HodnotaCarmichaelovyFunkce)
                    {
                        Limit = (int)HodnotaCarmichaelovyFunkce;
                    }
                    
                    List<int> primes = GenerujPrivocisla(0, Limit);
                    for (int i = 10; i < primes.Count; i++) 
                    {
                        if (HodnotaCarmichaelovyFunkce % primes[i] != 0) 
                        {
                            VerejnySifrovaciExponent = primes[i];
                            break;
                        }
                    }

                    textBoxVerejnySifrovaciExponent.Text = VerejnySifrovaciExponent.ToString();
                    SoukromySifrovaciExponent = VypocetSoukromehoSifrovacihoExponentu(HodnotaCarmichaelovyFunkce,VerejnySifrovaciExponent);
                    textBoxSoukromySifrovaciExponent.Text = SoukromySifrovaciExponent.ToString();

                    if (Zprava<Modul)
                    {
                        ZasifrovanaZprava = BigInteger.Pow(Zprava, VerejnySifrovaciExponent) % Modul;
                        textBoxZasifrovanaZprava.Text = ZasifrovanaZprava.ToString();
                    }
                    else
                    {
                        Console.Text = ("Zprava musí být menší než modul");
                    }

                    textBoxVerejnyKlic.Text = Modul.ToString() + ", " + VerejnySifrovaciExponent.ToString();
                    textBoxSoukromyKlic.Text = Modul.ToString() + ", " + SoukromySifrovaciExponent.ToString();

                }

                else if (p > 0 && q > 0 && Zprava > 0)
                {
                    Console.Text = ("p nebo q není prvočíslo");
                }
                else
                {
                    Console.Text = ("Zadejte hodnotu větší než 0");
                }
            }
            else
            {
                Console.Text = ("Neplatné číslo");
            }
        }

        public static BitArray EratosthenovoSito(int limit)
        {
            BitArray bits = new BitArray(limit + 1, true);
            bits[0] = false;
            bits[1] = false;
            for (int i = 0; i * i <= limit; i++)
            {
                if (bits[i])
                {
                    for (int j = i * i; j <= limit; j += i)
                    {
                        bits[j] = false;
                    }
                }
            }
            return bits;
        }

        public static List<int> GenerujPrivocisla(int DolniLimit,int HorniLimit)
        {
            BitArray bits = EratosthenovoSito(HorniLimit);
            List<int> primes = new List<int>();
            for (int i = DolniLimit; i < HorniLimit; i++)
            {
                if (bits[i])
                {
                    primes.Add(i);
                }
            }
            return primes;
        }

        public static int VyberPrvekListu (List<int> Primes)
        {
            Random random = new Random();
            int Vyber = random.Next(Primes.Count);
            Vyber = Primes[Vyber];
            return Vyber;
        }

        public static BigInteger NejmensiSpolecnyNasobek(BigInteger a, BigInteger b)
        {
            BigInteger cislo1, cislo2;
            if (a > b)
            {
                cislo1 = a; cislo2 = b;
            }
            else
            {
                cislo1 = b; cislo2 = a;
            }

            for (BigInteger i = 1; i < cislo2; i++)
            {
                BigInteger nasobek = cislo1 * i;
                if (nasobek % cislo2 == 0)
                {
                    return nasobek;
                }
            }
            return cislo1 * cislo2;
        }

        public static BigInteger VypocetSoukromehoSifrovacihoExponentu(BigInteger HodnotaCarmichaelovyFunkce,int VerejnySifrovaciExponent)
        {
            BigInteger SoukromySifrovaciExponent;
            for (int i = 1; i < int.MaxValue; i++)
            {
                SoukromySifrovaciExponent = (HodnotaCarmichaelovyFunkce * i + 1) / VerejnySifrovaciExponent;
                if ((SoukromySifrovaciExponent * VerejnySifrovaciExponent) % HodnotaCarmichaelovyFunkce == 1)
                {
                    return SoukromySifrovaciExponent;
                }
            }
            return 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Console.Text = "";
            BigInteger ZasifrovanaZprava;
            BigInteger DesifrovanaZprava;
            BigInteger Modul = BigInteger.Parse(textBoxModul.Text);
            BigInteger SoukromyKlic = BigInteger.Parse(textBoxSoukromySifrovaciExponent.Text);
            if (BigInteger.TryParse(textBoxZasifrovanaZprava.Text, out ZasifrovanaZprava))
            {
                if (ZasifrovanaZprava > 0)
                {
                    DesifrovanaZprava = DesifrovaniZpravy(ZasifrovanaZprava, SoukromyKlic, Modul);
                    textBoxDesifrovanaZprava.Text = DesifrovanaZprava.ToString();

                }

                else
                {
                    Console.Text = ("Zadejte hodnotu větší než 0");
                }
            }
            else
            {
                Console.Text = ("Neplatné číslo");
            }
        }

        static BigInteger DesifrovaniZpravy(BigInteger ZasifrovanaZprava, BigInteger SoukromyExponent, BigInteger Modul)
        {
            BigInteger DesifrovanaZprava = 1;
            ZasifrovanaZprava = ZasifrovanaZprava % Modul; 

            if (ZasifrovanaZprava == 0)
                return 0; 

            while (SoukromyExponent > 0)
            {
                if ((SoukromyExponent & 1) != 0)
                    DesifrovanaZprava = (DesifrovanaZprava * ZasifrovanaZprava) % Modul;

                SoukromyExponent = SoukromyExponent >> 1;
                ZasifrovanaZprava = (ZasifrovanaZprava * ZasifrovanaZprava) % Modul;
            }
            return DesifrovanaZprava;
        }

        public static bool JePrvocislo(BigInteger cislo)
        {
            BigInteger zbytek = cislo;
            bool fakt = false;

            for (BigInteger i = 2; i <= (DruhaOdmocnina(zbytek)); i++)
            {
                while (zbytek % i == 0)
                {
                    return fakt;
                }
            }
            if (zbytek == cislo)
            {
                fakt = true;
            }
            return fakt;
        }

        private void buttonGenerujqp_Click(object sender, EventArgs e)
        {
            Console.Text = "";
            int PocetCislicp = 0;
            int PocetCislicq = 0;
            if (int.TryParse(textBoxPocetCislicp.Text, out PocetCislicp) && int.TryParse(textBoxPocetCislicq.Text, out PocetCislicq))
            {
                if (PocetCislicp > 1 && PocetCislicq > 1)
                {
                    int Limitq = (int)Math.Pow(10, PocetCislicq) - 1;
                    int Limitp = (int)Math.Pow(10, PocetCislicp) - 1;
                    List<int> primesp = GenerujPrivocisla((int)Math.Pow(10, PocetCislicp-1), Limitp);
                    List<int> primesq = GenerujPrivocisla((int)Math.Pow(10, PocetCislicq-1), Limitq);

                    Random random = new Random();
                    while (true)
                    {
                        int Vyberp = random.Next(primesp.Count);
                        int Vyberq = random.Next(primesq.Count);
                        if (Vyberp != Vyberq)
                        {
                            textBoxp.Text = primesp[Vyberp].ToString();
                            textBoxq.Text = primesq[Vyberq].ToString();
                            break;
                        }
                    }
                }
                else
                {
                    Console.Text = "Zadejte číslo větší než 1";
                }
            }
            else
            {
                Console.Text = "Neplatné číslo";
            }
        }

        private void groupBox7_Enter(object sender, EventArgs e)
        {

        }

        static public byte[] Encryption(byte[] Data, RSAParameters RSAKey, bool DoOAEPPadding)
        {
            try
            {
                byte[] encryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(RSAKey);
                    encryptedData = RSA.Encrypt(Data, DoOAEPPadding);
                    
                }
                return encryptedData;
            }
            catch (CryptographicException e)
            {
                return null;
            }
        }

        static public byte[] Decryption(byte[] Data, RSAParameters RSAKey, bool DoOAEPPadding)
        {
            try
            {
                byte[] decryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(RSAKey);
                    decryptedData = RSA.Decrypt(Data, DoOAEPPadding);
                }
                return decryptedData;
            }
            catch (CryptographicException e)
            {
                return null;
            }
        }

        private void buttonEnkryptujText_Click(object sender, EventArgs e)
        {
            plaintext = ByteConverter.GetBytes(textBoxTextKZasifrovani.Text);
            encryptedtext = Encryption(plaintext, RSA.ExportParameters(false), false);    
            string ZasifrovanaZpravaText = (ByteConverter.GetString(encryptedtext)).ToString();
           
            textBoxZasifrovanyText.Text = ZasifrovanaZpravaText;
        }

        private void buttonDekryptujText_Click(object sender, EventArgs e)
        {
            byte[] decryptedtex = Decryption(encryptedtext,RSA.ExportParameters(true), false);
            textBoxDesifrovanyText.Text = ByteConverter.GetString(decryptedtex);
        }

        private BigInteger FaktorizacePollardAlgorithm (BigInteger cislo)
        {
            BigInteger a = 2;
            int i = 2;
            BigInteger d=1;
            while(true)
            {
                a = BigInteger.Pow(a, i) % cislo;
                d = BigInteger.GreatestCommonDivisor((a - 1), cislo);

                if (d > 1)
                {
                    return d;
                    
                }
                i += 1;
            }
        }

        private string Faktorizace3(BigInteger cislo)
        {
            BigInteger cislotemp = cislo;
            string fakt = "";
            BigInteger d = 1;
            BigInteger r = 1;
            while (true)
            {
                d = FaktorizacePollardAlgorithm(cislotemp);
                fakt = fakt + d + " x ";
                r = (cislotemp / d);

                if (JePrvocislo(r))
                {
                    fakt = fakt + r;
                    break;
                }
                else
                {
                    cislotemp = r;
                }     
            }
            return fakt;
        }

        private void label26_Click(object sender, EventArgs e)
        {

        }

        private void label25_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label27_Click(object sender, EventArgs e)
        {

        }
    }
}
