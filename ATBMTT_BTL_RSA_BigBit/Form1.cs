using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Policy;

namespace ATBMTT_BTL_RSA_BigBit
{
    public partial class Form1 : Form
    {
        string pathVanBanCanKy;
        string pathVanBanCanXacNhan;
        private RSA rsa;
        public Form1()
        {
            InitializeComponent();
        }
        private void showMessageBox(string message)
        {
            MessageBox.Show(message + "\nMời bạn nhập 2 số nguyên tố khác nhau P, Q",
                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void readFileDocx(string path, TextBox textBox)
        {
            Microsoft.Office.Interop.Word.Application word
                       = new Microsoft.Office.Interop.Word.Application();
            object miss = System.Reflection.Missing.Value;
            object pathObject = path;
            object readOnly = true;
            Microsoft.Office.Interop.Word.Document docs =
                word.Documents.Open(ref pathObject, ref miss, ref readOnly,
                ref miss, ref miss, ref miss, ref miss, ref miss, ref miss,
                ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss);
            string result = "";
            for (int i = 0; i < docs.Paragraphs.Count; i++)
            {
                result += docs.Paragraphs[i + 1].Range.Text.ToString();
            }
            textBox.Text = result;
        }

        private void finish(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private void buttonTaoPQNgauNhien_Click(object sender, EventArgs e)
        {
            int bitLength = 21;

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                BigInteger p = GenerateRandomPrime(bitLength, rng);
                BigInteger q = GenerateRandomPrime(bitLength, rng);

                textBoxP.Text = p.ToString();
                textBoxQ.Text = q.ToString();
            }
        }

        private BigInteger GenerateRandomPrime(int bitLength, RandomNumberGenerator rng)
        {
            BigInteger prime;
            bool isPrime = false;

            do
            {
                prime = GenerateRandomNumber(bitLength, rng);

                // Kiểm tra tính nguyên tố sử dụng phương pháp Miller-Rabin
                isPrime = IsProbablyPrime(prime, 10); // Kiểm tra với số lần lặp là 10

            } while (!isPrime);

            return prime;
        }

        private BigInteger GenerateRandomNumber(int bitLength, RandomNumberGenerator rng)
        {
            byte[] bytes = new byte[bitLength / 8];
            rng.GetBytes(bytes);
            bytes[bytes.Length - 1] &= 0x7F; // Đảm bảo bit cao nhất là 0 để đảm bảo số dương
            BigInteger number = new BigInteger(bytes);
            number |= BigInteger.One << (bitLength - 1); // Đảm bảo bit cao nhất là 1 để đảm bảo độ dài chính xác
            return number;
        }

        private bool IsProbablyPrime(BigInteger number, int iterations)
        {
            if (number == 2 || number == 3)
                return true;
            if (number < 2 || number % 2 == 0)
                return false;

            BigInteger d = number - 1;
            int s = 0;

            while (d % 2 == 0)
            {
                d /= 2;
                s++;
            }

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[1];

                for (int i = 0; i < iterations; i++)
                {
                    rng.GetBytes(bytes);
                    BigInteger a = new BigInteger(bytes);
                    a = BigInteger.Abs(a);
                    a = a % (number - 3) + 2;

                    BigInteger x = BigInteger.ModPow(a, d, number);

                    if (x == 1 || x == number - 1)
                        continue;

                    for (int r = 1; r < s; r++)
                    {
                        x = BigInteger.ModPow(x, 2, number);

                        if (x == 1)
                            return false;

                        if (x == number - 1)
                            break;
                    }

                    if (x != number - 1)
                        return false;
                }
            }

            return true;
        }

        private void buttonTaoKhoa_Click(object sender, EventArgs e)
        {
            try
            {
                BigInteger p = BigInteger.Parse(textBoxP.Text.ToString());
                BigInteger q = BigInteger.Parse(textBoxQ.Text.ToString());
                rsa = new RSA(p, q);
                if (!rsa.IndependencePQ())
                {
                    showMessageBox("P, Q phải là hai giá trị độc lập");
                    rsa = null;
                    return;
                }
                if (!rsa.PrimeNumber(p))
                {
                    showMessageBox("P không phải số nguyên tố");
                    rsa = null;
                    return;
                }
                if (!rsa.PrimeNumber(q))
                {
                    showMessageBox("Q không phải số nguyên tố");
                    rsa = null;
                    return;
                }
                if (!rsa.CheckMinN())
                {
                    showMessageBox("Hãy chọn P Q lớn hơn để chữ ký được tạo chính xác");
                    rsa = null;
                    return;
                }
                textBoxNPrivate.Text = rsa.n.ToString();
                textBoxNPublic.Text = rsa.n.ToString();
                textBoxB.Text = rsa.b.ToString();
                textBoxA.Text = rsa.a.ToString();
                textBoxNXacNhan.Text = rsa.n.ToString();
                textBoxBXacNhan.Text = rsa.b.ToString();
            }
            catch (FormatException)
            {
                textBoxP.ResetText();
                textBoxQ.ResetText();
                showMessageBox("P, Q không hợp lệ");
            }
        }
        private void buttonTaiVanBanCanKy_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFile = new OpenFileDialog())
            {
                openFile.Filter = "|*.docx;*.txt";
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    pathVanBanCanKy = openFile.FileName;
                    textBoxVanBanCanKy.Text = pathVanBanCanKy;
                    textBoxHamBam.Text = Hash.MD5Hexadecimal(pathVanBanCanKy);
                }
            }
        }
        
        private void buttonKy_Click(object sender, EventArgs e)
        {
            BigInteger a = 0, n = 0;
            try
            {
                a = BigInteger.Parse(textBoxA.Text);
                n = BigInteger.Parse(textBoxNPublic.Text);
            }
            catch
            {
                MessageBox.Show("Khóa bí mật không hợp lệ! Hãy tạo khóa!", "Waring", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (pathVanBanCanKy == null && string.IsNullOrEmpty(textBoxVanBanCanKy.Text))
            {
                MessageBox.Show("Hãy chọn file văn bản để tạo chữ ký", "Waring", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
              

            byte[] arrayHashMD5 = Hash.MD5Decimal(pathVanBanCanKy);
            List<BigInteger> signature = new List<BigInteger>();

            RSA rsa = new RSA(); // Tạo đối tượng RSA

            foreach (byte b in arrayHashMD5)
            {
                BigInteger bInt = new BigInteger(b); // Chuyển byte thành BigInteger
                BigInteger signedValue = rsa.CalculatePow(bInt, a, n); // Tính toán chữ ký
                signature.Add(signedValue);
            }

            string signatureString = string.Join("-", signature); // Chuyển danh sách chữ ký thành chuỗi ngăn cách bởi "-"
            textBoxChuKy.Text = Hash.Base64Encode(signatureString);
        }
        
        
        private void buttonLuuChuKy_Click(object sender, EventArgs e)
        {
            if (textBoxChuKy.Text.ToString().Equals(""))
            {
                MessageBox.Show("Hãy tạo chữ ký trước khi lưu!",
                "Waring", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (SaveFileDialog saveFile = new SaveFileDialog())
            {
                saveFile.Filter = "|*.txt";
                if (saveFile.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter writer = new StreamWriter(saveFile.FileName);
                    writer.Write(textBoxChuKy.Text.ToString());
                    writer.Close();
                    MessageBox.Show("Chữ ký đã được lưu tại " + saveFile.FileName,
                        "Message");
                }
            }
        }

        private void buttonTaiVanBanXacNhan_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFile = new OpenFileDialog())
            {
                openFile.Filter = "|*.docx;*.txt";
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    //pathVanBanCanXacNhan = openFile.FileName;
                    //textBoxVanBanCanXacNhan.Text = pathVanBanCanXacNhan;
                    StreamReader reader = new StreamReader(openFile.FileName);
                    textBoxVanBanCanXacNhan.Text = reader.ReadToEnd();
                    reader.Close();
                }
            }
        }

        private void buttonTaiChuKy_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFile = new OpenFileDialog())
            {
                openFile.Filter = "|*.docx;*.txt";
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    StreamReader reader = new StreamReader(openFile.FileName);
                    textBoxChuKyCanXacNhan.Text = reader.ReadToEnd();
                    reader.Close();
                }
            }
        }

        private void buttonXacNhan_Click(object sender, EventArgs e)
        {
            long bb = long.Parse(textBoxB.Text.ToString());
            long nn = long.Parse(textBoxNPublic.Text.ToString());
            long b = 0, n = 0;
            try
            {
                b = long.Parse(textBoxBXacNhan.Text.ToString());
                n = long.Parse(textBoxNXacNhan.Text.ToString());
                if (b != bb || n != nn)
                {
                    MessageBox.Show("Khoá công khai sai hoặc đã bị sửa!",
                "Waring", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Khoá công khai sai hoặc đã bị sửa!",
                "Waring", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                string[] signatureString = Hash.Base64Decode(textBoxChuKyCanXacNhan.Text.ToString()).Split('-');
                byte[] arrayHash = new byte[16];
                List<long> signature = new List<long>();
                RSA rsaConfirm = new RSA();
                for (int i = 0; i < signatureString.Length; i++)
                {
                    signature.Add(long.Parse(signatureString[i]));
                }
                if (signature.Count > 16)
                {
                    MessageBox.Show("Chữ kí không hợp lệ!",
                    "Waring", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                for (int i = 0; i < signature.Count; i++)
                {
                    arrayHash[i] = (byte)rsaConfirm.CalculatePow(signature[i], b, n);
                }
                //pathVanBanCanXacNhan= textBoxVanBanCanXacNhan.Text ;
                if (pathVanBanCanXacNhan == null && textBoxVanBanCanXacNhan.Text == null)
                {
                    MessageBox.Show("Hãy chọn file văn bản cần xác nhận hoặc nhập vào ô Văn bản cần xác nhận",
                    "Waring", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (Hash.MD5Hexadecimal(pathVanBanCanXacNhan)
                    .Equals(BitConverter.ToString(arrayHash)))
                {
                    MessageBox.Show("Chữ ký chính xác! Văn bản không có sự thay đổi!",
                        "Message");
                }
                else
                {
                    MessageBox.Show("Văn bản đã được chỉnh sửa hoặc chữ ký không chính xác!",
                        "Message");
                }
            }
            catch
            {
                MessageBox.Show("Chữ kí không hợp lệ!",
                "Waring", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonThoat2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonThoat0_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonThoat1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}