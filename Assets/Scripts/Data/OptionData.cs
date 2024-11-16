namespace TestGame.Data
{
    public enum OptionState
    {
        None,
        Close,
        Open,
        Select,
        Error,
    }
    
    public class OptionData
    {
        public string sha1;
        public OptionState state;
        public int rowIndex;
        public int colIndex;
    }
}