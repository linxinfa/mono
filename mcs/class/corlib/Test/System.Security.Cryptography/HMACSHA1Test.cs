//
// HMACSHA1Test.cs - NUnit Test Cases for HMACSHA1
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography {

// References:
// a.	The Keyed-Hash Message Authentication Code (HMAC)
//	http://csrc.nist.gov/publications/fips/fips198/fips-198a.pdf
// b.	IETF RFC2202: Test Cases for HMAC-MD5 and HMAC-SHA-1
//	http://www.ietf.org/rfc/rfc2202.txt

public class HMACSHA1Test : KeyedHashAlgorithmTest {

	protected HMACSHA1 algo;

	protected override void SetUp () 
	{
		base.SetUp ();
	}

	protected override void TearDown () {}

	public void TestConstructors () 
	{
		algo = new HMACSHA1 ();
		AssertNotNull ("HMACSHA1 ()", algo);

		byte[] key = new byte [8];
		algo = new HMACSHA1 (key);
		AssertNotNull ("HMACSHA1 (key)", algo);

		try {
			algo = new HMACSHA1 (null);
			Fail ("HMACSHA1 (null) - Expected NullReferenceException but got none");
		}
		catch (NullReferenceException) {
			// well ArgumentNullException would have been more appropriate
		}
		catch (Exception e) {
			Fail ("HMACSHA1 (null) - Expected NullReferenceException but got: " + e.ToString ());
		}
	}

	public void TestInvariants () 
	{
		algo = new HMACSHA1 ();
		AssertEquals ("HMACSHA1.CanReuseTransform", true, algo.CanReuseTransform);
		AssertEquals ("HMACSHA1.CanTransformMultipleBlocks", true, algo.CanTransformMultipleBlocks);
		AssertEquals ("HMACSHA1.HashName", "SHA1", algo.HashName);
		AssertEquals ("HMACSHA1.HashSize", 160, algo.HashSize);
		AssertEquals ("HMACSHA1.InputBlockSize", 1, algo.InputBlockSize);
		AssertEquals ("HMACSHA1.OutputBlockSize", 1, algo.OutputBlockSize);
		AssertEquals ("HMACSHA1.ToString()", "System.Security.Cryptography.HMACSHA1", algo.ToString ()); 
	}

	public void TestExceptions () 
	{
		algo = new HMACSHA1 ();
		try {
			algo.HashName = "MD5";
			byte[] data = Encoding.Default.GetBytes ("MD5");
			byte[] hmac = algo.ComputeHash (data);
			Fail ("Expected InvalidCastException but got none");
		}
		catch (InvalidCastException) {
			// do nothing, this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected InvalidCastException but got " + e.ToString ());
		}
	}

	public void Check (string testName, byte[] key, byte[] data, byte[] result) 
	{
		string classTestName = "HMACSHA1-" + testName;
		CheckA (testName, key, data, result);
		CheckB (testName, key, data, result);
		CheckC (testName, key, data, result);
		CheckD (testName, key, data, result);
		CheckE (testName, key, data, result);
	}

	public void CheckA (string testName, byte[] key, byte[] data, byte[] result) 
	{
		algo = new HMACSHA1 ();
		algo.Key = key;
		byte[] hmac = algo.ComputeHash (data);
		AssertEquals (testName + "a1", result, hmac);
		AssertEquals (testName + "a2", result, algo.Hash);
	}

	public void CheckB (string testName, byte[] key, byte[] data, byte[] result) 
	{
		algo = new HMACSHA1 ();
		algo.Key = key;
		byte[] hmac = algo.ComputeHash (data, 0, data.Length);
		AssertEquals (testName + "b1", result, hmac);
		AssertEquals (testName + "b2", result, algo.Hash);
	}
	
	public void CheckC (string testName, byte[] key, byte[] data, byte[] result) 
	{
		algo = new HMACSHA1 ();
		algo.Key = key;
		MemoryStream ms = new MemoryStream (data);
		byte[] hmac = algo.ComputeHash (ms);
		AssertEquals (testName + "c1", result, hmac);
		AssertEquals (testName + "c2", result, algo.Hash);
	}

	public void CheckD (string testName, byte[] key, byte[] data, byte[] result) 
	{
		algo = new HMACSHA1 ();
		algo.Key = key;
		// LAMESPEC or FIXME: TransformFinalBlock doesn't return HashValue !
		algo.TransformFinalBlock (data, 0, data.Length);
		AssertEquals (testName + "d", result, algo.Hash);
	}

	public void CheckE (string testName, byte[] key, byte[] data, byte[] result) 
	{
		algo = new HMACSHA1 ();
		algo.Key = key;
		byte[] copy = new byte [data.Length];
		// LAMESPEC or FIXME: TransformFinalBlock doesn't return HashValue !
		for (int i=0; i < data.Length - 1; i++)
			algo.TransformBlock (data, i, 1, copy, i);
		algo.TransformFinalBlock (data, data.Length - 1, 1);
		AssertEquals (testName + "e", result, algo.Hash);
	}

	public void TestFIPS198_A1 () 
	{
		// exact 64 bytes key (no hashing - no padding)
		byte[] key =  { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 
				0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
				0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
				0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f,
				0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
				0x28, 0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f,
				0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37,
				0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f };

		byte[] fips = { 0x4f, 0x4c, 0xa3, 0xd5, 0xd6, 0x8b, 0xa7, 0xcc, 0x0a, 0x12,
				0x08, 0xc9, 0xc6, 0x1e, 0x9c, 0x5d, 0xa0, 0x40, 0x3c, 0x0a };

		byte[] data = Encoding.Default.GetBytes ("Sample #1");
		Check ("FIPS198-A1", key, data, fips);
	}

	public void TestFIPS198_A2 () 
	{
		// key < 64 bytes -> requires padding
		byte[] key =  { 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39,
				0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f, 0x40, 0x41, 0x42, 0x43 };

		byte[] fips = { 0x09, 0x22, 0xd3, 0x40, 0x5f, 0xaa, 0x3d, 0x19, 0x4f, 0x82,
				0xa4, 0x58, 0x30, 0x73, 0x7d, 0x5c, 0xc6, 0xc7, 0x5d, 0x24 };

		byte[] data = Encoding.Default.GetBytes ("Sample #2");
		Check ("FIPS198-A2", key, data, fips);
	}

	public void TestFIPS198_A3 () 
	{
		// key > 64 bytes -> requires hashing
		byte[] key =  { 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5a, 0x5b, 0x5c, 0x5d, 0x5e, 0x5f,
				0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x6b, 0x6c, 0x6d, 0x6e, 0x6f,
				0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7a, 0x7b, 0x7c, 0x7d, 0x7e, 0x7f,
				0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8a, 0x8b, 0x8c, 0x8d, 0x8e, 0x8f,
				0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9a, 0x9b, 0x9c, 0x9d, 0x9e, 0x9f,
				0xa0, 0xa1, 0xa2, 0xa3, 0xa4, 0xa5, 0xa6, 0xa7, 0xa8, 0xa9, 0xaa, 0xab, 0xac, 0xad, 0xae, 0xaf,
				0xb0, 0xb1, 0xb2, 0xb3 };

		byte[] fips = { 0xbc, 0xf4, 0x1e, 0xab, 0x8b, 0xb2, 0xd8, 0x02, 0xf3, 0xd0,
				0x5c, 0xaf, 0x7c, 0xb0, 0x92, 0xec, 0xf8, 0xd1, 0xa3, 0xaa };

		byte[] data = Encoding.Default.GetBytes ("Sample #3");
		Check ("FIPS198-A3", key, data, fips);
	}

	public void TestFIPS198_A4 () 
	{
		byte[] key =  { 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79,
				0x7a, 0x7b, 0x7c, 0x7d, 0x7e, 0x7f, 0x80, 0x81, 0x82, 0x83,
				0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8a, 0x8b, 0x8c, 0x8d,
				0x8e, 0x8f, 0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97,
				0x98, 0x99, 0x9a, 0x9b, 0x9c, 0x9d, 0x9e, 0x9f, 0xa0 };

		byte[] fips = { 0x9e, 0xa8, 0x86, 0xef, 0xe2, 0x68, 0xdb, 0xec, 0xce, 0x42,
				0x0c, 0x75, 0x24, 0xdf, 0x32, 0xe0, 0x75, 0x1a, 0x2a, 0x26 };

		byte[] data = Encoding.Default.GetBytes ("Sample #4");
		Check ("FIPS198-A4", key, data, fips);
	}

	public void TestRFC2202_TC1 () 
	{
		byte[] key =  { 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b };
		byte[] data = Encoding.Default.GetBytes ("Hi There");
		byte[] digest = { 0xb6, 0x17, 0x31, 0x86, 0x55, 0x05, 0x72, 0x64, 0xe2, 0x8b, 0xc0, 0xb6, 0xfb, 0x37, 0x8c, 0x8e, 0xf1, 0x46, 0xbe, 0x00 };
		Check ("RFC2202-TC1", key, data, digest);
	}

	public void TestRFC2202_TC2 () 
	{
		byte[] key = Encoding.Default.GetBytes ("Jefe");
		byte[] data = Encoding.Default.GetBytes ("what do ya want for nothing?");
		byte[] digest = { 0xef, 0xfc, 0xdf, 0x6a, 0xe5, 0xeb, 0x2f, 0xa2, 0xd2, 0x74, 0x16, 0xd5, 0xf1, 0x84, 0xdf, 0x9c, 0x25, 0x9a, 0x7c, 0x79 };
		Check ("RFC2202-TC2", key, data, digest);
	}

	public void TestRFC2202_TC3 () 
	{
		byte[] key = { 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa, 0xaa };
		byte[] data = new byte [50];
		for (int i = 0; i < data.Length; i++)
			data[i] = 0xdd;
		byte[] digest = { 0x12, 0x5d, 0x73, 0x42, 0xb9, 0xac, 0x11, 0xcd, 0x91, 0xa3, 0x9a, 0xf4, 0x8a, 0xa1, 0x7b, 0x4f, 0x63, 0xf1, 0x75, 0xd3 };
		Check ("RFC2202-TC3", key, data, digest);
	}

	public void TestRFC2202_TC4 () 
	{
		byte[] key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19 };
		byte[] data = new byte [50];
		for (int i = 0; i < data.Length; i++)
			data[i] = 0xcd;
		byte[] digest = { 0x4c, 0x90, 0x07, 0xf4, 0x02, 0x62, 0x50, 0xc6, 0xbc, 0x84, 0x14, 0xf9, 0xbf, 0x50, 0xc8, 0x6c, 0x2d, 0x72, 0x35, 0xda };
		Check ("RFC2202-TC4", key, data, digest);
	}

	public void TestRFC2202_TC5 () 
	{
		byte[] key = { 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c };
		byte[] data = Encoding.Default.GetBytes ("Test With Truncation");
		byte[] digest = { 0x4c, 0x1a, 0x03, 0x42, 0x4b, 0x55, 0xe0, 0x7f, 0xe7, 0xf2, 0x7b, 0xe1, 0xd5, 0x8b, 0xb9, 0x32, 0x4a, 0x9a, 0x5a, 0x04 };
		Check ("RFC2202-TC5", key, data, digest);
	}

	public void TestRFC2202_TC6 () 
	{
		byte[] key = new byte [80];
		for (int i = 0; i < key.Length; i++)
			key[i] = 0xaa;
		byte[] data = Encoding.Default.GetBytes ("Test Using Larger Than Block-Size Key - Hash Key First");
		byte[] digest = { 0xaa, 0x4a, 0xe5, 0xe1, 0x52, 0x72, 0xd0, 0x0e, 0x95, 0x70, 0x56, 0x37, 0xce, 0x8a, 0x3b, 0x55, 0xed, 0x40, 0x21, 0x12 };
		Check ("RFC2202-TC6", key, data, digest);
	}

	public void TestRFC2202_TC7 () 
	{
		byte[] key = new byte [80];
		for (int i = 0; i < key.Length; i++)
			key[i] = 0xaa;
		byte[] data = Encoding.Default.GetBytes ("Test Using Larger Than Block-Size Key and Larger Than One Block-Size Data");
		byte[] digest = { 0xe8, 0xe9, 0x9d, 0x0f, 0x45, 0x23, 0x7d, 0x78, 0x6d, 0x6b, 0xba, 0xa7, 0x96, 0x5c, 0x78, 0x08, 0xbb, 0xff, 0x1a, 0x91 };
		Check ("RFC2202-TC7", key, data, digest);
	}
}

}
