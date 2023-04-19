using MimeKit;

namespace MacroMail.Service.Helper;

public static class MimeMessageHelper
{
    public static byte[] Serialize(this MimeMessage mimeMessage)
    {
        using var stream = new MemoryStream();
        mimeMessage.WriteTo(stream);
        var contents = stream.ToArray();
        return contents;
    }

    public static MimeMessage Deserialize(this byte[] serializeMimeMessage)
    {
        using var stream      = new MemoryStream(serializeMimeMessage);
        var       mimeMessage = MimeMessage.Load(stream);
        return mimeMessage;
    }
}