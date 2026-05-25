namespace _002_Scripts.Data.Message
{
    public readonly struct PlayerFlagMsg
    {
        public readonly bool isJumped;
        public readonly bool isDrag;


        public PlayerFlagMsg(bool isJumped = false, bool isDrag = false)
        {
            this.isJumped = isJumped;
            this.isDrag = isDrag;
        }
    }
}