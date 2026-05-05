[System.Serializable]
public class WheelSection
{
    public string name;
    public int points;
    public int discountPct;
    public bool bonusNextPurchase;
    public SectionType type;
}

public enum SectionType
{
    Points,
    Discount,
    BonusPoints,
    SpecialPrize,
    SpinAgain,
    Nothing
}