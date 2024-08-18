using System.Security.Cryptography;
using System.Linq;
using System.Text;

public class SHA1
{
    private CustomAnswerEvent _answerEvent;
    public SHA1()
    {
        _answerEvent = OnGetHash;
        Translator.Add<GeneralProtocol>(_answerEvent);
    }

    //static string Hash(string input)
    //{
    //    using (SHA1Managed sha1 = new SHA1Managed())
    //    {
    //        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
    //        var sb = new StringBuilder(hash.Length * 2);

    //        foreach (byte b in hash)
    //        {
    //            // can be "x2" if you want lowercase
    //            sb.Append(b.ToString("X2"));
    //        }

    //        return sb.ToString();
    //    }
    //}

    private ISendData OnGetHash(System.Enum code, ISendData data)
    {
        switch (code)
        {
            case GeneralProtocol.GetHash:
                return new StringData { value = Hash(((StringData)data).value) };
        }
        return default;
    }

    static string Hash(string input)
    {
        var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(input));
        return string.Concat(hash.Select(b => b.ToString("x2")));
    }
}
