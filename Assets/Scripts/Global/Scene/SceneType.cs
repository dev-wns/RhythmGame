
// Build Index
public enum SceneType : int
{
    FreeStyle = 1, Game, Result, // SiglePlay 
    Lobby, Stage,                // MultiPlay
};

public enum SceneAction : byte
{
    Main, Exit,                                                    // Public
    Search, Comment, GameOption, SystemOption, KeySetting, ReLoad, // FreeStyle
    Pause, GameOver,                                               // InGame
    Login, CreateStage,                                            // MultiPlay
}