
public interface IProtocol { }
public struct MESSAGE : IProtocol { public string message; }
public struct CONFIRM : IProtocol { public bool isCompleted; }
public struct Personnel { public int current, maximum; }
public struct STAGE_INFO : IProtocol
{
    public uint stageSerial;
    public uint hostSerial;
    public string title;
    public string password;
    public string host;
    public string song;
    public bool isPlaying;
    public Personnel personnel;
}

public struct USER_INFO : IProtocol
{
    public string name;
    public string password;
    public int level;
    public float exp;
    public float accuracy;
    public int playCount;

    public USER_INFO( string _name, string _password )
    {
        name = _name;
        password = _password;
        level = playCount = 0;
        exp = accuracy = 0f;
    }
}

public struct CHAT_MESSAGE : IProtocol
{
    public uint   serial;
    public string nickname;
    public string message;
}