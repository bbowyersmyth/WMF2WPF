namespace WPFGDI
{
    public class LogFont : LogObject
    {
        public short Height { get; set; }
        public short Width { get; set; }
        public short Escapement { get; set; }
        public short Orientation { get; set; }
        public short Weight { get; set; }
        public bool IsItalic { get; set; }
        public bool IsUnderline { get; set; }
        public bool IsStrikeout { get; set; }
        public ushort Charset { get; set; }
        public byte OutPrecision { get; set; }
        public byte ClipPrecision { get; set; }
        public byte Quality { get; set; }
        public byte Pitch { get; set; }
        public FamilyFont Family { get; set; }
        public string FaceName { get; set; }
    }
}
