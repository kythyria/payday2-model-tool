using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Nexus;
using NUnit.Framework;

namespace PD2ModelParser.Tests
{
    [TestFixture]
    public class MultiplicationTest
    {
        private static Matrix3D MatrixDecode(string str)
        {
            // str is a byte-for-byte dump of the Matrix3D structure, a C float[4][4] struct
            // a float is four bytes, so that's 64 bytes. With 2 characters per byte, that's 128 chars.
            Assert.AreEqual((4 * 4) * 4 * 2, str.Length);

            Matrix3D mat = new Matrix3D();

            byte[] data = new byte[64];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);
            }

            for (int i = 0; i < 16; i++)
            {
                mat[i] = BitConverter.ToSingle(data, i * 4);
            }

            return mat;
        }

        private static void TestRoundedEquals(Matrix3D expect, Matrix3D test, float err)
        {
            for (int i = 0; i < 16; i++)
            {
                float diff = Math.Abs(expect[i] - test[i]);
                if (diff > err)
                    Assert.Fail("Matrix value mismatch: {0} vs {1}", expect[i], test[i]);
            }
        }

        [Test]
        public void TestIdentityMultiplication()
        {
            Matrix3D base_ = new Matrix3D(
                -1, 1.22465e-16f, 0, 0,
                -6.16298e-32f, -4.44089e-16f, 1, 0,
                1.22465e-16f, 1, 4.44089e-16f, 0,
                1.31163e-14f, 5.32948f, 101.777f, 1
            );

            Matrix3D result = base_.MultDiesel(Matrix3D.Identity);

            Assert.AreEqual(base_, result);
        }

        [Test]
        public void TestSimpleMultiplication()
        {
            // vec11
            Matrix3D base_ = new Matrix3D(
                0.644716f, -0.205625f, 0.736247f, 0,
                0.135465f, 0.978631f, 0.154697f, 0,
                -0.752323f, 4.72831e-09f, 0.658794f, 0,
                4.06338f, -1.85149f, 2.32434f, 1
            );

            Matrix3D arg = new Matrix3D(
                0.602691f, 0.747197f, -0.280109f, 0,
                -0.538034f, 0.639738f, 0.548867f, 0,
                0.589308f, -0.180089f, 0.787581f, 0,
                -21.0668f, 36.5858f, 120.295f, 1
            );

            Matrix3D target = new Matrix3D(
                0.933074f, 0.217594f, 0.286403f, 0,
                -0.353729f, 0.699427f, 0.621029f, 0,
                -0.0651858f, -0.680775f, 0.729586f, 0,
                -16.2519f, 38.0189f, 119.972f, 1
            );

            Matrix3D result = base_.MultDiesel(arg);

            // Check if the values are within reason. Note we need to be quite sloppy (1mm), as copy+pasted
            // values from C printf don't have all the digits, and the errors add up.
            TestRoundedEquals(target, result, 0.001f);
        }

        [Test]
        public void TestDecoder()
        {
            // vec11 in1
            Matrix3D base_ = new Matrix3D(
                0.644716f, -0.205625f, 0.736247f, 0,
                0.135465f, 0.978631f, 0.154697f, 0,
                -0.752323f, 4.72831e-09f, 0.658794f, 0,
                4.06338f, -1.85149f, 2.32434f, 1
            );

            TestRoundedEquals(base_, MatrixDecode(
                "1C0C253F6D8F52BEAE7A3C3F0000000040B70A3E8C877A3FD0681E3E00000000" +
                "459840BFB076A231B7A6283F000000003107824090FDECBF07C214400000803F"
            ), 0.0001f);
        }

        public void TestInputFile(string filename)
        {
            Regex inpat1 = new Regex(@"in1 vec([\d ]\d): Matrix4{ ([\dABCDEF]{128}) }", RegexOptions.IgnoreCase);
            Regex inpat2 = new Regex(@"in2 vec([\d ]\d): Matrix4{ ([\dABCDEF]{128}) }", RegexOptions.IgnoreCase);
            Regex outpat = new Regex(@"out vec([\d ]\d): Matrix4{ ([\dABCDEF]{128}) }", RegexOptions.IgnoreCase);

            using (StreamReader file = new StreamReader(filename))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    // Leave room for comments
                    if (!line.Contains("Matrix4")) continue;

                    Match match1 = inpat1.Match(line);
                    Assert.True(match1.Success);
                    Match match2 = inpat2.Match(file.ReadLine());
                    Assert.True(match2.Success);
                    Match matchout = outpat.Match(file.ReadLine());
                    Assert.True(matchout.Success);
                    
                    // Concat the strings, otherwise it fails for some strange reason
                    Assert.AreEqual(match1.Groups[1].Value, match2.Groups[1].Value);
                    Assert.AreEqual(match1.Groups[1].Value, matchout.Groups[1].Value);

                    Matrix3D in1 = MatrixDecode(match1.Groups[2].Value);
                    Matrix3D in2 = MatrixDecode(match2.Groups[2].Value);
                    Matrix3D outm = MatrixDecode(matchout.Groups[2].Value);
                    
                    // Values aren't going to come out exactly the same unfortunately, since we're in C# the
                    // float calculations will be ever so slightly off, but not nearly enough to be a problem.
                    TestRoundedEquals(outm, in1.MultDiesel(in2), 0.0001f);
                }
            }
        }

        [Test]
        public void TestCopInputFile()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            TestInputFile(dir + "/../../../testdata/cop matrix dump.td");
        }
    }
}
