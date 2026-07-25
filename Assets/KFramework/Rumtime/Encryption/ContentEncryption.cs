using System;
using System.IO;
using System.Text;

public static class ContentEncryption
{
    public static string Encode(string data)
    {
        data = Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
        data = BinaryEncode(data);
        data = Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
        return data;
    }
    
    public static string Decode(string data)
    {
        data = Encoding.UTF8.GetString(Convert.FromBase64String(data));
        data = BinaryDecode(data);
        data = Encoding.UTF8.GetString(Convert.FromBase64String(data));
        return data;
    }
    
    private static string BinaryEncode(string data)
    {
        MemoryStream mStream = new MemoryStream(); 
        BinaryWriter sw = new BinaryWriter(mStream, Encoding.UTF8); 
        sw.Write(data);
        data = Convert.ToBase64String(mStream.ToArray());
        sw.Close();
        mStream.Close();
        return data;
    }

    private static string BinaryDecode(string data)
    {
        MemoryStream mStream = new MemoryStream(Convert.FromBase64String(data)); 
        BinaryReader br = new BinaryReader(mStream, Encoding.UTF8); 
        data = br.ReadString();
        br.Close();
        mStream.Close();
        return data;
    }

}